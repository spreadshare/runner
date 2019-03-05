using System;
using System.Collections.Generic;
using Microsoft.Extensions.Logging;
using SpreadShare.ExchangeServices.Providers.Observing;
using SpreadShare.Models.Database;
using SpreadShare.Models.Trading;

namespace SpreadShare.SupportServices
{
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
        }

        public static DatabaseEventListenerService Instance { get; private set; }

        public void Bind() => Instance = this;

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
            var item = new DatabaseOrder(
                order,
                string.Empty,
                0);

            _database.Orders.Add(item);
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