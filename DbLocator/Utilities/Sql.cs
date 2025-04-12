using System.Text.RegularExpressions;

internal class Sql
{
    // Allow only alphanumeric and underscores
    public static string SanitizeSqlIdentifier(string input)
    {
        if (Regex.IsMatch(input, @"^[a-zA-Z0-9_]+$"))
            return input;

        throw new ArgumentException($"Invalid SQL identifier: {input}");
    }

    // Escape single quotes for use inside dynamic EXEC ('' for one ')
    public static string EscapeForDynamicSql(string input)
    {
        return input.Replace("'", "''");
    }
}
