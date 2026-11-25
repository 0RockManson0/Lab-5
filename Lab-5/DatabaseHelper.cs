using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Lab5
{
    /// <summary>
    /// Вспомогательный класс: весь функционал приложения.
    /// Все выборки реализованы LINQ-запросами.
    /// </summary>
    internal static class DatabaseHelper
    {
        private const int PageSize = 20;
        

        public static Database Load(string path)
        {
            Database db = ExcelService.Load(path);
            Console.WriteLine("\nЗагружено: " + db.SourcePath);
            PrintCounters(db);
            return db;
        }

        public static void Save(Database db)
        {
            Console.WriteLine("\nТекущий файл: " + db.SourcePath);
            Console.Write("Путь (Enter — текущий, 0 — отмена): ");
            string path = (Console.ReadLine() ?? "").Trim();

            if (path == "0")
            {
                Console.WriteLine("Сохранение отменено.");
                return;
            }

            if (path.Length == 0)
            {
                path = db.SourcePath;
            }

            if (!path.EndsWith(
                    ".xls", StringComparison.OrdinalIgnoreCase) &&
                !path.EndsWith(
                    ".xlsx", StringComparison.OrdinalIgnoreCase))
            {
                Console.WriteLine(
                    "Нужно расширение .xls или .xlsx.");
                return;
            }

            if (File.Exists(path) && !ConsoleHelper.ReadYesNo(
                "Файл существует. Перезаписать?"))
            {
                Console.WriteLine("Сохранение отменено.");
                return;
            }

            ExcelService.Save(db, path);
            db.SourcePath = Path.GetFullPath(path);
            db.HasChanges = false;
            Console.WriteLine("Сохранено: " + db.SourcePath);
        }

        public static void PrintCounters(Database db)
        {
            Console.WriteLine(
                "Счетов: " + db.Accounts.Count +
                " | Валют: " + db.Rates.Count +
                " | Поступлений: " + db.Transactions.Count);
        }

        private static int ChooseTable(string action)
        {
            Console.WriteLine("\n" + action + ". Таблица:");
            Console.WriteLine("1 — Счета");
            Console.WriteLine("2 — Курс валют");
            Console.WriteLine("3 — Поступления");
            Console.WriteLine("0 — назад");
            return ConsoleHelper.ReadIntInRange(
                "Ваш выбор: ", 0, 3);
        }
        

        public static void View(Database db)
        {
            int table = ChooseTable("Просмотр");
            if (table == 0)
            {
                return;
            }

            Console.WriteLine("1 — постранично");
            Console.WriteLine("2 — поиск по ключу (ID)");
            int mode = ConsoleHelper.ReadIntInRange(
                "Режим (0 — назад): ", 0, 2);

            if (mode == 1)
            {
                ShowTable(db, table);
            }
            else if (mode == 2)
            {
                FindByKey(db, table);
            }
        }

        private static void ShowTable(Database db, int table)
        {
            if (table == 1)
            {
                ShowPages(
                    db.Accounts.OrderBy(a => a.Id).ToList(),
                    "Счета");
            }
            else if (table == 2)
            {
                ShowPages(
                    db.Rates.OrderBy(r => r.Id).ToList(),
                    "Курс валют");
            }
            else
            {
                ShowPages(
                    db.Transactions.OrderBy(t => t.Id).ToList(),
                    "Поступления");
            }
        }

        private static void FindByKey(Database db, int table)
        {
            int id = ConsoleHelper.ReadIdOrCancel(
                "ID для поиска (0 — отмена): ");
            if (id == 0)
            {
                Console.WriteLine("Поиск отменён.");
                return;
            }

            object found = null;
            if (table == 1)
            {
                found = db.Accounts
                    .FirstOrDefault(a => a.Id == id);
            }
            else if (table == 2)
            {
                found = db.Rates
                    .FirstOrDefault(r => r.Id == id);
            }
            else
            {
                found = db.Transactions
                    .FirstOrDefault(t => t.Id == id);
            }

            Console.WriteLine(found == null
                ? "Запись с таким ID не найдена."
                : "Найдено: " + found);
        }

        private static void ShowPages<T>(
            List<T> items, string title)
        {
            if (items.Count == 0)
            {
                Console.WriteLine(title + ": записей нет.");
                return;
            }

            int pages = (items.Count + PageSize - 1) / PageSize;
            for (int page = 0; page < pages; page++)
            {
                Console.WriteLine(
                    "\n" + title + ": страница " + (page + 1) +
                    "/" + pages + ", всего " + items.Count);
                foreach (T item in items
                    .Skip(page * PageSize).Take(PageSize))
                {
                    Console.WriteLine(item);
                }

                if (page == pages - 1)
                {
                    break;
                }

                Console.Write("Enter — далее, q — выход: ");
                if ((Console.ReadLine() ?? "")
                    .Trim().ToLowerInvariant() == "q")
                {
                    break;
                }
            }
        }
        

        private static int NewKey(
            IEnumerable<int> keys, string title)
        {
            HashSet<int> used = new HashSet<int>(keys);
            int free = used.Count == 0 ? 1 : used.Max() + 1;
            Console.WriteLine("Свободный ID: " + free);

            while (true)
            {
                int id = ConsoleHelper.ReadIdOrCancel(
                    title + " (0 — отмена): ");
                if (id == 0)
                {
                    return 0;
                }

                if (!used.Contains(id))
                {
                    return id;
                }

                Console.WriteLine("ID " + id + " уже занят.");
            }
        }

        private static int ForeignKey(
            IEnumerable<int> keys, string title)
        {
            HashSet<int> used = new HashSet<int>(keys);
            while (true)
            {
                int id = ConsoleHelper.ReadIdOrCancel(
                    title + " (0 — отмена): ");
                if (id == 0)
                {
                    return 0;
                }

                if (used.Contains(id))
                {
                    return id;
                }

                Console.WriteLine(
                    "Записи с ID " + id + " не существует.");
            }
        }
        

        public static void Add(Database db)
        {
            int table = ChooseTable("Добавление");
            if (table == 0)
            {
                return;
            }

            bool added = false;
            if (table == 1)
            {
                added = AddAccount(db);
            }
            else if (table == 2)
            {
                added = AddRate(db);
            }
            else
            {
                added = AddTransaction(db);
            }

            if (added)
            {
                db.HasChanges = true;
                Console.WriteLine("Запись добавлена.");
                PrintCounters(db);
            }
            else
            {
                Console.WriteLine("Добавление отменено.");
            }
        }

        private static bool AddAccount(Database db)
        {
            int id = NewKey(
                db.Accounts.Select(a => a.Id), "ID счёта");
            if (id == 0)
            {
                return false;
            }

            string name = ConsoleHelper.ReadString(
                "ФИО владельца: ", 60);
            DateTime date = ConsoleHelper.ReadDate(
                "Дата открытия (ДД.ММ.ГГГГ): ");
            db.Accounts.Add(new Account(id, name, date));
            return true;
        }

        private static bool AddRate(Database db)
        {
            int id = NewKey(
                db.Rates.Select(r => r.Id), "ID валюты");
            if (id == 0)
            {
                return false;
            }

            HashSet<string> codes = new HashSet<string>(
                db.Rates.Select(r => r.Code),
                StringComparer.OrdinalIgnoreCase);

            string code = "";
            while (true)
            {
                code = ConsoleHelper.ReadString(
                    "Буквенный код (3 латинские буквы): ", 3)
                    .ToUpperInvariant();
                if (code.Length == 3 &&
                    code.All(c => c >= 'A' && c <= 'Z') &&
                    !codes.Contains(code))
                {
                    break;
                }

                Console.WriteLine(
                    "Код — ровно 3 буквы A-Z, без повторов.");
            }

            double rate = ConsoleHelper.ReadPositiveDouble(
                "Курс к рублю: ");
            string name = ConsoleHelper.ReadString(
                "Полное наименование: ", 80);
            db.Rates.Add(new CurrencyRate(id, code, rate, name));
            return true;
        }

        private static bool AddTransaction(Database db)
        {
            if (db.Accounts.Count == 0 || db.Rates.Count == 0)
            {
                Console.WriteLine(
                    "Сначала добавьте счёт и валюту.");
                return false;
            }

            int id = NewKey(
                db.Transactions.Select(t => t.Id),
                "ID поступления");
            if (id == 0)
            {
                return false;
            }

            int accountId = ForeignKey(
                db.Accounts.Select(a => a.Id), "ID счёта");
            if (accountId == 0)
            {
                return false;
            }

            int currencyId = ForeignKey(
                db.Rates.Select(r => r.Id), "ID валюты");
            if (currencyId == 0)
            {
                return false;
            }

            DateTime date = ConsoleHelper.ReadDate(
                "Дата операции (ДД.ММ.ГГГГ): ");
            double amount = ConsoleHelper.ReadNonZeroDouble(
                "Сумма (может быть отрицательной): ");
            db.Transactions.Add(new Transaction(
                id, accountId, currencyId, date, amount));
            return true;
        }
        

        public static void Delete(Database db)
        {
            int table = ChooseTable("Удаление по ключу");
            if (table == 0)
            {
                return;
            }

            bool removed = false;
            if (table == 1)
            {
                removed = DeleteByKey(
                    db.Accounts, a => a.Id, "счёт",
                    id => db.Transactions
                        .Count(t => t.AccountId == id),
                    id => db.Transactions
                        .RemoveAll(t => t.AccountId == id));
            }
            else if (table == 2)
            {
                removed = DeleteByKey(
                    db.Rates, r => r.Id, "валюта",
                    id => db.Transactions
                        .Count(t => t.CurrencyId == id),
                    id => db.Transactions
                        .RemoveAll(t => t.CurrencyId == id));
            }
            else
            {
                removed = DeleteByKey(
                    db.Transactions, t => t.Id, "поступление",
                    id => 0, id => { });
            }

            if (removed)
            {
                db.HasChanges = true;
                PrintCounters(db);
            }
        }


        private static bool DeleteByKey<T>(
            List<T> table,
            Func<T, int> key,
            string title,
            Func<int, int> countRelated,
            Action<int> removeRelated) where T : class
        {
            int id = ConsoleHelper.ReadIdOrCancel(
                "ID (" + title + "), 0 — отмена: ");
            if (id == 0)
            {
                Console.WriteLine("Удаление отменено.");
                return false;
            }

            T item = table.FirstOrDefault(x => key(x) == id);
            if (item == null)
            {
                Console.WriteLine(
                    "Запись с ID " + id + " не найдена.");
                return false;
            }

            Console.WriteLine("Найдено: " + item);
            int related = countRelated(id);
            if (related > 0)
            {
                Console.WriteLine(
                    "Связанных поступлений: " + related +
                    ". Будут удалены вместе с записью.");
            }

            if (!ConsoleHelper.ReadYesNo(
                "Подтвердить удаление?"))
            {
                Console.WriteLine("Удаление отменено.");
                return false;
            }

            removeRelated(id);
            table.Remove(item);
            Console.WriteLine(table.Any(x => key(x) == id)
                ? "ОШИБКА: запись не удалена."
                : "Запись удалена.");
            return true;
        }

        private static void ReadPeriod(
            IEnumerable<DateTime> dates,
            out DateTime start,
            out DateTime end)
        {
            List<DateTime> all = dates.ToList();
            start = all.Min();
            end = all.Max();
            Console.WriteLine(
                "Доступный период: " +
                start.ToString("dd.MM.yyyy") + " — " +
                end.ToString("dd.MM.yyyy"));

            if (!ConsoleHelper.ReadYesNo(
                "Задать период вручную?"))
            {
                return;
            }

            do
            {
                start = ConsoleHelper.ReadDate(
                    "Начало периода: ");
                end = ConsoleHelper.ReadDate(
                    "Конец периода: ");
                if (start > end)
                {
                    Console.WriteLine(
                        "Начало не может быть позже конца.");
                }
            }
            while (start > end);
        }

        //запрос 1: одна таблица, перечень

        public static void Query1(Database db)
        {
            Console.WriteLine(
                "\nЗапрос 1 (одна таблица, перечень):");
            Console.WriteLine(
                "Счета, открытые в указанный период, " +
                "отсортированные по дате открытия.");

            if (db.Accounts.Count == 0)
            {
                Console.WriteLine("Таблица «Счета» пуста.");
                return;
            }

            DateTime start;
            DateTime end;
            ReadPeriod(
                db.Accounts.Select(a => a.OpenDate),
                out start, out end);

            List<Account> result = (
                from account in db.Accounts
                where account.OpenDate >= start &&
                      account.OpenDate <= end
                orderby account.OpenDate, account.Id
                select account).ToList();

            Console.WriteLine(
                "\nНайдено счетов: " + result.Count);
            ShowPages(result, "Результат запроса 1");
        }

        //запрос 2: две таблицы, значение

        public static void Query2(Database db)
        {
            Console.WriteLine(
                "\nЗапрос 2 (две таблицы, значение):");
            Console.WriteLine(
                "Суммарный объём всех поступлений " +
                "в рублях за указанный период.");

            if (db.Transactions.Count == 0)
            {
                Console.WriteLine(
                    "Таблица «Поступления» пуста.");
                return;
            }

            DateTime start;
            DateTime end;
            ReadPeriod(
                db.Transactions.Select(t => t.Date),
                out start, out end);

            List<double> rubles = (
                from transaction in db.Transactions
                join rate in db.Rates
                    on transaction.CurrencyId equals rate.Id
                where transaction.Date >= start &&
                      transaction.Date <= end
                select transaction.Amount * rate.Rate).ToList();

            Console.WriteLine(
                "\nОпераций учтено: " + rubles.Count);
            Console.WriteLine(
                "Сумма: " + rubles.Sum().ToString("F2") +
                " руб.");
            Console.WriteLine(
                "Целая часть: " +
                Math.Truncate(rubles.Sum()).ToString("F0"));
        }

        //запрос 3: три таблицы, значение

        public static void Query3(Database db)
        {
            Console.WriteLine(
                "\nЗапрос 3 (три таблицы, значение):");
            Console.WriteLine(
                "Фамилия держателя счёта с максимальным " +
                "начислением в рублях за период.");

            if (db.Transactions.Count == 0)
            {
                Console.WriteLine(
                    "Таблица «Поступления» пуста.");
                return;
            }

            DateTime start;
            DateTime end;
            ReadPeriod(
                db.Transactions.Select(t => t.Date),
                out start, out end);

            var best = (
                from transaction in db.Transactions
                join rate in db.Rates
                    on transaction.CurrencyId equals rate.Id
                join account in db.Accounts
                    on transaction.AccountId equals account.Id
                where transaction.Date >= start &&
                      transaction.Date <= end
                group transaction.Amount * rate.Rate
                    by account.FullName into holder
                select new
                {
                    Name = holder.Key,
                    Total = holder.Sum()
                })
                .OrderByDescending(x => x.Total)
                .ThenBy(x => x.Name)
                .FirstOrDefault();

            if (best == null)
            {
                Console.WriteLine("\nЗа период операций нет.");
                return;
            }

            string surname = best.Name.Split(
                new[] { ' ' },
                StringSplitOptions.RemoveEmptyEntries)[0];

            Console.WriteLine("\nДержатель: " + best.Name);
            Console.WriteLine(
                "Начисление: " +
                best.Total.ToString("F2") + " руб.");
            Console.WriteLine(
                "ОТВЕТ: " + surname.ToUpperInvariant());
        }

        // запрос 4: три таблицы, перечень

        public static void Query4(Database db)
        {
            Console.WriteLine(
                "\nЗапрос 4 (три таблицы, перечень):");
            Console.WriteLine(
                "По выбранной валюте: держатели счетов, " +
                "число операций и сумма в рублях " +
                "по убыванию суммы.");

            if (db.Transactions.Count == 0 ||
                db.Rates.Count == 0)
            {
                Console.WriteLine("Недостаточно данных.");
                return;
            }

            foreach (CurrencyRate rate in
                db.Rates.OrderBy(r => r.Id))
            {
                Console.WriteLine("  " + rate);
            }

            int currencyId = ForeignKey(
                db.Rates.Select(r => r.Id), "ID валюты");
            if (currencyId == 0)
            {
                Console.WriteLine("Запрос отменён.");
                return;
            }

            DateTime start;
            DateTime end;
            ReadPeriod(
                db.Transactions.Select(t => t.Date),
                out start, out end);
            int top = ConsoleHelper.ReadIntInRange(
                "Сколько строк вывести (1-100): ", 1, 100);

            var result = (
                from transaction in db.Transactions
                join rate in db.Rates
                    on transaction.CurrencyId equals rate.Id
                join account in db.Accounts
                    on transaction.AccountId equals account.Id
                where transaction.CurrencyId == currencyId &&
                      transaction.Date >= start &&
                      transaction.Date <= end
                group new
                {
                    account.FullName,
                    Rubles = transaction.Amount * rate.Rate
                }
                by account.FullName into holder
                orderby holder.Sum(x => x.Rubles) descending
                select new
                {
                    Name = holder.Key,
                    Count = holder.Count(),
                    Total = holder.Sum(x => x.Rubles)
                })
                .Take(top)
                .ToList();

            if (result.Count == 0)
            {
                Console.WriteLine(
                    "\nПо этой валюте операций нет.");
                return;
            }

            Console.WriteLine(
                "\n№   ФИО                       " +
                "Операций  Сумма, руб.");
            for (int i = 0; i < result.Count; i++)
            {
                Console.WriteLine(
                    (i + 1).ToString().PadRight(4) +
                    result[i].Name.PadRight(26) +
                    result[i].Count.ToString().PadRight(10) +
                    result[i].Total.ToString("F2"));
            }
        }
    }
}