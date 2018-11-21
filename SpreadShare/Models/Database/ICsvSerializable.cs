namespace SpreadShare.Models.Database
{
    /// <summary>
    /// Interface that allows for serialization to csv files
    /// </summary>
    internal interface ICsvSerializable
    {
        /// <summary>
        /// Get the header of the CSV representation
        /// </summary>
        /// <returns></returns>
        string GetCsvHeader();

        /// <summary>
        /// Get the current instance as CSV row
        /// </summary>
        /// <returns></returns>
        string GetCsvRepresentation();
    }
}