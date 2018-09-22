using Xunit;
using Prime.Services;

namespace Prime.UnitTests.Services
{
	public class IsPrimeShould
	{
		private readonly PrimeService _primeService;

		public IsPrimeShould()
		{
			_primeService = new PrimeService();
		}

		[Fact]
		public void ReturnFalseGivenValueOf1()
		{
			var result = _primeService.IsPrime(1);

			Assert.False(result, "1 should not be prime");
		}
	}
}
