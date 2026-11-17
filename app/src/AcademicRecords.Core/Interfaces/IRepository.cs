//Контракт для базових операцій читання та зміни сутностей у сховищі
using AcademicRecords.Core.Models;

namespace AcademicRecords.Core.Interfaces;

public interface IRepository<T> where T : Entity
{
//Повертає всі сутності зі сховища
    IReadOnlyList<T> GetAll();
//Знаходить сутність за ідентифікатором
    T Get(Guid id);
//Додає сутність до сховища
    void Add(T item);
//Оновлює наявну сутність у сховищі
    void Update(T item);
//Видаляє сутність за ідентифікатором
    void Remove(Guid id);
}