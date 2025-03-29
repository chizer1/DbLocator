using DbLocator.Db;
using FluentValidation;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Distributed;

namespace DbLocator.Features.Connections;

internal record DeleteConnectionCommand(int ConnectionId);

internal sealed class DeleteConnectionCommandValidator : AbstractValidator<DeleteConnectionCommand>
{
    internal DeleteConnectionCommandValidator()
    {
        RuleFor(command => command.ConnectionId)
            .NotEmpty()
            .WithMessage("Connection Id is required.");
    }
}

internal class DeleteConnection(
    IDbContextFactory<DbLocatorContext> dbContextFactory,
    IDistributedCache cache
)
{
    internal async Task Handle(DeleteConnectionCommand command)
    {
        await new DeleteConnectionCommandValidator().ValidateAndThrowAsync(command);

        await using var dbContext = dbContextFactory.CreateDbContext();

        var connection =
            await dbContext
                .Set<ConnectionEntity>()
                .FirstOrDefaultAsync(c => c.ConnectionId == command.ConnectionId)
            ?? throw new KeyNotFoundException(
                $"Connection with Id {command.ConnectionId} not found."
            );

        dbContext.Set<ConnectionEntity>().Remove(connection);
        await dbContext.SaveChangesAsync();

        cache?.Remove("connections");
    }
}
