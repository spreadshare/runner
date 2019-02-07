using System;
using System.Diagnostics;
using System.Net.Mail;
using System.Text;
using Microsoft.Extensions.Logging;
using SpreadShare.Algorithms;
using AdministratorSettings = SpreadShare.SupportServices.Configuration.AdministratorSettings;

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
        private readonly bool _mailerEnabled;

        /// <summary>
        /// Initializes a new instance of the <see cref="ErrorService"/> class.
        /// </summary>
        /// <param name="factory">LoggerFactory for generating a logger.</param>
        /// <param name="algorithmService">Algorithm service to stop algorithms.</param>
        public ErrorService(ILoggerFactory factory, IAlgorithmService algorithmService)
        {
            _algorithmService = algorithmService;
            _logger = factory.CreateLogger(GetType());
            _administrationSettings = Configuration.Configuration.Instance.AdministratorSettings;
            _mailerEnabled = Configuration.Configuration.Instance.MailerEnabled;
            if (_mailerEnabled)
            {
                _client = new SmtpClient
                {
                    Port = 587,
                    Host = "smtp.gmail.com",
                    EnableSsl = true,
                    Timeout = 10000,
                    DeliveryMethod = SmtpDeliveryMethod.Network,
                    UseDefaultCredentials = false,
                    Credentials = new System.Net.NetworkCredential(
                        _administrationSettings.AdminEmail,
                        _administrationSettings.AdminPassword),
                };
            }
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
        /// <param name="state">The state the algorithm was in.</param>
        /// <param name="stackFrame">The stackframe associated with the exception that caused the error.</param>
        /// <param name="msg">The message accompanying the report.</param>
        public void ReportCriticalError(
            Type algorithm,
            string state,
            StackFrame stackFrame,
            string msg)
        {
            stackFrame = stackFrame ?? new StackFrame();
            string algorithmName = algorithm is null ? "Null" : algorithm.Name;
            string errorMsg = GetReport(algorithmName, state, stackFrame, msg);
            _logger.LogError(errorMsg);
            _logger.LogCritical($"Attempting to stop {algorithmName}");
            var query = _algorithmService.StopAlgorithm(algorithm);
            if (!query.Success)
            {
                _logger.LogError($"{algorithmName} could not be stopped, shutting down");
                SendReport(
                    "Program killed",
                    $"The program was killed after {algorithmName} reported a critical error but was unsuccessfully stopped \n\n {errorMsg}");
                Program.ExitProgramWithCode(ExitCode.AlgorithmNotStopping);
            }

            _logger.LogCritical($"{algorithmName} successfully killed");

            if (_mailerEnabled)
            {
                SendReport($"{algorithmName} Killed", errorMsg);
            }
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

        private static string GetReport(string algorithm, string state, StackFrame stackFrame, string msg)
        {
            return $"UTC Timestamp: {DateTimeOffset.UtcNow}\n" +
                   $"Algorithm: {algorithm}\n" +
                   $"State: {state}\n" +
                   $"Method: {stackFrame.GetMethod().Name}\n" +
                   $"File: {stackFrame.GetFileName()}\n" +
                   $"Line: {stackFrame.GetFileLineNumber()}\n" +
                   $"Message: {msg}\n";
        }

        private void SendReport(string header, string message)
        {
            foreach (var recipient in _administrationSettings.Recipients)
            {
                MailMessage mm = new MailMessage(
                    "donotreply@spreadshare.com",
                    recipient,
                    header,
                    message);
                mm.BodyEncoding = Encoding.UTF8;
                mm.DeliveryNotificationOptions = DeliveryNotificationOptions.OnFailure;
                _client.Send(mm);
            }
        }
    }
}