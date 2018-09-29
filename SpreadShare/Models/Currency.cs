namespace SpreadShare.Models
{
    public class Currency
    {
        public string Symbol { get; }

        public Currency(string symbol) {
            Symbol = symbol;
        }

        public override string ToString() {
            return Symbol;
        }
    }
}