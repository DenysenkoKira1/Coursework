//Точка запуску програми: налаштовує консоль, читає параметри та запускає меню
using System.Globalization;
using System.Text;
using System.Text.Json;
using AcademicRecords.ConsoleApp;
using AcademicRecords.Core.Services;

Console.InputEncoding = Encoding.UTF8;
Console.OutputEncoding = Encoding.UTF8;
CultureInfo.CurrentCulture = CultureInfo.GetCultureInfo("uk-UA");
CultureInfo.CurrentUICulture = CultureInfo.CurrentCulture;

var path = Path.Combine("data", "academic.json");
var demo = false;
for (var i = 0; i < args.Length; i++)
{
    switch (args[i])
    {
        case "--data" when i + 1 < args.Length: path = args[++i]; break;
        case "--demo": demo = true; break;
        case "--help":
            Console.WriteLine("Деканат | --data <файл.json> | --demo (демодані лише для нового файлу)");
            return 0;
            default:
            Console.Error.WriteLine($"Невідомий або неповний параметр: {args[i]}. Довідка: --help");
            return 2;
    }
}

try
{
    var store = new JsonDataStore(path);
    var service = demo && !File.Exists(path) ? DemoData.Create() : AcademicService.FromData(store.Load());
    Console.WriteLine("ДЕКАНАТ ТА УСПІШНІСТЬ СТУДЕНТІВ | Варіант 4");
    Console.WriteLine($"Оцінки: 0–100. Прохідний бал: {AcademicService.PassingGrade}.");
    Console.WriteLine($"Файл даних: {Path.GetFullPath(path)}");
    Console.WriteLine($"Завантажено: студентів {service.Students.Count}, дисциплін {service.Courses.Count}.");
    return new ConsoleMenu(service, store).Run();
}
catch (Exception ex) when (ex is IOException or UnauthorizedAccessException or JsonException
    or ArgumentException or InvalidOperationException or KeyNotFoundException)
{
    Console.Error.WriteLine($"Не вдалося відкрити дані: {ex.Message}");
    Console.Error.WriteLine("Початковий файл не перезаписано. Перевірте файл або його копію .bak та повторіть запуск.");
    return 1;
}