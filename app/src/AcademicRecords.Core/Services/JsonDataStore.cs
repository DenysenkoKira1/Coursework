//Завантажує та безпечно зберігає академічні дані у форматі JSON
using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.Json.Serialization;
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
        UnmappedMemberHandling = JsonUnmappedMemberHandling.Disallow,
        RespectRequiredConstructorParameters = true
    };
//Зберігає повний шлях до файлу даних
    public JsonDataStore(string path) => _path = Path.GetFullPath(path);

//Читає JSON, перевіряє його структуру та повертає дані
    public AcademicData Load()
    {
        if (!File.Exists(_path)) return new AcademicData();
        using var stream = File.OpenRead(_path);
        var data = JsonSerializer.Deserialize<AcademicData>(stream, Options)
            ?? throw new JsonException("Файл даних містить null.");
        AcademicService.FromData(data);
        return data;
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