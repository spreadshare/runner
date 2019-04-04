using SpreadShare.Algorithms;
using SpreadShare.Algorithms.Implementations;

namespace SpreadShare.Tests.Stubs
{
    internal class TestState : State<TemplateAlgorithmConfiguration>
    {
        public void TestWaitForNextCandle() => WaitForNextCandle();
    }
}