using Microsoft.Extensions.Logging;

namespace SpreadShare.Strategy
{
    class SimpleBandWagonStrategy : BaseStrategy
    {
        public SimpleBandWagonStrategy(ILoggerFactory loggerFactory) : base(loggerFactory)
        {
        }

        public override State GetInitialState()
        {
            return new EntryState();
        }

        internal class EntryState : State
        {
            protected override void ValidateContext()
            {
                Logger.LogInformation("Validating context");
            }

            public override void OnSomeAction()
            {
                Logger.LogInformation("Some action");
                SwitchState(new FinalState());
            }
        }

        internal class FinalState : State
        {
            protected override void ValidateContext()
            {
                Logger.LogInformation("Validating context");
            }

            public override void OnSomeAction()
            {
                Logger.LogInformation("Some action");
                SwitchState(new EntryState());
            }
        }
    }
}
