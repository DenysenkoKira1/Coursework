//Модель навчальної дисципліни, її викладача та місткості
namespace AcademicRecords.Core.Models;

public sealed class Course : Entity
{
    public string Title { get; }
    public Guid TeacherId { get; }
    public int Capacity { get; }

//Створює дисципліну та перевіряє її назву, викладача й місткість
    public Course(Guid id, string title, Guid teacherId, int capacity) : base(id)
    {
        Title = Required(title, "Назва дисципліни");
        if (teacherId == Guid.Empty) throw new ArgumentException("Потрібно вказати викладача.");
        if (capacity is < 1 or > 1000) throw new ArgumentException("Місткість має бути від 1 до 1000.");
        TeacherId = teacherId;
        Capacity = capacity;
    }
}