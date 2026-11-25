using System;
using System.Globalization;

namespace Lab5
{
    /// <summary>Проверка ввода с клавиатуры.</summary>
    internal static class ConsoleHelper
    {
        private const double Limit = 1e12;

        private static string ReadLine(string message)
        {
            Console.Write(message);
            return (Console.ReadLine() ?? "").Trim();
        }

        public static int ReadInt(string message)
        {
            int value = 0;
            while (!int.TryParse(ReadLine(message), out value))
            {
                Console.WriteLine("Ошибка. Введите целое число.");
            }

            return value;
        }

        public static int ReadIntInRange(
            string message, int min, int max)
        {
            int value = ReadInt(message);
            while (value < min || value > max)
            {
                Console.WriteLine(
                    "Число должно быть от " + min +
                    " до " + max + ".");
                value = ReadInt(message);
            }

            return value;
        }

        public static int ReadPositiveInt(string message)
        {
            return ReadIntInRange(message, 1, int.MaxValue);
        }

        /// <summary>ID. 0 — отмена операции.</summary>
        public static int ReadIdOrCancel(string message)
        {
            return ReadIntInRange(message, 0, int.MaxValue);
        }

        public static uint ReadUInt(string message)
        {
            uint value = 0;
            while (!uint.TryParse(ReadLine(message), out value))
            {
                Console.WriteLine(
                    "Ошибка. Введите целое " +
                    "неотрицательное число.");
            }

            return value;
        }

        public static uint ReadUIntInRange(
            string message, uint min, uint max)
        {
            uint value = ReadUInt(message);
            while (value < min || value > max)
            {
                Console.WriteLine(
                    "Число должно быть от " + min +
                    " до " + max + ".");
                value = ReadUInt(message);
            }

            return value;
        }

        public static double ReadDouble(string message)
        {
            double value = 0;
            while (!double.TryParse(
                ReadLine(message).Replace(',', '.'),
                NumberStyles.Float,
                CultureInfo.InvariantCulture,
                out value) ||
                !double.IsFinite(value) ||
                Math.Abs(value) > Limit)
            {
                Console.WriteLine(
                    "Ошибка. Введите число, " +
                    "|значение| не больше 1e12.");
            }

            return value;
        }

        public static double ReadPositiveDouble(string message)
        {
            double value = ReadDouble(message);
            while (value <= 0)
            {
                Console.WriteLine(
                    "Число должно быть больше нуля.");
                value = ReadDouble(message);
            }

            return value;
        }

        public static double ReadNonZeroDouble(string message)
        {
            double value = ReadDouble(message);
            while (value == 0)
            {
                Console.WriteLine(
                    "Сумма не может быть нулевой.");
                value = ReadDouble(message);
            }

            return value;
        }

        public static string ReadString(
            string message, int maxLength = 60)
        {
            string value = ReadLine(message);
            while (value.Length == 0 || value.Length > maxLength)
            {
                Console.WriteLine(
                    "Нужна непустая строка не длиннее " +
                    maxLength + " символов.");
                value = ReadLine(message);
            }

            return value;
        }

        public static DateTime ReadDate(string message)
        {
            DateTime value = DateTime.MinValue;
            while (!DateTime.TryParseExact(
                ReadLine(message),
                "dd.MM.yyyy",
                CultureInfo.InvariantCulture,
                DateTimeStyles.None,
                out value) ||
                value.Year < 1900 || value.Year > 2100)
            {
                Console.WriteLine(
                    "Формат ДД.ММ.ГГГГ, " +
                    "год от 1900 до 2100.");
            }

            return value;
        }

        public static bool ReadYesNo(string message)
        {
            string answer = ReadLine(message + " (д/н): ")
                .ToLowerInvariant();
            while (answer != "д" && answer != "да" &&
                answer != "y" && answer != "н" &&
                answer != "нет" && answer != "n")
            {
                Console.WriteLine("Введите 'д' или 'н'.");
                answer = ReadLine(message + " (д/н): ")
                    .ToLowerInvariant();
            }

            return answer == "д" || answer == "да" ||
                answer == "y";
        }

        public static void Pause()
        {
            Console.Write("\nНажмите Enter для продолжения...");
            Console.ReadLine();
        }
    }
}