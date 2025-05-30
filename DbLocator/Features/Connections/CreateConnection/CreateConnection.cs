#nullable enable

using DbLocator.Db;
using DbLocator.Utilities;
using FluentValidation;
using Microsoft.EntityFrameworkCore;

namespace DbLocator.Features.Connections.CreateConnection;

internal record CreateConnectionCommand(int TenantId, int DatabaseId);

internal sealed class CreateConnectionCommandValidator : AbstractValidator<CreateConnectionCommand>
{
    internal CreateConnectionCommandValidator()
    {
        RuleFor(command => command.TenantId)
            .GreaterThan(0)
            .WithMessage("Tenant ID must be greater than zero");
        RuleFor(command => command.DatabaseId)
            .GreaterThan(0)
            .WithMessage("Database ID must be greater than zero");
    }
}

internal class CreateConnectionHandler(
    IDbContextFactory<DbLocatorContext> contextFactory,
    DbLocatorCache? cache = null
)
{
    private readonly IDbContextFactory<DbLocatorContext> _contextFactory = contextFactory;
    private readonly DbLocatorCache? _cache = cache;

    public async Task<int> Handle(
        CreateConnectionCommand request,
        CancellationToken cancellationToken = default
    )
    {
        await new CreateConnectionCommandValidator().ValidateAndThrowAsync(
            request,
            cancellationToken
        );

        await using var context = await _contextFactory.CreateDbContextAsync(cancellationToken);
        await ValidateRequest(request, context, cancellationToken);

        var connection = new ConnectionEntity
        {
            TenantId = request.TenantId,
            DatabaseId = request.DatabaseId
        };

        await context.Set<ConnectionEntity>().AddAsync(connection, cancellationToken);
        await context.SaveChangesAsync(cancellationToken);

        if (_cache != null)
            await _cache.Remove("connections");

        return connection.ConnectionId;
    }

    private static async Task ValidateRequest(
        CreateConnectionCommand request,
        DbLocatorContext context,
        CancellationToken cancellationToken
    )
    {
        var tenantExists = await context
            .Set<TenantEntity>()
            .AnyAsync(t => t.TenantId == request.TenantId, cancellationToken);

        if (!tenantExists)
            throw new KeyNotFoundException($"Tenant with ID {request.TenantId} not found");

        var databaseExists = await context
            .Set<DatabaseEntity>()
            .AnyAsync(d => d.DatabaseId == request.DatabaseId, cancellationToken);

        if (!databaseExists)
            throw new KeyNotFoundException($"Database with ID {request.DatabaseId} not found");

        var connectionExists = await context
            .Set<ConnectionEntity>()
            .AnyAsync(
                c => c.TenantId == request.TenantId && c.DatabaseId == request.DatabaseId,
                cancellationToken
            );

        if (connectionExists)
            throw new ArgumentException(
                $"Connection already exists between tenant with ID {request.TenantId} and database with ID {request.DatabaseId}"
            );
    }
}
