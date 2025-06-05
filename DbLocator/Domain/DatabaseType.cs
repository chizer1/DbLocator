#nullable enable

namespace DbLocator.Domain;

/// <summary>
/// Represents a logical type of database within the DbLocator system, which defines
/// the kind of data being stored or the logical grouping of data. This class serves
/// as a categorization mechanism for databases, allowing the system to manage and
/// configure different types of databases based on their intended use and characteristics.
///
/// The DatabaseType class is designed to support various database categorization
/// scenarios, such as distinguishing between operational databases, analytical
/// databases, archival databases, or any other logical grouping that makes sense
/// in the context of the system. This categorization helps in organizing and
/// managing databases more effectively, especially in complex environments with
/// multiple databases serving different purposes.
///
/// This class is immutable by design, ensuring that database type configurations
/// remain consistent throughout their lifecycle. Any modifications to a database
/// type's configuration should be performed through the appropriate service layer
/// methods.
/// </summary>
/// <param name="Id">
/// The unique identifier for the database type. This ID is used to distinguish
/// between different logical types of databases in the system, allowing the system
/// to manage and configure each logical type separately. The ID is assigned by
/// the system during creation and should not be modified after the database type
/// is created.
/// </param>
/// <param name="Name">
/// The name of the logical database type, which describes the kind of data stored
/// or the role of the data within the system (e.g., "Customer Data", "Inventory Data").
/// This name helps identify the logical categorization of databases and is used in
/// configurations, reports, and logs to refer to these types of data storage. The
/// name should be descriptive and follow the system's naming conventions for easy
/// identification and management.
/// </param>
public class DatabaseType(int Id, string Name)
{
    /// <summary>
    /// Gets the unique identifier for the database type.
    /// This ID serves as the primary key used to uniquely identify and reference
    /// different logical database types within the system, enabling efficient
    /// management and configuration of databases based on their type. The ID is
    /// immutable and cannot be changed after the database type is created.
    /// </summary>
    public int Id { get; init; } = Id;

    /// <summary>
    /// Gets the name of the logical database type.
    /// This name represents the category or purpose of the data stored in the database,
    /// such as "Customer Data", "Product Catalog", or "Employee Records". It is used
    /// to categorize databases within the system according to the logical grouping of
    /// the data, helping distinguish between different kinds of information stored.
    /// The name should be descriptive and follow the system's naming conventions for
    /// easy identification and management. This property is immutable and cannot be
    /// changed after the database type is created.
    /// </summary>
    public string Name { get; init; } = Name;
}
