using System.Reflection;
using SpreadShare.SupportServices.ErrorServices;

namespace SpreadShare.Tests
{
    public abstract class EnvironmentTest
    {
        protected static void SetEnv(IEnv env)
        {
            var envProperty = typeof(Program).GetField("Env", BindingFlags.Static | BindingFlags.NonPublic);
            envProperty.SetValue(null, env);
        }
    }
}