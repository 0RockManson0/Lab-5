using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using NPOI.HSSF.UserModel;
using NPOI.SS.UserModel;
using NPOI.XSSF.UserModel;

namespace Lab5
{
    internal static class ExcelService
    {
        public static Database Load(string path)
        {
            if (!File.Exists(path))
            {
                throw new FileNotFoundException(
                    "Файл не найден: " + path);
            }

            IWorkbook book = WorkbookFactory.Create(path);
            Database db = new Database();
            db.SourcePath = Path.GetFullPath(path);

            ISheet accounts = FindSheet(book, 0, "Счета");
            ISheet rates = FindSheet(
                book, 1, "Курс валют", "Валюты");
            ISheet transactions = FindSheet(
                book, 2, "Поступления", "Начисления");

            if (accounts == null || rates == null ||
                transactions == null)
            {
                throw new InvalidDataException(
                    "В файле не найдены все три таблицы.");
            }

            db.Accounts.AddRange(
                ReadTable<Account>(accounts, (id, row) =>
                {
                    string name = Text(row, 1);
                    DateTime date;
                    if (name.Length == 0 ||
                        !TryDate(row, 2, out date))
                    {
                        return null;
                    }

                    return new Account(id, name, date);
                }));

            db.Rates.AddRange(
                ReadTable<CurrencyRate>(rates, (id, row) =>
                {
                    string code = Text(row, 1);
                    double rate;
                    if (code.Length == 0 ||
                        !TryDouble(row, 2, out rate) ||
                        rate <= 0)
                    {
                        return null;
                    }

                    return new CurrencyRate(
                        id, code, rate, Text(row, 3));
                }));

            db.Transactions.AddRange(
                ReadTable<Transaction>(transactions, (id, row) =>
                {
                    int accountId;
                    int currencyId;
                    DateTime date;
                    double amount;
                    if (!TryInt(row, 1, out accountId) ||
                        accountId <= 0 ||
                        !TryInt(row, 2, out currencyId) ||
                        currencyId <= 0 ||
                        !TryDate(row, 3, out date) ||
                        !TryDouble(row, 4, out amount))
                    {
                        return null;
                    }

                    return new Transaction(
                        id, accountId, currencyId, date, amount);
                }));

            if (db.Accounts.Count == 0 || db.Rates.Count == 0)
            {
                throw new InvalidDataException(
                    "Таблицы «Счета» или «Курс валют» пусты.");
            }

            return db;
        }

        public static void Save(Database db, string path)
        {
            IWorkbook book = path.EndsWith(
                ".xlsx", StringComparison.OrdinalIgnoreCase)
                ? (IWorkbook)new XSSFWorkbook()
                : new HSSFWorkbook();

            ICellStyle headerStyle = book.CreateCellStyle();
            IFont font = book.CreateFont();
            font.IsBold = true;
            headerStyle.SetFont(font);

            ICellStyle dateStyle = book.CreateCellStyle();
            dateStyle.DataFormat =
                book.CreateDataFormat().GetFormat("dd.mm.yyyy");

            WriteSheet(book, "Счета",
                new[] { "ID", "ФИО", "Дата открытия вклада" },
                db.Accounts.Select(a => new object[]
                    { a.Id, a.FullName, a.OpenDate }),
                headerStyle, dateStyle);

            WriteSheet(book, "Курс валют",
                new[]
                {
                    "ID", "Буквенный код", "Курс",
                    "Полное наименование"
                },
                db.Rates.Select(r => new object[]
                    { r.Id, r.Code, r.Rate, r.Name }),
                headerStyle, dateStyle);

            WriteSheet(book, "Поступления",
                new[]
                {
                    "ID", "ID счёта", "ID валюты",
                    "Дата", "Сумма"
                },
                db.Transactions.Select(t => new object[]
                {
                    t.Id, t.AccountId, t.CurrencyId,
                    t.Date, t.Amount
                }),
                headerStyle, dateStyle);

            using (FileStream stream = new FileStream(
                path, FileMode.Create, FileAccess.Write))
            {
                book.Write(stream);
            }
        }
        
        private static List<T> ReadTable<T>(
            ISheet sheet, Func<int, IRow, T> map)
            where T : class
        {
            List<T> items = new List<T>();
            HashSet<int> keys = new HashSet<int>();
            int i = 0;

            for (i = 0; i <= sheet.LastRowNum; i++)
            {
                IRow row = sheet.GetRow(i);
                int id;
                if (!TryInt(row, 0, out id) || id <= 0 ||
                    !keys.Add(id))
                {
                    continue;
                }

                T item = map(id, row);
                if (item != null)
                {
                    items.Add(item);
                }
            }

            return items;
        }
        
        private static void WriteSheet(
            IWorkbook book,
            string name,
            string[] headers,
            IEnumerable<object[]> rows,
            ICellStyle headerStyle,
            ICellStyle dateStyle)
        {
            ISheet sheet = book.CreateSheet(name);
            IRow header = sheet.CreateRow(0);
            int i = 0;

            for (i = 0; i < headers.Length; i++)
            {
                ICell cell = header.CreateCell(i);
                cell.SetCellValue(headers[i]);
                cell.CellStyle = headerStyle;
                sheet.SetColumnWidth(i, 20 * 256);
            }

            int index = 1;
            foreach (object[] values in rows)
            {
                IRow row = sheet.CreateRow(index);
                index++;
                for (i = 0; i < values.Length; i++)
                {
                    ICell cell = row.CreateCell(i);
                    if (values[i] is DateTime)
                    {
                        cell.SetCellValue((DateTime)values[i]);
                        cell.CellStyle = dateStyle;
                    }
                    else if (values[i] is int)
                    {
                        cell.SetCellValue((int)values[i]);
                    }
                    else if (values[i] is double)
                    {
                        cell.SetCellValue((double)values[i]);
                    }
                    else
                    {
                        cell.SetCellValue(
                            Convert.ToString(values[i]));
                    }
                }
            }
        }

        private static ISheet FindSheet(
            IWorkbook book, int index, params string[] names)
        {
            int i = 0;
            for (i = 0; i < book.NumberOfSheets; i++)
            {
                string title =
                    (book.GetSheetName(i) ?? "").Trim();
                if (names.Any(n => string.Equals(
                    title, n,
                    StringComparison.OrdinalIgnoreCase)))
                {
                    return book.GetSheetAt(i);
                }
            }

            return index < book.NumberOfSheets
                ? book.GetSheetAt(index)
                : null;
        }

        private static ICell Cell(IRow row, int index)
        {
            return row == null ? null : row.GetCell(index);
        }

        private static CellType TypeOf(ICell cell)
        {
            if (cell == null)
            {
                return CellType.Blank;
            }

            return cell.CellType == CellType.Formula
                ? cell.CachedFormulaResultType
                : cell.CellType;
        }

        private static string Text(IRow row, int index)
        {
            ICell cell = Cell(row, index);
            return TypeOf(cell) == CellType.String
                ? (cell.StringCellValue ?? "").Trim()
                : "";
        }

        private static bool TryInt(
            IRow row, int index, out int value)
        {
            value = 0;
            ICell cell = Cell(row, index);
            CellType type = TypeOf(cell);

            if (type == CellType.Numeric)
            {
                double number = cell.NumericCellValue;
                if (!double.IsFinite(number) ||
                    Math.Abs(number - Math.Round(number)) > 1e-9 ||
                    number < int.MinValue ||
                    number > int.MaxValue)
                {
                    return false;
                }

                value = (int)Math.Round(number);
                return true;
            }

            if (type == CellType.String)
            {
                return int.TryParse(
                    (cell.StringCellValue ?? "").Trim(),
                    NumberStyles.Integer,
                    CultureInfo.InvariantCulture,
                    out value);
            }

            return false;
        }

        private static bool TryDouble(
            IRow row, int index, out double value)
        {
            value = 0;
            ICell cell = Cell(row, index);
            CellType type = TypeOf(cell);

            if (type == CellType.Numeric)
            {
                value = cell.NumericCellValue;
                return double.IsFinite(value);
            }

            if (type == CellType.String)
            {
                return double.TryParse(
                    (cell.StringCellValue ?? "")
                        .Trim().Replace(',', '.'),
                    NumberStyles.Float,
                    CultureInfo.InvariantCulture,
                    out value) && double.IsFinite(value);
            }

            return false;
        }

        private static bool TryDate(
            IRow row, int index, out DateTime value)
        {
            value = DateTime.MinValue;
            ICell cell = Cell(row, index);
            CellType type = TypeOf(cell);

            if (type == CellType.Numeric)
            {
                double number = cell.NumericCellValue;
                if (!double.IsFinite(number) ||
                    number < 1 || number > 100000)
                {
                    return false;
                }

                value = DateUtil.GetJavaDate(number);
                return true;
            }

            if (type == CellType.String)
            {
                return DateTime.TryParseExact(
                    (cell.StringCellValue ?? "").Trim(),
                    "dd.MM.yyyy",
                    CultureInfo.InvariantCulture,
                    DateTimeStyles.None,
                    out value);
            }

            return false;
        }
    }
}