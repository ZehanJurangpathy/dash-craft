using DashCraft.Application.Common.Behaviors;
using FluentValidation;
using Mediator;

namespace DashCraft.Application.Tests.Common.Behaviors;

public sealed class ValidationBehaviorTests
{
    public sealed record PingCommand(string Name) : ICommand<string>;

    private sealed class PingCommandValidator : AbstractValidator<PingCommand>
    {
        public PingCommandValidator() => RuleFor(c => c.Name).NotEmpty();
    }

    private sealed class RecordingNext
    {
        public bool Called { get; private set; }

        public ValueTask<string> Invoke(PingCommand message, CancellationToken cancellationToken)
        {
            Called = true;
            return ValueTask.FromResult($"pong:{message.Name}");
        }
    }

    [Fact]
    public async Task Invalid_message_throws_and_does_not_call_next()
    {
        var behavior = new ValidationBehavior<PingCommand, string>([new PingCommandValidator()]);
        var next = new RecordingNext();

        var ex = await Should.ThrowAsync<ValidationException>(
            behavior.Handle(new PingCommand(""), next.Invoke, TestContext.Current.CancellationToken).AsTask());

        ex.Errors.ShouldContain(e => e.PropertyName == nameof(PingCommand.Name));
        next.Called.ShouldBeFalse();
    }

    [Fact]
    public async Task Valid_message_returns_the_next_result()
    {
        var behavior = new ValidationBehavior<PingCommand, string>([new PingCommandValidator()]);
        var next = new RecordingNext();

        var result = await behavior.Handle(new PingCommand("dash"), next.Invoke, TestContext.Current.CancellationToken);

        result.ShouldBe("pong:dash");
        next.Called.ShouldBeTrue();
    }

    [Fact]
    public async Task No_validators_calls_next()
    {
        var behavior = new ValidationBehavior<PingCommand, string>([]);
        var next = new RecordingNext();

        await behavior.Handle(new PingCommand(""), next.Invoke, TestContext.Current.CancellationToken);

        next.Called.ShouldBeTrue();
    }
}
