using System;
using System.Threading;

namespace SpreadShare.Strategy
{
    public class Timer {
        long countdown;
        Thread thread;
        Action callback;
        bool shouldStop = false;

        public bool Valid { get { return DateTimeOffset.Now.ToUnixTimeMilliseconds() >= countdown; }}
        public Timer(long ms, Action callback) {
            this.callback = callback;
            countdown = DateTimeOffset.Now.ToUnixTimeMilliseconds() + ms;
            thread = new Thread(Wait);
            thread.Start();
        }

        private void Wait() {
            while(DateTimeOffset.Now.ToUnixTimeMilliseconds() < countdown) {
                if (shouldStop)
                    return;
                Thread.Sleep(1);
            }
            callback();
        }

        public long getRemaining() {
            return countdown - DateTimeOffset.Now.ToUnixTimeMilliseconds();
        }

        public void Stop() {
            shouldStop = true;
            thread.Join();
        }
    }
}