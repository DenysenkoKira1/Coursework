//Виняток, який повідомляє про перевищення кількості місць на дисципліні
namespace AcademicRecords.Core.Exceptions;

public sealed class CourseCapacityExceededException : InvalidOperationException
{
    public int Capacity { get; }
//Створює виняток із назвою дисципліни та її місткістю
    public CourseCapacityExceededException(string title, int capacity)
        : base($"На дисципліні «{title}» немає вільних місць. Ліміт: {capacity}.") => Capacity = capacity;
}