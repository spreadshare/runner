namespace SpreadShare.Models
{
    static class CurrencyPairs
    {
        public static CurrencyPair BNBETH = new CurrencyPair(Currencies.BNB, Currencies.ETH);
    }

    public class CurrencyPair
    {
        Currency _left;
        Currency _right;

        public CurrencyPair(Currency Left, Currency Right) {
            _left = Left;
            _right = Right;
        }

        public Currency Left { get { return _left; }}
        public Currency Right { get { return _right; }}

        public override string ToString() {
            return $"{_left}{_right}";
        }
    }
}