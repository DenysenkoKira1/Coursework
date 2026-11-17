//Базовий клас людини з ім'ям і спільним методом формування опису
namespace AcademicRecords.Core.Models;

public abstract class Person : Entity
{
    public string FullName { get; }
//Створює людину з перевіреним іменем
    protected Person(Guid id, string fullName) : base(id)
        => FullName = Required(fullName, "ПІБ");
//Повертає текстовий опис людини, визначений у похідному класі
    public abstract string Describe();
}