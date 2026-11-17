//Модель усіх даних системи, які зберігаються у файлі
using System.Text.Json.Serialization;

namespace AcademicRecords.Core.Models;

//Формат файлу. Після читання перевіряється весь граф зв'язків
public sealed class AcademicData
{
    [JsonRequired] public int SchemaVersion { get; set; } = 1;
    [JsonRequired] public List<Student> Students { get; set; } = [];
    [JsonRequired] public List<Teacher> Teachers { get; set; } = [];
    [JsonRequired] public List<Course> Courses { get; set; } = [];
    [JsonRequired] public List<Enrollment> Enrollments { get; set; } = [];
    [JsonRequired] public List<GradeRecord> Grades { get; set; } = [];
}