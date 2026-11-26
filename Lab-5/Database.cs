using System.Collections.Generic;

namespace Lab5
{
    public class Database
    {
        public List<Account> Accounts { get; set; }
        public List<CurrencyRate> Rates { get; set; }
        public List<Transaction> Transactions { get; set; }
        public string SourcePath { get; set; }
        public bool HasChanges { get; set; }

        public Database()
        {
            Accounts = new List<Account>();
            Rates = new List<CurrencyRate>();
            Transactions = new List<Transaction>();
            SourcePath = "";
            HasChanges = false;
        }
    }
}