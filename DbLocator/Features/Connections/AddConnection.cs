using DbLocator.Db;
using FluentValidation;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Distributed;

namespace DbLocator.Features.Connections;

internal record AddConnectionCommand(int TenantId, int DatabaseId);

internal sealed class AddConnectionCommandValidator : AbstractValidator<AddConnectionCommand>
{
    internal AddConnectionCommandValidator()
    {
        RuleFor(command => command.TenantId).NotEmpty().WithMessage("Tenant Id is required.");
        RuleFor(command => command.DatabaseId).NotEmpty().WithMessage("Database Id is required.");
    }
}

internal class AddConnection(
    IDbContextFactory<DbLocatorContext> dbContextFactory,
    IDistributedCache cache
)
{
    internal async Task<int> Handle(AddConnectionCommand command)
    {
        await new AddConnectionCommandValidator().ValidateAndThrowAsync(command);

        await using var dbContext = dbContextFactory.CreateDbContext();

        await Validation(command, dbContext);

        var connection = new ConnectionEntity
        {
            TenantId = command.TenantId,
            DatabaseId = command.DatabaseId
        };

        await dbContext.Set<ConnectionEntity>().AddAsync(connection);
        await dbContext.SaveChangesAsync();

        cache?.Remove("connections");

        return connection.ConnectionId;
    }

    private static async Task Validation(AddConnectionCommand command, DbLocatorContext dbContext)
    {
        var tenantExists = await dbContext
            .Set<TenantEntity>()
            .AnyAsync(t => t.TenantId == command.TenantId);

        if (!tenantExists)
            throw new KeyNotFoundException($"Tenant with Id '{command.TenantId}' not found.");

        var databaseExists = await dbContext
            .Set<DatabaseEntity>()
            .AnyAsync(d => d.DatabaseId == command.DatabaseId);

        if (!databaseExists)
            throw new KeyNotFoundException($"Database with Id '{command.DatabaseId}' not found.");

        var connectionExists = await dbContext
            .Set<ConnectionEntity>()
            .AnyAsync(c => c.TenantId == command.TenantId && c.DatabaseId == command.DatabaseId);

        if (connectionExists)
            throw new InvalidOperationException("Connection already exists.");

        var connectionOfSameTypeExists = await dbContext
            .Set<ConnectionEntity>()
            .AnyAsync(c =>
                c.TenantId == command.TenantId && c.Database.DatabaseTypeId == command.DatabaseId
            );

        if (connectionOfSameTypeExists)
            throw new InvalidOperationException("Connection of same type already exists.");
    }
}
