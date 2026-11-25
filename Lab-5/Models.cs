using System;
using System.Collections.Generic;

namespace Lab5
{
    /// <summary>Таблица «Счета».</summary>
    public class Account
    {
        public int Id { get; set; }
        public string FullName { get; set; }
        public DateTime OpenDate { get; set; }

        public Account() : this(0, "", DateTime.MinValue)
        {
        }

        public Account(
            int id, string fullName, DateTime openDate)
        {
            Id = id;
            FullName = fullName;
            OpenDate = openDate;
        }

        public override string ToString()
        {
            return Id.ToString().PadRight(7) +
                FullName.PadRight(26) +
                OpenDate.ToString("dd.MM.yyyy");
        }
    }

    /// <summary>Таблица «Курс валют».</summary>
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

    /// <summary>Таблица «Поступления».</summary>
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

    /// <summary>База данных целиком.</summary>
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