//Модель оцінки студента з перевіркою значення та пов'язаних ідентифікаторів
using AcademicRecords.Core.Exceptions;

namespace AcademicRecords.Core.Models;

public sealed class GradeRecord : Entity
{
    public Guid StudentId { get; }
    public Guid CourseId { get; }
    public int Points { get; }

//Створює оцінку та перевіряє бал і пов'язані ідентифікатори
    public GradeRecord(Guid id, Guid studentId, Guid courseId, int points) : base(id)
    {
        if (points is < 0 or > 100) throw new InvalidGradeValueException(points);
        if (studentId == Guid.Empty || courseId == Guid.Empty)
            throw new ArgumentException("Оцінка потребує студента та дисципліни.");
        StudentId = studentId;
        CourseId = courseId;
        Points = points;
    }
}