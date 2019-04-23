using SpreadShare.SupportServices.ErrorServices;

namespace SpreadShare.Tests.Stubs
{
    public class TestEnvironment : IEnv
    {
        private bool _hasExited;

        public int Code { get; private set; } = 0;

        public bool HasExited => _hasExited;

        public void ExitEnvironment(int code)
        {
            Code = code;
            _hasExited = true;
        }
    }
}