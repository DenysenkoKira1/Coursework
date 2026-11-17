// Файл: Модель студента з номером залікової книжки та назвою групи.

namespace AcademicRecords.Core.Models;

public sealed class Student : Person
{
    public string RecordBookNumber { get; }
    public string GroupName { get; }

// Коментар: Створює студента та перевіряє номер залікової книжки й групу.
    public Student(Guid id, string fullName, string recordBookNumber, string groupName)
        : base(id, fullName)
    {
        RecordBookNumber = Required(recordBookNumber, "Номер залікової книжки");
        GroupName = Required(groupName, "Група");
    }

// Коментар: Формує короткий опис студента з ПІБ, групою та номером залікової.
    public override string Describe() => $"{FullName}; група {GroupName}; № {RecordBookNumber}";
}
