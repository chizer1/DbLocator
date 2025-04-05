namespace DbLocator.Domain
{
    /// <summary>
    /// Represents a logical type of database, which defines the kind of data being stored
    /// or the logical grouping of data. This could represent different categories or
    /// schemas of data within the system, such as "Customer Data", "Product Inventory",
    /// "Financial Transactions", or other types of data storage.
    /// </summary>
    /// <param name="Id">
    /// The unique identifier for the database type. This ID is used to distinguish
    /// between different logical types of databases in the system, allowing the system
    /// to manage and configure each logical type separately.
    /// </param>
    /// <param name="Name">
    /// The name of the logical database type, which describes the kind of data stored
    /// or the role of the data within the system (e.g., "Customer Data", "Inventory Data").
    /// This name helps identify the logical categorization of databases and is used in
    /// configurations, reports, and logs to refer to these types of data storage.
    /// </param>
    public class DatabaseType(int Id, string Name)
    {
        /// <summary>
        /// Gets the unique identifier for the database type.
        /// This ID serves as the primary key used to uniquely identify and reference
        /// different logical database types within the system, enabling efficient
        /// management and configuration of databases based on their type.
        /// </summary>
        public int Id { get; init; } = Id;

        /// <summary>
        /// Gets the name of the logical database type.
        /// This name represents the category or purpose of the data stored in the database,
        /// such as "Customer Data", "Product Catalog", or "Employee Records". It is used
        /// to categorize databases within the system according to the logical grouping of
        /// the data, helping distinguish between different kinds of information stored.
        /// </summary>
        public string Name { get; init; } = Name;
    }
}
