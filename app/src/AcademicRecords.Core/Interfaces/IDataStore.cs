//Контракт для завантаження та збереження даних академічної системи
using AcademicRecords.Core.Models;

namespace AcademicRecords.Core.Interfaces;

public interface IDataStore
{
//Завантажує академічні дані зі сховища
    AcademicData Load();
//Зберігає передані академічні дані у сховищі.
    void Save(AcademicData data);
}