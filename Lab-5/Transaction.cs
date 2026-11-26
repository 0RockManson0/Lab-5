using System;

namespace Lab5
{
    public class Transaction
    {
        public int Id { get; set; }
        public int AccountId { get; set; }
        public int CurrencyId { get; set; }
        public DateTime Date { get; set; }
        public double Amount { get; set; }

        public Transaction()
            : this(0, 0, 0, DateTime.MinValue, 0)
        {
        }

        public Transaction(
            int id,
            int accountId,
            int currencyId,
            DateTime date,
            double amount)
        {
            Id = id;
            AccountId = accountId;
            CurrencyId = currencyId;
            Date = date;
            Amount = amount;
        }

        public override string ToString()
        {
            return Id.ToString().PadRight(8) +
                   AccountId.ToString().PadRight(8) +
                   CurrencyId.ToString().PadRight(8) +
                   Date.ToString("dd.MM.yyyy").PadRight(13) +
                   Amount.ToString("F2");
        }
    }
}