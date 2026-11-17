//Підсумок успішності студента: середній бал, зарахування та борги
namespace AcademicRecords.Core.Models;

public sealed record StudentResult(Student Student, decimal? Average, int EnrollmentCount, int DebtCount);