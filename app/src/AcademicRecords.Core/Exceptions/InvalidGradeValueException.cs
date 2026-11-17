//Виняток для оцінки, що виходить за допустимий діапазон від 0 до 100
namespace AcademicRecords.Core.Exceptions;

public sealed class InvalidGradeValueException : ArgumentException
{
    public int Value { get; }
//Створює виняток і зберігає недопустиме значення оцінки
    public InvalidGradeValueException(int value)
        : base($"Оцінка {value} недопустима. Дозволено цілі числа від 0 до 100.") => Value = value;
}