using System;
using System.IO;
using System.Text;

namespace Lab5
{
    internal class Program
    {
        private const string DefaultFile = "LR5-var13.xls";

        private static void Main()
        {
            Console.OutputEncoding = Encoding.UTF8;
            Console.InputEncoding = Encoding.UTF8;
            Console.WriteLine(
                "Лабораторная работа №5. LINQ-запросы");
            Console.WriteLine(
                "Вариант 13 — «Инвестиционные счета»");

            Database db = LoadAtStart();
            if (db == null)
            {
                Console.WriteLine("База не загружена.");
                ConsoleHelper.Pause();
                return;
            }

            bool running = true;
            while (running)
            {
                int choice = 0;
                try
                {
                    PrintMenu(db);
                    choice = ConsoleHelper.ReadIntInRange(
                        "Ваш выбор: ", 0, 9);
                    switch (choice)
                    {
                        case 0:
                            running = !ConfirmExit(db);
                            break;
                        case 1:
                            DatabaseHelper.View(db);
                            break;
                        case 2:
                            DatabaseHelper.Add(db);
                            break;
                        case 3:
                            DatabaseHelper.Delete(db);
                            break;
                        case 4:
                            DatabaseHelper.Query1(db);
                            break;
                        case 5:
                            DatabaseHelper.Query2(db);
                            break;
                        case 6:
                            DatabaseHelper.Query3(db);
                            break;
                        case 7:
                            DatabaseHelper.Query4(db);
                            break;
                        case 8:
                            DatabaseHelper.Save(db);
                            break;
                        case 9:
                            db = Reload(db);
                            break;
                    }
                }
                catch (Exception exception)
                {
                    Console.WriteLine(
                        "\nОШИБКА: " + exception.Message);
                }

                if (running && choice != 0)
                {
                    ConsoleHelper.Pause();
                }
            }

            Console.WriteLine("Выход.");
        }

        private static void PrintMenu(Database db)
        {
            Console.WriteLine(
                "\n======================================");
            Console.WriteLine("Файл: " + db.SourcePath);
            DatabaseHelper.PrintCounters(db);
            if (db.HasChanges)
            {
                Console.WriteLine(
                    "ЕСТЬ НЕСОХРАНЁННЫЕ ИЗМЕНЕНИЯ");
            }

            Console.WriteLine(
                "======================================");
            Console.WriteLine("1 — Просмотр / поиск по ключу");
            Console.WriteLine("2 — Добавление элемента");
            Console.WriteLine("3 — Удаление по ключу");
            Console.WriteLine(
                "4 — Запрос 1: счета за период (перечень)");
            Console.WriteLine(
                "5 — Запрос 2: сумма в рублях (значение)");
            Console.WriteLine(
                "6 — Запрос 3: держатель с максимумом");
            Console.WriteLine(
                "7 — Запрос 4: статистика по валюте");
            Console.WriteLine("8 — Сохранить в Excel");
            Console.WriteLine("9 — Перезагрузить из файла");
            Console.WriteLine("0 — Выход");
        }

        private static Database LoadAtStart()
        {
            while (true)
            {
                Console.Write(
                    "\nПуть к Excel (Enter — " + DefaultFile +
                    ", 0 — выход): ");
                string input = (Console.ReadLine() ?? "").Trim();
                if (input == "0")
                {
                    return null;
                }

                string path = Resolve(
                    input.Length == 0 ? DefaultFile : input);
                try
                {
                    return DatabaseHelper.Load(path);
                }
                catch (Exception exception)
                {
                    Console.WriteLine(
                        "Не удалось загрузить: " +
                        exception.Message);
                }
            }
        }

        private static string Resolve(string path)
        {
            if (File.Exists(path))
            {
                return Path.GetFullPath(path);
            }

            DirectoryInfo dir =
                new DirectoryInfo(AppContext.BaseDirectory);
            int i = 0;
            for (i = 0; i < 4 && dir != null; i++)
            {
                string candidate =
                    Path.Combine(dir.FullName, path);
                if (File.Exists(candidate))
                {
                    return Path.GetFullPath(candidate);
                }

                dir = dir.Parent;
            }

            return path;
        }

        private static Database Reload(Database db)
        {
            if (db.HasChanges && !ConsoleHelper.ReadYesNo(
                "Изменения будут потеряны. Продолжить?"))
            {
                return db;
            }

            try
            {
                return DatabaseHelper.Load(db.SourcePath);
            }
            catch (Exception exception)
            {
                Console.WriteLine(
                    "Не удалось перезагрузить: " +
                    exception.Message);
                return db;
            }
        }

        private static bool ConfirmExit(Database db)
        {
            if (!db.HasChanges)
            {
                return true;
            }

            if (!ConsoleHelper.ReadYesNo(
                "Есть несохранённые изменения. Сохранить?"))
            {
                return true;
            }

            try
            {
                DatabaseHelper.Save(db);
                return !db.HasChanges;
            }
            catch (Exception exception)
            {
                Console.WriteLine(
                    "Не удалось сохранить: " +
                    exception.Message);
                return ConsoleHelper.ReadYesNo(
                    "Выйти без сохранения?");
            }
        }
    }
}