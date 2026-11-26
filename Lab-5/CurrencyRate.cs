namespace Lab5
{
    public class CurrencyRate
    {
        public int Id { get; set; }
        public string Code { get; set; }
        public double Rate { get; set; }
        public string Name { get; set; }

        public CurrencyRate() : this(0, "", 0, "")
        {
        }

        public CurrencyRate(
            int id, string code, double rate, string name)
        {
            Id = id;
            Code = code;
            Rate = rate;
            Name = name;
        }

        public override string ToString()
        {
            return Id.ToString().PadRight(7) +
                   Code.PadRight(6) +
                   Rate.ToString("F4").PadRight(12) + Name;
        }
    }
}