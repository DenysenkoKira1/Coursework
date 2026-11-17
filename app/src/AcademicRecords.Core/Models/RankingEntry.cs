//Запис рейтингу: місце студента та його середній бал
namespace AcademicRecords.Core.Models;

public sealed record RankingEntry(int Rank, Student Student, decimal Average);