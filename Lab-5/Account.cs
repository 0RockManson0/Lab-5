using System;

namespace Lab5
{
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
}