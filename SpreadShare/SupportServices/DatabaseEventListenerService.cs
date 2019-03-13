using System;
using System.Collections.Generic;
using Microsoft.Extensions.Logging;
using SpreadShare.ExchangeServices.Providers.Observing;
using SpreadShare.Models.Database;
using SpreadShare.Models.Trading;

namespace SpreadShare.SupportServices
{
    /// <summary>
    /// Service that logs OrderUpdates as events to the database.
    /// </summary>
    internal class DatabaseEventListenerService : IDisposable
    {
        private readonly List<IDisposable> _sources;
        private readonly ILogger _logger;
        private readonly DatabaseContext _database;

        /// <summary>
        /// Initializes a new instance of the <see cref="DatabaseEventListenerService"/> class.
        /// </summary>
        /// <param name="factory">To create output.</param>
        /// <param name="database">To events to.</param>
        public DatabaseEventListenerService(ILoggerFactory factory, DatabaseContext database)
        {
            _sources = new List<IDisposable>();
            _logger = factory.CreateLogger(GetType());
            _database = database;
            Session = new AlgorithmSession
            {
                Name = Configuration.Configuration.Instance.EnabledAlgorithm.Name,
            };

            _database.Sessions.Add(Session);
            _database.SaveChanges();
        }

        /// <summary>
        /// Gets the instance of the <see cref="DatabaseEventListenerService"/> class.
        /// This property is not set if the database was not available.
        /// </summary>
        public static DatabaseEventListenerService Instance { get; private set; }

        /// <summary>
        /// Gets the current database session.
        /// </summary>
        public AlgorithmSession Session { get; }

        /// <summary>
        /// Lift the current instance to the static instance.
        /// </summary>
        public void Bind() => Instance = this;

        /// <summary>
        /// Add a whose broadcasted order updates should be recorded as events.
        /// </summary>
        /// <param name="source">The broadcaster of order updates.</param>
        public void AddDataSource(Observable<OrderUpdate> source)
        {
            _sources.Add(source.Subscribe(new ConfigurableObserver<OrderUpdate>(
                OnNext,
                () => { },
                e => { })));
        }

        /// <inheritdoc />
        public virtual void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        private void OnNext(OrderUpdate order)
        {
            var item = new OrderEvent(order, DateTimeOffset.Now.ToUnixTimeMilliseconds(), Session);
            _database.OrderEvents.Add(item);
            _database.SaveChanges();
        }

        private void Dispose(bool disposing)
        {
            if (disposing)
            {
                foreach (var source in _sources)
                {
                    source?.Dispose();
                }
            }
        }
    }
}