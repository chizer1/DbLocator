using DbLocator.Db;
using DbLocator.Domain;
using DbLocator.Utilities;
using FluentValidation;
using Microsoft.EntityFrameworkCore;

namespace DbLocator.Features.Databases;

internal record UpdateDatabaseCommand(
    int DatabaseId,
    string DatabaseName,
    int? DatabaseServerId,
    byte? DatabaseTypeId,
    Status? DatabaseStatus,
    bool? UseTrustedConnection
// bool UpdateDatabase = false
);

internal sealed class UpdateDatabaseCommandValidator : AbstractValidator<UpdateDatabaseCommand>
{
    internal UpdateDatabaseCommandValidator()
    {
        RuleFor(x => x.DatabaseId).NotNull().WithMessage("Database Id is required.");

        RuleFor(x => x.DatabaseName)
            .MaximumLength(50)
            .WithMessage("Database Name cannot be more than 50 characters.")
            .Matches(@"^[a-zA-Z0-9_]+$")
            .WithMessage("Database Name can only contain letters, numbers, and underscores.");
    }
}

internal class UpdateDatabase(
    IDbContextFactory<DbLocatorContext> dbContextFactory,
    DbLocatorCache cache
)
{
    internal async Task Handle(UpdateDatabaseCommand command)
    {
        await new UpdateDatabaseCommandValidator().ValidateAndThrowAsync(command);

        await using var dbContext = dbContextFactory.CreateDbContext();

        var databaseEntity =
            await dbContext
                .Set<DatabaseEntity>()
                .FirstOrDefaultAsync(d => d.DatabaseId == command.DatabaseId)
            ?? throw new InvalidOperationException(
                $"Database Id '{command.DatabaseId}' not found."
            );

        if (
            command.DatabaseServerId.HasValue
            && !await dbContext
                .Set<DatabaseServerEntity>()
                .AnyAsync(ds => ds.DatabaseServerId == command.DatabaseServerId.Value)
        )
            throw new KeyNotFoundException(
                $"Database Server Id '{command.DatabaseServerId}' not found."
            );

        // if (
        //     command.DatabaseTypeId.HasValue
        //     && !await dbContext
        //         .Set<DatabaseTypeEntity>()
        //         .AnyAsync(dt => dt.DatabaseTypeId == command.DatabaseTypeId.Value)
        // )
        //     throw new KeyNotFoundException(
        //         $"Database Type Id '{command.DatabaseTypeId}' not found."
        //     );

        var oldDatabaseName = databaseEntity.DatabaseName;

        if (!string.IsNullOrEmpty(command.DatabaseName))
            databaseEntity.DatabaseName = command.DatabaseName;

        if (command.DatabaseServerId.HasValue)
            databaseEntity.DatabaseServerId = command.DatabaseServerId.Value;

        if (command.DatabaseTypeId.HasValue)
            databaseEntity.DatabaseTypeId = command.DatabaseTypeId.Value;

        if (command.DatabaseStatus.HasValue)
            databaseEntity.DatabaseStatusId = (byte)command.DatabaseStatus.Value;

        if (command.UseTrustedConnection.HasValue)
            databaseEntity.UseTrustedConnection = command.UseTrustedConnection.Value;

        dbContext.Update(databaseEntity);
        await dbContext.SaveChangesAsync();

        // if (oldDatabaseName != command.DatabaseName && !string.IsNullOrEmpty(command.DatabaseName))
        // {
        //     var oldDbName = Sql.SanitizeSqlIdentifier(oldDatabaseName);
        //     var newDbName = Sql.SanitizeSqlIdentifier(command.DatabaseName);

        //     await Sql.ExecuteSqlCommandAsync(
        //         dbContext,
        //         $"alter database [{oldDbName}] modify name = [{newDbName}]",
        //         databaseEntity.DatabaseServer.IsLinkedServer,
        //         databaseEntity.DatabaseServer.DatabaseServerHostName
        //     );
        // }

        cache?.Remove("databases");
        cache?.Remove("connections");

        // TODO: Make this more specific
        cache?.TryClearConnectionStringFromCache(databaseTypeId: databaseEntity.DatabaseTypeId);
    }
}
