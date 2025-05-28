#nullable enable

using DbLocator.Db;
using DbLocator.Utilities;
using FluentValidation;
using Microsoft.EntityFrameworkCore;

namespace DbLocator.Features.Connections.DeleteConnection;

internal record DeleteConnectionCommand(int ConnectionId);

internal sealed class DeleteConnectionCommandValidator : AbstractValidator<DeleteConnectionCommand>
{
    internal DeleteConnectionCommandValidator()
    {
        RuleFor(command => command.ConnectionId)
            .GreaterThan(0)
            .WithMessage("Connection Id must be greater than 0.");
    }
}

internal class DeleteConnectionHandler(
    IDbContextFactory<DbLocatorContext> dbContextFactory,
    DbLocatorCache? cache = null
)
{
    private readonly IDbContextFactory<DbLocatorContext> _dbContextFactory = dbContextFactory;
    private readonly DbLocatorCache? _cache = cache;

    public async Task Handle(
        DeleteConnectionCommand request,
        CancellationToken cancellationToken = default
    )
    {
        await new DeleteConnectionCommandValidator().ValidateAndThrowAsync(
            request,
            cancellationToken
        );

        await using var dbContext = _dbContextFactory.CreateDbContext();

        var connection = await dbContext
            .Set<ConnectionEntity>()
            .FirstOrDefaultAsync(c => c.ConnectionId == request.ConnectionId, cancellationToken);

        if (connection == null)
            throw new KeyNotFoundException($"Connection with ID {request.ConnectionId} not found.");

        dbContext.Set<ConnectionEntity>().Remove(connection);
        await dbContext.SaveChangesAsync(cancellationToken);

        if (_cache != null)
            await _cache.Remove("connections");
    }
}

#nullable disable
