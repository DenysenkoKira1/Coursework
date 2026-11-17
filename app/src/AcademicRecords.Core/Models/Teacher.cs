//Модель викладача та кафедри, на якій він працює
namespace AcademicRecords.Core.Models;

public sealed class Teacher : Person
{
    public string Department { get; }
//Створює викладача та перевіряє назву кафедри
    public Teacher(Guid id, string fullName, string department) : base(id, fullName)
        => Department = Required(department, "Кафедра");
//Формує короткий опис викладача та його кафедри
    public override string Describe() => $"{FullName}; кафедра {Department}";
}