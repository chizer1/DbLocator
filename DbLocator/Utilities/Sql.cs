using System.Text.RegularExpressions;
using DbLocator.Db;
using Microsoft.EntityFrameworkCore;

internal class Sql
{
    public static string SanitizeSqlIdentifier(string input)
    {
        // Allow only alphanumeric and underscores
        if (Regex.IsMatch(input, @"^[a-zA-Z0-9_]+$"))
            return input;

        throw new ArgumentException($"Invalid SQL identifier: {input}");
    }

    public static async Task ExecuteSqlCommandAsync(
        DbLocatorContext dbContext,
        string commandText,
        bool isLinkedServer = false,
        string linkedServerHostName = null
    )
    {
        if (isLinkedServer)
        {
            if (string.IsNullOrWhiteSpace(linkedServerHostName))
                throw new ArgumentException(
                    "Linked server host name must be provided when isLinkedServer is true.",
                    nameof(linkedServerHostName)
                );

            commandText =
                $"exec('{EscapeForDynamicSql(commandText)}') at [{linkedServerHostName}];";
        }

        await using var command = dbContext.Database.GetDbConnection().CreateCommand();
        command.CommandText = commandText;
        await dbContext.Database.OpenConnectionAsync();
        await command.ExecuteNonQueryAsync();
    }

    private static string EscapeForDynamicSql(string input)
    {
        return input.Replace("'", "''");
    }
}
