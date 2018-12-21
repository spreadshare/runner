using SpreadShare.Models;

namespace SpreadShare.ZeroMQ
{
    /// <summary>
    /// Interface for the ZeroMQ Service
    /// ZeroMQ protocol documentation is found in the PROTOCOL.md.
    /// </summary>
    internal interface IZeroMqService
    {
        /// <summary>
        /// Starts the ZeroMQ service.
        /// </summary>
        /// <returns>Whether the starting of the service was successful.</returns>
        ResponseObject Start();
    }
}
