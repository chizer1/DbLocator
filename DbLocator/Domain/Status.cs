namespace DbLocator.Domain;

/// <summary>
/// Represents the status of an entity.
/// </summary>
[Flags]
public enum Status
{
    /// <summary>
    /// The entity is active.
    /// </summary>
    Active = 1,

    /// <summary>
    /// The entity is inactive.
    /// </summary>
    Inactive = 2,
}
