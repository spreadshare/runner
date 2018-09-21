using System;
using System.Threading;

namespace SpreadShare.Strategy
{
    public class Timer {
        private readonly long _countdown;
        private readonly Thread _thread;
        private readonly Action _callback;
        private bool _shouldStop;

        public bool Valid => DateTimeOffset.Now.ToUnixTimeMilliseconds() >= _countdown;

        public Timer(long ms, Action callback) {
            _callback = callback;
            _countdown = DateTimeOffset.Now.ToUnixTimeMilliseconds() + ms;
            _thread = new Thread(Wait);
            _thread.Start();
        }

        private void Wait() {
            while(DateTimeOffset.Now.ToUnixTimeMilliseconds() < _countdown) {
                if (_shouldStop)
                    return;
                Thread.Sleep(1);
            }
            _callback();
        }

        public long GetRemaining() {
            return _countdown - DateTimeOffset.Now.ToUnixTimeMilliseconds();
        }

        public void Stop() {
            _shouldStop = true;
            _thread.Join();
        }
    }
}