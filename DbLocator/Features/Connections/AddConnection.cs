using DbLocator.Db;
using DbLocator.Utilities;
using FluentValidation;
using Microsoft.EntityFrameworkCore;

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
    DbLocatorCache cache
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
            throw new KeyNotFoundException($"Tenant with ID {command.TenantId} not found.");

        var databaseExists = await dbContext
            .Set<DatabaseEntity>()
            .AnyAsync(d => d.DatabaseId == command.DatabaseId);

        if (!databaseExists)
            throw new KeyNotFoundException($"Database with ID {command.DatabaseId} not found.");

        var connectionExists = await dbContext
            .Set<ConnectionEntity>()
            .AnyAsync(c => c.TenantId == command.TenantId && c.DatabaseId == command.DatabaseId);

        if (connectionExists)
            throw new ArgumentException(
                $"Connection already exists for tenant ID {command.TenantId} and database ID {command.DatabaseId}."
            );
    }
}
