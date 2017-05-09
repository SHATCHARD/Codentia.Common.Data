namespace Codentia.Common.Data
{
    /// <summary>
    /// Enumerated type containing types of queries which can be executed against a database
    /// </summary>
    public enum DbQueryType
    {
        /// <summary>
        /// Adhoc (inline) query
        /// </summary>
        Adhoc,

        /// <summary>
        /// Stored Procedure
        /// </summary>
        StoredProcedure
    }
}