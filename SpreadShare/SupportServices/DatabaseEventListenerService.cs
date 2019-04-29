using System;
using System.Collections.Generic;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using SpreadShare.ExchangeServices.Allocation;
using SpreadShare.ExchangeServices.Providers.Observing;
using SpreadShare.Models.Database;
using SpreadShare.Models.Trading;
using SpreadShare.SupportServices.ErrorServices;

namespace SpreadShare.SupportServices
{
    /// <summary>
    /// Service that logs OrderUpdates as events to the database.
    /// </summary>
    internal class DatabaseEventListenerService : IDisposable
    {
        private static readonly object Lock = new object();
        private readonly List<IDisposable> _sources;
        private readonly ILogger _logger;
        private readonly DatabaseContext _database;
        private readonly IAllocationManager _allocation;

        /// <summary>
        /// Initializes a new instance of the <see cref="DatabaseEventListenerService"/> class.
        /// </summary>
        /// <param name="factory">To create output.</param>
        /// <param name="allocation">To listen for portfolio changes.</param>
        /// <param name="database">To log events to.</param>
        public DatabaseEventListenerService(ILoggerFactory factory, IAllocationManager allocation, DatabaseContext database)
        {
            _sources = new List<IDisposable>();
            _logger = factory.CreateLogger(GetType());
            _allocation = allocation;
            lock (Lock)
            {
                _database = database;
            }
        }

        /// <summary>
        /// Gets or sets the instance of the <see cref="DatabaseEventListenerService"/> class.
        /// This property is not set if the database was not available.
        /// </summary>
        private static DatabaseEventListenerService Instance { get; set; }

        /// <summary>
        /// Gets or sets the current database session.
        /// </summary>
        private AlgorithmSession Session { get; set; }

        /// <summary>
        /// Add a whose broadcasted order updates should be recorded as events.
        /// </summary>
        /// <param name="source">The broadcaster of order updates.</param>
        public static void AddOrderSource(Observable<OrderUpdate> source) => Instance?.AddOrderSourceImplementation(source);

        /// <summary>
        /// Add whose broadcasted state switches should be recorded as events.
        /// </summary>
        /// <param name="source">The broadcaster of state switches.</param>
        public static void AddStateSource(Observable<Type> source) => Instance?.AddStateSourceImplementation(source);

        /// <summary>
        /// Allows logs to be written to the database.
        /// </summary>
        /// <param name="logLevel">logLevel.</param>
        /// <param name="state">state.</param>
        /// <param name="exception">exception.</param>
        /// <param name="formatter">formatter.</param>
        /// <typeparam name="TState">TState.</typeparam>
        public static void Log<TState>(LogLevel logLevel, TState state, Exception exception, Func<TState, Exception, string> formatter)
            => Instance?.LogImplementation(logLevel, state, exception, formatter);

        /// <summary>
        /// Closes the current session.
        /// </summary>
        /// <param name="exitCode">The exit code with which to close the session.</param>
        public static void CloseSession(ExitCode exitCode) => Instance?.CloseSessionImplementation(exitCode);

        /// <summary>
        /// Lift the current instance to the static instance.
        /// </summary>
        public void Bind()
        {
            if (Instance != null)
            {
                _logger.LogWarning("A new session is being started but one already exists.");
            }

            Session = new AlgorithmSession
            {
                Name = Configuration.Configuration.Instance.EnabledAlgorithm.Algorithm.Name,
                CreatedTimestamp = DateTimeOffset.Now.ToUnixTimeMilliseconds(),
                ContainerId = Environment.MachineName,
                AlgorithmConfiguration = JsonConvert.SerializeObject(Configuration.Configuration.Instance.EnabledAlgorithm.AlgorithmConfiguration),
                Active = true,
            };

            _sources.Add(_allocation.Subscribe(new ConfigurableObserver<Portfolio>(
                () => { },
                _ => { },
                OnNext)));

            lock (Lock)
            {
                _database.Sessions.Add(Session);
                _database.SaveChanges();
            }

            var prev = Instance;
            Instance = this;
            prev?.Dispose();
        }

        /// <inheritdoc />
        public virtual void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        private void AddOrderSourceImplementation(Observable<OrderUpdate> source)
        {
            _sources.Add(source.Subscribe(new ConfigurableObserver<OrderUpdate>(
                () => { },
                _ => { },
                OnNext)));
        }

        private void AddStateSourceImplementation(Observable<Type> source)
        {
            _sources.Add(source.Subscribe(new ConfigurableObserver<Type>(
                () => { },
                _ => { },
                OnNext)));
        }

        private void LogImplementation<TState>(LogLevel logLevel, TState state, Exception exception, Func<TState, Exception, string> formatter)
        {
            var item = new LogEvent
            {
                LogLevel = logLevel,
                Session = Session,
                Text = formatter(state, exception),
                EventTimestamp = DateTimeOffset.Now.ToUnixTimeMilliseconds(),
            };

            lock (Lock)
            {
                _database.LogEvents.Add(item);
                _database.SaveChanges();
            }
        }

        private void CloseSessionImplementation(ExitCode exitCode)
        {
            Session.Active = false;
            Session.ExitCode = (int)exitCode;
            lock (Lock)
            {
                _database.SaveChanges();
            }

            Dispose();
        }

        private void OnNext(OrderUpdate order)
        {
            var item = new OrderEvent(Session, DateTimeOffset.Now.ToUnixTimeMilliseconds(), order);
            lock (Lock)
            {
                _database.OrderEvents.Add(item);
                _database.SaveChanges();
            }
        }

        private void OnNext(Type stateSwitch)
        {
            var item = new StateSwitchEvent(
                DateTimeOffset.Now.ToUnixTimeMilliseconds(),
                stateSwitch.Name,
                Session);

            lock (Lock)
            {
                _database.StateSwitchEvents.Add(item);
                _database.SaveChanges();
            }
        }

        private void OnNext(Portfolio portfolio)
        {
            Session.Allocation = portfolio;
            lock (Lock)
            {
                _database.SaveChanges();
            }
        }

        private void Dispose(bool disposing)
        {
            if (disposing)
            {
                foreach (var source in _sources)
                {
                    source?.Dispose();
                }

                _logger.LogInformation($"Deactivating session '{Session.Name}'");
                Session.Active = false;
                Session.ClosedTimestamp = DateTimeOffset.Now.ToUnixTimeMilliseconds();

                lock (Lock)
                {
                    _database.SaveChanges();
                }
            }
        }
    }
}
