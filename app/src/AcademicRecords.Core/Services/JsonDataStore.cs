//Завантажує та безпечно зберігає академічні дані у форматі JSON
using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Reflection;
using AcademicRecords.Core.Interfaces;
using AcademicRecords.Core.Models;

namespace AcademicRecords.Core.Services;

public sealed class JsonDataStore : IDataStore
{
    private readonly string _path;
    private static readonly JsonSerializerOptions Options = new()
    {
        WriteIndented = true,
        Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping,
        PropertyNameCaseInsensitive = false,
        UnmappedMemberHandling = JsonUnmappedMemberHandling.Disallow
    };
//Зберігає повний шлях до файлу даних
    public JsonDataStore(string path) => _path = Path.GetFullPath(path);

//Читає JSON, перевіряє його структуру та повертає дані
    public AcademicData Load()
    {
        if (!File.Exists(_path)) return new AcademicData();
        using var stream = File.OpenRead(_path);
        using var document = JsonDocument.Parse(stream);
        EnsureConstructorParametersPresent(document.RootElement);
        var data = JsonSerializer.Deserialize<AcademicData>(document.RootElement, Options)
            ?? throw new JsonException("Файл даних містить null.");
        AcademicService.FromData(data);
        return data;
    }

//Перевіряє, що у JSON присутні поля, потрібні конструкторам моделей
    private static void EnsureConstructorParametersPresent(JsonElement root)
    {
        if (root.ValueKind != JsonValueKind.Object) return;

        foreach (var collection in typeof(AcademicData).GetProperties())
        {
            if (!root.TryGetProperty(collection.Name, out var items) || items.ValueKind != JsonValueKind.Array)
                continue;

            var itemType = collection.PropertyType.GetGenericArguments().FirstOrDefault();
            var constructor = itemType?.GetConstructors(BindingFlags.Public | BindingFlags.Instance)
                .OrderByDescending(candidate => candidate.GetParameters().Length)
                .FirstOrDefault();
            if (itemType is null || constructor is null) continue;

            foreach (var item in items.EnumerateArray())
            {
                if (item.ValueKind != JsonValueKind.Object) continue;
                foreach (var parameter in constructor.GetParameters())
                {
                    if (parameter.IsOptional) continue;
                    var property = itemType.GetProperty(parameter.Name!, BindingFlags.Public | BindingFlags.Instance | BindingFlags.IgnoreCase);
                    if (property is null) continue;
                    var propertyName = property.GetCustomAttribute<JsonPropertyNameAttribute>()?.Name
                        ?? Options.PropertyNamingPolicy?.ConvertName(property.Name)
                        ?? property.Name;
                    if (!item.TryGetProperty(propertyName, out _))
                        throw new JsonException($"У JSON відсутнє обов'язкове поле '{propertyName}'.");
                }
            }
        }
    }

//Перевіряє дані та записує JSON із резервною копією попереднього файлу
    public void Save(AcademicData data)
    {
        AcademicService.FromData(data);
        var json = JsonSerializer.Serialize(data, Options);
        Directory.CreateDirectory(Path.GetDirectoryName(_path)!);
        var temporary = _path + "." + Guid.NewGuid().ToString("N") + ".tmp";
        try
        {
            //Запис у сусідній файл, потім атомарна заміна з резервною копією
            using (var stream = new FileStream(temporary, FileMode.CreateNew, FileAccess.Write, FileShare.None))
            using (var writer = new StreamWriter(stream, new System.Text.UTF8Encoding(false)))
            {
                writer.Write(json);
                writer.Flush();
                stream.Flush(flushToDisk: true);
            }
            if (File.Exists(_path)) File.Replace(temporary, _path, _path + ".bak");
            else File.Move(temporary, _path);
        }
        finally
        {
            if (File.Exists(temporary)) File.Delete(temporary);
        }
    }
}
