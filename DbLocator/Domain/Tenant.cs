namespace DbLocator.Domain;

/// <summary>
/// Represents a tenant within the DbLocator system.
/// A tenant is a fundamental organizational unit that represents a customer,
/// organization, or separate entity that has access to a set of resources or
/// services within the system. This class is designed to support multi-tenant
/// architectures where different organizations or customers need to be isolated
/// and managed independently.
///
/// The Tenant class encapsulates essential information about the tenant's identity,
/// operational status, and unique identifiers. It serves as a central component
/// for managing tenant-specific resources, configurations, and access controls
/// within the system.
///
/// This class is immutable by design, ensuring that tenant configurations remain
/// consistent throughout their lifecycle. Any modifications to a tenant's
/// configuration should be performed through the appropriate service layer methods.
/// </summary>
/// <param name="Id">
/// The unique identifier of the tenant. This ID is used to uniquely identify
/// the tenant within the system. It is typically used as a primary key when
/// referencing the tenant in other entities or records. The ID is assigned by
/// the system during creation and should not be modified after the tenant is
/// created.
/// </param>
/// <param name="Name">
/// The name of the tenant. The name represents the tenant's official name or
/// the organization it represents. This name is used for display purposes and
/// helps to identify the tenant within the system. The name should be descriptive
/// and follow the system's naming conventions for easy identification and
/// management.
/// </param>
/// <param name="Code">
/// The unique code of the tenant. The tenant code is a unique string or identifier
/// that is often used for system-level or integration-level references. It can be
/// used for programmatic identification and is typically shorter and more concise
/// than the name. The code should follow the system's coding conventions and be
/// easily recognizable in logs and system interfaces.
/// </param>
/// <param name="Status">
/// The current status of the tenant. The status represents the tenant's current
/// state within the system, which could be "Active", "Inactive", or any other
/// defined status. It helps manage tenant-specific access, configurations, and
/// operations. The status is used to control the tenant's ability to access
/// system resources and perform operations.
/// </param>
public class Tenant(int Id, string Name, string Code, Status Status)
{
    /// <summary>
    /// Gets the unique identifier of the tenant.
    /// This ID is used to uniquely identify the tenant within the system. It is
    /// typically used as a primary key when referencing the tenant in other
    /// entities or records. The ID is immutable and cannot be changed after
    /// the tenant is created.
    /// </summary>
    public int Id { get; init; } = Id;

    /// <summary>
    /// Gets the name of the tenant.
    /// The name represents the tenant's official name or the organization
    /// it represents. This name is used for display purposes and helps to
    /// identify the tenant within the system. The name should be descriptive
    /// and follow the system's naming conventions for easy identification and
    /// management. This property is immutable and cannot be changed after the
    /// tenant is created.
    /// </summary>
    public string Name { get; init; } = Name;

    /// <summary>
    /// Gets the unique code of the tenant.
    /// The tenant code is a unique string or identifier that is often used for
    /// system-level or integration-level references. It can be used for
    /// programmatic identification and is typically shorter and more concise
    /// than the name. The code should follow the system's coding conventions
    /// and be easily recognizable in logs and system interfaces. This property
    /// is immutable and cannot be changed after the tenant is created.
    /// </summary>
    public string Code { get; init; } = Code;

    /// <summary>
    /// Gets the current status of the tenant.
    /// The status represents the tenant's current state within the system,
    /// which could be "Active", "Inactive", or any other defined status.
    /// It helps manage tenant-specific access, configurations, and operations.
    /// The status is used to control the tenant's ability to access system
    /// resources and perform operations. This property is immutable and cannot
    /// be changed after the tenant is created.
    /// </summary>
    public Status Status { get; init; } = Status;
}
