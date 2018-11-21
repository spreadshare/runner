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
        /// <param name="delimiter">delimiter</param>
        /// <returns>CSV header string</returns>
        string GetCsvHeader(char delimiter);

        /// <summary>
        /// Get the current instance as CSV row
        /// </summary>
        /// <param name="delimiter">delimiter</param>
        /// <returns>CSV serialized string</returns>
        string GetCsvRepresentation(char delimiter);
    }
}