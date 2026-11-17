//Універсальне сховище сутностей у пам'яті з базовими операціями
using AcademicRecords.Core.Interfaces;
using AcademicRecords.Core.Models;

namespace AcademicRecords.Core.Services;

public sealed class InMemoryRepository<T> : IRepository<T> where T : Entity
{
    private readonly List<T> _items = [];
//Повертає копію списку сутностей зі сховища
    public IReadOnlyList<T> GetAll() => _items.ToArray();
//Повертає сутність за її ідентифікатором або повідомляє про відсутність
    public T Get(Guid id) => _items.FirstOrDefault(x => x.Id == id)
        ?? throw new KeyNotFoundException("Запис із таким ідентифікатором не знайдено.");
//Додає сутність, якщо вона не null і її ідентифікатор унікальний
    public void Add(T item)
    {
        ArgumentNullException.ThrowIfNull(item);
        if (_items.Any(x => x.Id == item.Id)) throw new InvalidOperationException("Повторний ідентифікатор запису.");
        _items.Add(item);
    }
//Замінює наявну сутність із таким самим ідентифікатором
    public void Update(T item)
    {
        ArgumentNullException.ThrowIfNull(item);
        var index = _items.FindIndex(x => x.Id == item.Id);
        if (index < 0) throw new KeyNotFoundException("Запис для редагування не знайдено.");
        _items[index] = item;
    }
//Повертає сутність за її ідентифікатором або повідомляє про відсутність
    public void Remove(Guid id) => _items.Remove(Get(id));
}