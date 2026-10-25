using DashCraft.Api.ErrorHandling;
using FluentValidation;
using FluentValidation.Results;
using Microsoft.AspNetCore.Http;

namespace DashCraft.Api.Tests.ErrorHandling;

public sealed class ValidationExceptionHandlerTests
{
    [Fact]
    public async Task Validation_exception_is_written_as_400_problem_details_with_errors()
    {
        var context = ProblemDetailsTestHost.CreateContext();
        var exception = new ValidationException(
        [
            new ValidationFailure("Name", "Name is required."),
            new ValidationFailure("Name", "Name is too short."),
        ]);

        var handled = await new ValidationExceptionHandler().TryHandleAsync(context, exception, TestContext.Current.CancellationToken);

        handled.ShouldBeTrue();
        context.Response.StatusCode.ShouldBe(StatusCodes.Status400BadRequest);
        var body = ProblemDetailsTestHost.ReadBody(context);
        body.GetProperty("title").GetString().ShouldBe("One or more validation errors occurred.");
        var nameErrors = body.GetProperty("errors").GetProperty("Name").EnumerateArray().Select(e => e.GetString()).ToList();
        nameErrors.ShouldBe(["Name is required.", "Name is too short."]);
    }

    [Fact]
    public async Task Other_exceptions_are_not_handled()
    {
        var context = ProblemDetailsTestHost.CreateContext();

        var handled = await new ValidationExceptionHandler().TryHandleAsync(
            context, new InvalidOperationException("boom"), TestContext.Current.CancellationToken);

        handled.ShouldBeFalse();
    }
}
