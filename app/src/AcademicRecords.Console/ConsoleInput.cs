//Допоміжні функції для читання та перевірки введення користувача
namespace AcademicRecords.ConsoleApp;
internal static class ConsoleInput
{
//Зчитує текст і перевіряє його довжину та допустимі символи
    public static string Text(string prompt)
    {
        while (true)
        {
            Console.Write(prompt);
            var text = Console.ReadLine() ?? throw new EndOfStreamException();
            if (!string.IsNullOrWhiteSpace(text) && text.Trim().Length <= 120 &&
            !text.Any(char.IsControl)) return text.Trim();
            Console.WriteLine("Помилка: введіть від 1 до 120 символів без керівних символів.");
        }
    }
//Зчитує ціле число, доки воно не потрапить у заданий діапазон
    public static int Number(string prompt, int min, int max)
    {
        while (true)
        {
            Console.Write(prompt);
            var text = Console.ReadLine() ?? throw new EndOfStreamException();
            if (int.TryParse(text, out var value) && value >= min && value <= max) return value;
            Console.WriteLine($"Помилка: введіть ціле число від {min} до {max}.");
        }
    }
//Показує список і повертає елемент, обраний користувачем
    public static T Pick<T>(IReadOnlyList<T> items, Func<T, string> label)
    {
        if (items.Count == 0) throw new InvalidOperationException("Список порожній. Спочатку
        додайте записи.");
        for (var i = 0; i < items.Count; i++) Console.WriteLine($"{i + 1}. {label(items[i])}");
        return items[Number("Оберіть номер: ", 1, items.Count) - 1];
    }
//Повертає true, якщо користувач підтвердив дію словом «так»
    public static bool Confirm(string action)
    {
        Console.Write($"{action} Введіть «так» для підтвердження: ");
        return string.Equals(Console.ReadLine() ?? throw new EndOfStreamException(), "так",
        StringComparison.OrdinalIgnoreCase);
    }
}