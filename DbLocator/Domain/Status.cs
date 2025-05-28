namespace DbLocator.Domain;

/// <summary>
/// Represents the status of an entity within the system.
/// This enum is used to indicate the current state of an entity, such as whether
/// it is active or inactive, and can be used in conjunction with other flags for
/// more granular state management. The enum is marked with the <see cref="FlagsAttribute"/>
/// to allow for combining multiple status values if needed.
/// </summary>
[Flags]
public enum Status
{
    /// <summary>
    /// Indicates that the entity is active and operational.
    /// An active status typically means the entity is available for use,
    /// functional, and part of the system's current operations.
    /// </summary>
    Active = 1,

    /// <summary>
    /// Indicates that the entity is inactive and not currently operational.
    /// An inactive status typically means the entity is either disabled or
    /// temporarily unavailable, and it is not involved in any operations.
    /// </summary>
    Inactive = 2,
}
