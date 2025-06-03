namespace DbLocator.Domain;

/// <summary>
/// Represents a tenant within the system.
/// A tenant typically represents an organization, a customer, or a separate unit
/// that has access to a set of resources or services. This class is used to
/// define a tenant's identity, its unique code, and its current operational
/// status within the system.
/// </summary>
public class Tenant(int Id, string Name, string Code, Status Status)
{
    /// <summary>
    /// Gets the unique identifier of the tenant.
    /// This ID is used to uniquely identify the tenant within the system. It is
    /// typically used as a primary key when referencing the tenant in other
    /// entities or records.
    /// </summary>
    public int Id { get; init; } = Id;

    /// <summary>
    /// Gets the name of the tenant.
    /// The name represents the tenant's official name or the organization
    /// it represents. This name is used for display purposes and helps
    /// to identify the tenant within the system.
    /// </summary>
    public string Name { get; init; } = Name;

    /// <summary>
    /// Gets the unique code of the tenant.
    /// The tenant code is a unique string or identifier that is often used
    /// for system-level or integration-level references. It can be used for
    /// programmatic identification and is typically shorter and more concise
    /// than the name.
    /// </summary>
    public string Code { get; init; } = Code;

    /// <summary>
    /// Gets the current status of the tenant.
    /// The status represents the tenant's current state within the system,
    /// which could be "Active", "Inactive", or any other defined status.
    /// It helps manage tenant-specific access, configurations, and operations.
    /// </summary>
    public Status Status { get; init; } = Status;
}
