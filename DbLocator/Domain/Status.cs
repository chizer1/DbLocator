namespace DbLocator.Domain;

/// <summary>
/// Represents the operational status of an entity within the DbLocator system.
/// This enum is used to indicate the current state of various entities, such as
/// databases, servers, tenants, or other system components. The status values
/// help manage the lifecycle and operational state of these entities throughout
/// the system.
///
/// The Status enum is marked with the <see cref="FlagsAttribute"/> to allow for
/// combining multiple status values if needed. This flexibility enables more
/// complex state management scenarios where an entity might need to be in multiple
/// states simultaneously (e.g., both Active and InMaintenance).
///
/// This enum is used consistently across the system to maintain a uniform approach
/// to state management and to ensure that all components can properly interpret
/// and respond to entity status changes.
/// </summary>
[Flags]
public enum Status
{
    /// <summary>
    /// Indicates that the entity is active and fully operational.
    /// An active status means the entity is available for use, functional,
    /// and part of the system's current operations. This is the default
    /// state for most entities when they are first created and ready for use.
    ///
    /// When an entity is in the Active state:
    /// - It is available for normal operations
    /// - It can be accessed by authorized users
    /// - It is fully functional and ready to perform its intended tasks
    /// - It is part of the system's active infrastructure
    /// </summary>
    Active = 1,

    /// <summary>
    /// Indicates that the entity is inactive and not currently operational.
    /// An inactive status means the entity is either disabled or temporarily
    /// unavailable, and it is not involved in any operations. This state is
    /// typically used when an entity needs to be taken out of service, either
    /// temporarily or permanently.
    ///
    /// When an entity is in the Inactive state:
    /// - It is not available for normal operations
    /// - It cannot be accessed by users
    /// - It is not performing its intended tasks
    /// - It is not part of the system's active infrastructure
    ///
    /// This state can be used for various purposes, such as:
    /// - Maintenance periods
    /// - Decommissioning
    /// - Temporary suspension of services
    /// - Error states that require manual intervention
    /// </summary>
    Inactive = 2
}
