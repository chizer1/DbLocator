namespace DbLocator.Domain;

/// <summary>
/// Represents a Tenant with an id, name, code, and status.
/// </summary>
public class Tenant(int id, string name, string code, Status status)
{
    /// <summary>
    /// Gets the ID of the Tenant.
    /// </summary>
    public int Id { get; init; } = id;

    /// <summary>
    /// Gets the name of the Tenant.
    /// </summary>
    public string Name { get; init; } = name;

    /// <summary>
    /// Gets the code of the Tenant.
    /// </summary>
    public string Code { get; init; } = code;

    /// <summary>
    /// Gets the status of the Tenant.
    /// </summary>
    public Status Status { get; init; } = status;
};
