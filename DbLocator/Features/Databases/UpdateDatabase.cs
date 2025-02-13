using DbLocator.Db;
using DbLocator.Domain;
using DbLocator.Utilities;
using FluentValidation;
using Microsoft.EntityFrameworkCore;

namespace DbLocator.Features.Databases;

internal record UpdateDatabaseCommand(
    int DatabaseId,
    string DatabaseName,
    string DatabaseUser,
    string DatabaseUserPassword,
    int? DatabaseServerId,
    byte? DatabaseTypeId,
    Status? DatabaseStatus,
    bool? UseTrustedConnection
);

internal sealed class UpdateDatabaseCommandValidator : AbstractValidator<UpdateDatabaseCommand>
{
    internal UpdateDatabaseCommandValidator()
    {
        RuleFor(x => x.DatabaseName)
            .MaximumLength(50)
            .WithMessage("Database Name cannot be more than 50 characters.")
            .Matches(@"^\S*$")
            .WithMessage("Database Name cannot contain spaces.");
        RuleFor(x => x.DatabaseId).NotNull().WithMessage("Database Id is required.");

        RuleFor(x => x.DatabaseName)
            .NotEmpty()
            .WithMessage("Database Name is required.")
            .MaximumLength(50)
            .WithMessage("Database Name cannot be more than 50 characters.")
            .Matches(@"^\S*$")
            .WithMessage("Database Name cannot contain spaces.");

        // optional parameter
        // RuleFor(x => x.DatabaseUserPassword)
        //     .MinimumLength(10)
        //     .WithMessage("Database User Password must be at least 10 characters long.")
        //     .Matches(@"[A-Z]")
        //     .WithMessage("Database User Password must contain at least one uppercase letter.")
        //     .Matches(@"[0-9]")
        //     .WithMessage("Database User Password must contain at least one number.")
        //     .Matches(@"[\W_]")
        //     .WithMessage("Database User Password must contain at least one special character.")
        //     .MaximumLength(50);

        // optional parameter
        RuleFor(x => x.DatabaseUser)
            .MaximumLength(50)
            .WithMessage("Database User cannot be more than 50 characters.")
            .Matches(@"^\S*$")
            .WithMessage("Database User cannot contain spaces.");
    }
}

internal class UpdateDatabase(
    IDbContextFactory<DbLocatorContext> dbContextFactory,
    Encryption encryption
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

        if (
            command.DatabaseTypeId.HasValue
            && !await dbContext
                .Set<DatabaseTypeEntity>()
                .AnyAsync(dt => dt.DatabaseTypeId == command.DatabaseTypeId.Value)
        )
            throw new KeyNotFoundException(
                $"Database Type Id '{command.DatabaseTypeId}' not found."
            );

        var oldDatabaseName = databaseEntity.DatabaseName;
        var oldDatabaseUser = databaseEntity.DatabaseUser;

        if (!string.IsNullOrEmpty(command.DatabaseName))
            databaseEntity.DatabaseName = command.DatabaseName;

        if (!string.IsNullOrEmpty(command.DatabaseUserPassword))
            databaseEntity.DatabaseUserPassword = encryption.Encrypt(command.DatabaseUserPassword);

        if (!string.IsNullOrEmpty(command.DatabaseUser))
            databaseEntity.DatabaseUser = command.DatabaseUser;

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

        var commands = new List<string>();
        if (oldDatabaseName != command.DatabaseName && !string.IsNullOrEmpty(command.DatabaseName))
            commands.Add($"ALTER DATABASE {oldDatabaseName} MODIFY NAME = {command.DatabaseName}");

        if (oldDatabaseUser != command.DatabaseUser && !string.IsNullOrEmpty(command.DatabaseUser))
            commands.Add(
                $"USE {command.DatabaseName}; ALTER USER {oldDatabaseUser} WITH NAME = {command.DatabaseUser}"
            );

        if (command.DatabaseUserPassword != null)
            commands.Add(
                $"ALTER LOGIN {command.DatabaseUser} WITH PASSWORD = '{command.DatabaseUserPassword}'"
            );

        foreach (var commandText in commands)
        {
            using var cmd = dbContext.Database.GetDbConnection().CreateCommand();
            cmd.CommandText = commandText;
            await dbContext.Database.OpenConnectionAsync();
            await cmd.ExecuteNonQueryAsync();
        }
    }
}
