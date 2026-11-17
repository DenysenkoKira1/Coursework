//Модель зарахування студента на навчальну дисципліну
namespace AcademicRecords.Core.Models;

//Зарахування студента на дисципліну, незалежно від наявності оцінки
public sealed class Enrollment : Entity
{
    public Guid StudentId { get; }
    public Guid CourseId { get; }
//Створює зарахування та перевіряє ідентифікатори студента й дисципліни
    public Enrollment(Guid id, Guid studentId, Guid courseId) : base(id)
    {
        if (studentId == Guid.Empty || courseId == Guid.Empty)
            throw new ArgumentException("Зарахування потребує студента та дисципліни.");
        StudentId = studentId;
        CourseId = courseId;
    }
}