#nullable enable

using System.Text.RegularExpressions;
using DbLocator.Db;
using Microsoft.EntityFrameworkCore;

internal class Sql
{
    /// <summary>
    /// Sanitizes a SQL identifier by allowing only alphanumeric characters and underscores.
    /// </summary>
    /// <param name="input">The input string to sanitize.</param>
    /// <returns>The sanitized SQL identifier if valid.</returns>
    /// <exception cref="ArgumentException">Thrown if the input contains invalid characters.</exception>
    public static string SanitizeSqlIdentifier(string input)
    {
        // Allow only alphanumeric and underscores
        if (Regex.IsMatch(input, @"^[a-zA-Z0-9_]+$"))
            return input;

        throw new ArgumentException($"Invalid SQL identifier: \"{input}\"", nameof(input));
    }

    /// <summary>
    /// Executes a SQL command asynchronously against the specified DbLocatorContext.
    /// </summary>
    /// <param name="dbContext">The database context to use for executing the command.</param>
    /// <param name="commandText">The SQL command text to execute.</param>
    /// <param name="isLinkedServer">Indicates whether the command should be executed on a linked server.</param>
    /// <param name="linkedServerHostName">The host name of the linked server (required if isLinkedServer is true).</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    /// <exception cref="ArgumentException">Thrown if linkedServerHostName is required but not provided.</exception>
    public static async Task ExecuteSqlCommandAsync(
        DbLocatorContext dbContext,
        string commandText,
        bool isLinkedServer = false,
        string? linkedServerHostName = null
    )
    {
        if (isLinkedServer)
        {
            if (string.IsNullOrWhiteSpace(linkedServerHostName))
                throw new ArgumentException(
                    "Linked server host name is required when isLinkedServer is true",
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
