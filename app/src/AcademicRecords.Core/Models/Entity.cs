//Базовий клас сутностей із перевіркою ідентифікатора та текстових значень
namespace AcademicRecords.Core.Models;
//Спільна ідентичність усіх сутностей предметної області
public abstract class Entity
{
    public Guid Id { get; }

//Створює сутність і забороняє порожній ідентифікатор
    protected Entity(Guid id)
    {
        if (id == Guid.Empty) throw new ArgumentException("Ідентифікатор не може бути порожнім.");
        Id = id;
    }
//Перевіряє обов'язковий текст, його довжину та символи керування
    protected static string Required(string? value, string label)
    {
        if (string.IsNullOrWhiteSpace(value)) throw new ArgumentException($"{label}: значення обов'язкове.");
        var text = value.Trim();
        if (text.Length > 120 || text.Any(char.IsControl))
            throw new ArgumentException($"{label}: до 120 символів, без керівних символів.");
        return text;
    }
}