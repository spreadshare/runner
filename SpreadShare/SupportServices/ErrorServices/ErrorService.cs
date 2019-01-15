using System;
using System.Net.Mail;
using System.Runtime.CompilerServices;
using System.Text;
using Microsoft.Extensions.Logging;
using SpreadShare.Algorithms;
using SpreadShare.SupportServices.SettingsServices;

namespace SpreadShare.SupportServices.ErrorServices
{
    /// <summary>
    /// Service for reporting  to administrators.
    /// </summary>
    internal class ErrorService : IDisposable
    {
        private readonly IAlgorithmService _algorithmService;
        private readonly AdministratorSettings _administrationSettings;
        private readonly ILogger _logger;
        private readonly SmtpClient _client;

        /// <summary>
        /// Initializes a new instance of the <see cref="ErrorService"/> class.
        /// </summary>
        /// <param name="factory">LoggerFactory for generating a logger.</param>
        /// <param name="algorithmService">Algorithm service to stop algorithms.</param>
        /// <param name="settings">Settings for email authentication.</param>
        public ErrorService(ILoggerFactory factory, IAlgorithmService algorithmService, SettingsService settings)
        {
            _algorithmService = algorithmService;
            _logger = factory.CreateLogger(GetType());
            _administrationSettings = settings.AdministratorSettings;
            _client = new SmtpClient
            {
                Port = 587,
                Host = "smtp.gmail.com",
                EnableSsl = true,
                Timeout = 10000,
                DeliveryMethod = SmtpDeliveryMethod.Network,
                UseDefaultCredentials = false,
                Credentials = new System.Net.NetworkCredential(
                    _administrationSettings.AdminEmail.Address,
                    _administrationSettings.AdminPassword),
            };
        }

        /// <summary>
        /// Gets the instance of the ErrorService.
        /// </summary>
        public static ErrorService Instance { get; private set; }

        /// <summary>
        /// Lift the instance of the error handler to the global singleton.
        /// </summary>
        public void Bind()
        {
            Instance = this;
        }

        /// <summary>
        /// Report an error to the administrator list and shutdown the Algorithm.
        /// </summary>
        /// <param name="algorithm">The algorithm to stop.</param>
        /// <param name="msg">The message accompanying the report.</param>
        /// <param name="callerName">callerName is inserted at runtime.</param>
        /// <param name="fileName">fileName is inserted at runtime.</param>
        /// <param name="lineNumber">lineNumber is inserted at runtime.</param>
        public void ReportCriticalError(
            Type algorithm,
            string msg,
            [CallerMemberName] string callerName = "",
            [CallerFilePath] string fileName = "",
            [CallerLineNumber] int lineNumber = 0)
        {
            string errorMsg = GetReport(algorithm, callerName, fileName, lineNumber, msg);
            _logger.LogError(errorMsg);
            _logger.LogCritical($"Attempting to stop {algorithm.Name}");
            var query = _algorithmService.StopAlgorithm(algorithm);
            if (!query.Success)
            {
                _logger.LogError($"{algorithm.Name} could not be stopped, shutting down");
                SendReport(
                    "Program killed",
                    $"The program was killed after {algorithm.Name}.{callerName} reported a critical error but was unsuccessfully stopped \n\n {errorMsg}");
                Program.ExitProgramWithCode(ExitCode.AlgorithmNotStopping);
            }

            _logger.LogCritical($"{algorithm.Name} successfully killed");
            SendReport($"{algorithm.Name} Killed", errorMsg);
        }

        /// <inheritdoc />
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Dispose pattern.
        /// </summary>
        /// <param name="disposing">Actually dispose.</param>
        protected void Dispose(bool disposing)
        {
            if (disposing)
            {
                _client.Dispose();
            }
        }

        private static string GetReport(Type algorithm, string method, string file, int line, string msg)
        {
            return $"UTC Timestamp: {DateTimeOffset.UtcNow}" +
                   $"Algorithm: {algorithm.Name}\n" +
                   $"Method: {method}\n" +
                   $"File: {file}\n" +
                   $"Line: {line}\n" +
                   $"Message: {msg}";
        }

        private void SendReport(string header, string message)
        {
            foreach (var recipient in _administrationSettings.Recipients)
            {
                MailMessage mm = new MailMessage(
                    "donotreply@spreadshare.com",
                    recipient.Address,
                    header,
                    message);
                mm.BodyEncoding = Encoding.UTF8;
                mm.DeliveryNotificationOptions = DeliveryNotificationOptions.OnFailure;
                _client.Send(mm);
            }
        }
    }
}