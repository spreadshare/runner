using System;
using System.Threading;
using System.Threading.Tasks;
using SpreadShare.Models;

namespace SpreadShare.Strategy
{
   
    internal class Timer  : IDisposable
    {
        private readonly Action _callback;
        private readonly System.Threading.Timer _timer;
        private uint _counter = 0;
        private readonly uint _targetCount;
        private readonly int _rest;
        private bool _executed = false;

        /// <summary>
        /// Constructor: Startes waiting period
        /// </summary>
        /// <param name="ms">Waiting time</param>
        /// <param name="callback">Callback to execute after wait; can't be null</param>
        public Timer(uint ms, Action callback) {
            _callback = callback ?? throw new ArgumentException("Callback can't be null");

            _targetCount = ms / 1000;
            _rest = (int)(ms % 1000);
            
            
            //As suggested by https://adrientorris.github.io/aspnet-core/how-to-implement-timer-netcoreapp1-0-netcoreapp1-1.html
            var autoEvent = new AutoResetEvent(false);
            _timer = new System.Threading.Timer(Execute, autoEvent, 0, 1000);      
        }

        private async void Execute(object stateInfo)
        {
            if (_executed) return;
            if (_counter < _targetCount)
            {
                Console.WriteLine($"Call #{_counter++}    {DateTime.UtcNow}");
                return;
            }

            if (_rest > 0) await Task.Delay(_rest);
            
            Console.WriteLine("Executing Callback");
            _executed = true;
            _callback();
        }

        
        public ResponseObject Stop()
        {
            _executed = true;
            return new ResponseObject(ResponseCodes.Success);
        }

        public void Dispose()
        {
            _timer?.Dispose();
        }
    }
}