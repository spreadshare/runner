using System;
using System.IO;
using Microsoft.Extensions.Logging;
using Sentry;
using YamlDotNet.Serialization;

namespace SpreadShare.SupportServices.ErrorServices
{
    /// <summary>
    /// Provides an <see cref="ILogger"/> implementation that logs exceptions to <see cref="SentrySdk"/>.
    /// <para>
    /// The standard library <see href="https://docs.sentry.io/platforms/dotnet/microsoft-extensions-logging/">
    /// Sentry.Extensions.Logging</see> could not be used as this project used Yaml configuration instead of Json
    /// configuration.
    /// </para>
    /// Documentation on the Sentry Library is available at
    /// <see href="https://docs.sentry.io/platforms/dotnet/aspnetcore/">here</see>.
    /// </summary>
    public class SentryLogger : ILogger, IDisposable
    {
        private readonly SentryClient _sentryClient;
        private readonly string _serializedConfiguration;
        private readonly string _serializedAlgorithmConfiguration;
        private readonly Func<LogLevel, bool> _isEnabled;

        /// <summary>
        /// Initializes a new instance of the <see cref="SentryLogger"/> class.
        /// </summary>
        public SentryLogger()
        {
            GetDsn(out var dsn);
            if (dsn == null)
            {
                _isEnabled = _ => false;
                return;
            }

            // DSN is well-formatted --> Enable logging for errors
            _isEnabled = level => level == LogLevel.Error;

            // Setup logging
            var options = new SentryOptions
            {
                // Sentry authentication
                Dsn = dsn,

                // Attach entire stack-trace
                AttachStacktrace = true,

                // Send user info like IP
                SendDefaultPii = true,
            };
            _sentryClient = new SentryClient(options);

            // Set configuration
            _serializedConfiguration = new SerializerBuilder().Build()
                .Serialize(Configuration.Configuration.Instance);
            var algorithm = Configuration.Configuration.Instance.EnabledAlgorithm;

            _serializedAlgorithmConfiguration += $"\n## {algorithm.Algorithm.Name} ##\n";
            try
            {
                _serializedAlgorithmConfiguration += File.ReadAllText($"{algorithm.Algorithm.Name}.yaml");
            }
            catch (FileNotFoundException)
            {
                _serializedAlgorithmConfiguration += $"Could not find file {algorithm.Algorithm.Name}.yaml, but this " +
                                                     "algorithm is enabled";
            }
        }

        /// <summary>
        /// Gets the DSN from the configuration.
        /// </summary>
        /// <param name="dsn">Parsed DSN or null.</param>
        public static void GetDsn(out Dsn dsn)
        {
            dsn = null;

            if (Configuration.Configuration.Instance.LoggingSettings == null)
            {
                Console.WriteLine("Sentry logging disabled as no valid SentryDSN is configured");
                return;
            }

            if (!Dsn.TryParse(Configuration.Configuration.Instance.LoggingSettings.SentryDSN, out dsn))
            {
                Console.WriteLine("Sentry logging disabled as no valid SentryDSN is configured");
            }
        }

        /// <inheritdoc />
        public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception exception, Func<TState, Exception, string> formatter)
        {
            // Skip irrelevant logging levels
            if (!IsEnabled(logLevel))
            {
                return;
            }

            // Case in which logger.LogError("message") is used
            if (exception == null)
            {
                exception = new Exception($"No exception thrown. Message included: {state}");
            }

            // Log to sentry
            var sentryEvent = new SentryEvent(exception);
            sentryEvent.SetExtra("configuration", _serializedConfiguration);
            sentryEvent.SetExtra("algorithm_configuration", _serializedAlgorithmConfiguration);
            _sentryClient.CaptureEvent(sentryEvent);
        }

        /// <summary>
        /// Sentry logger is only enabled when logging errors.
        /// </summary>
        /// <param name="logLevel">Level of logging.</param>
        /// <returns>Whether the logger is enabled for the provided logging level.</returns>
        public bool IsEnabled(LogLevel logLevel)
        {
            return _isEnabled(logLevel);
        }

        /// <inheritdoc />
        public IDisposable BeginScope<TState>(TState state)
        {
            return null;
        }

        /// <inheritdoc />
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Mandatory dispose pattern by CA1063.
        /// </summary>
        /// <param name="disposing">Whether to dispose unmanaged resources.</param>
        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                _sentryClient?.Dispose();
            }
        }
    }
}