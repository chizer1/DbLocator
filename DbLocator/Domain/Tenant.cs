namespace DbLocator.Domain;

/// <summary>
/// Represents a tenant
/// </summary>
public class Tenant(int Id, string Name, string Code, Status Status)
{
    /// <summary>
    /// Gets the ID of the Tenant.
    /// </summary>
    public int Id { get; init; } = Id;

    /// <summary>
    /// Gets the name of the Tenant.
    /// </summary>
    public string Name { get; init; } = Name;

    /// <summary>
    /// Gets the code of the Tenant.
    /// </summary>
    public string Code { get; init; } = Code;

    /// <summary>
    /// Gets the status of the Tenant.
    /// </summary>
    public Status Status { get; init; } = Status;
};
