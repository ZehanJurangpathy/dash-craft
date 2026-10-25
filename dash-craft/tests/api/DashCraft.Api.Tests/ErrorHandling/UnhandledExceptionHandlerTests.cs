using DashCraft.Api.ErrorHandling;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging.Abstractions;

namespace DashCraft.Api.Tests.ErrorHandling;

public sealed class UnhandledExceptionHandlerTests
{
    private const string SensitiveMessage = "Login failed for user 'sa' on Server=secret-host";

    [Fact]
    public async Task Production_response_hides_exception_details()
    {
        var context = ProblemDetailsTestHost.CreateContext(Environments.Production);

        var handled = await CreateHandler(context).TryHandleAsync(
            context, new InvalidOperationException(SensitiveMessage), TestContext.Current.CancellationToken);

        handled.ShouldBeTrue();
        context.Response.StatusCode.ShouldBe(StatusCodes.Status500InternalServerError);
        var body = ProblemDetailsTestHost.ReadBody(context);
        body.GetProperty("title").GetString().ShouldBe("Something went wrong on our side. Please try again.");
        body.TryGetProperty("detail", out _).ShouldBeFalse();
        body.TryGetProperty("traceId", out _).ShouldBeTrue();
        body.GetRawText().ShouldNotContain("secret-host");
    }

    [Fact]
    public async Task Development_response_includes_the_exception_message()
    {
        var context = ProblemDetailsTestHost.CreateContext(Environments.Development);

        await CreateHandler(context).TryHandleAsync(
            context, new InvalidOperationException("boom"), TestContext.Current.CancellationToken);

        ProblemDetailsTestHost.ReadBody(context).GetProperty("detail").GetString().ShouldBe("boom");
    }

    private static UnhandledExceptionHandler CreateHandler(HttpContext context) =>
        new((IHostEnvironment)context.RequestServices.GetService(typeof(IHostEnvironment))!,
            NullLogger<UnhandledExceptionHandler>.Instance);
}
