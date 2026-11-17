//Створює набір демонстраційних даних для запуску програми
namespace AcademicRecords.Core.Services;

public static class DemoData
{
//Створює викладача, дисципліни, студентів, зарахування та приклади оцінок
    public static AcademicService Create()
    {
        var service = new AcademicService();
        var teacher = service.AddTeacher("Коваленко Олена Петрівна", "Інженерії програмного забезпечення");
        var oop = service.AddCourse("Об'єктно-орієнтоване програмування", teacher.Id, 3);
        var math = service.AddCourse("Дискретна математика", teacher.Id, 5);
        var a = service.AddStudent("Бондар Анна Олегівна", "ІПЗ-001", "ІПЗ-21");
        var b = service.AddStudent("Мельник Максим Ігорович", "ІПЗ-002", "ІПЗ-21");
        var c = service.AddStudent("Шевченко Софія Андріївна", "ІПЗ-003", "ІПЗ-21");
        var d = service.AddStudent("Ткаченко Артем Сергійович", "ІПЗ-004", "ІПЗ-22");
        foreach (var student in new[] { a, b, c }) { service.Enroll(student.Id, oop.Id); service.Enroll(student.Id, math.Id); }
        service.Enroll(d.Id, math.Id);
        service.SetGrade(a.Id, oop.Id, 95); service.SetGrade(a.Id, math.Id, 85);
        service.SetGrade(b.Id, oop.Id, 80); service.SetGrade(b.Id, math.Id, 100);
        service.SetGrade(c.Id, oop.Id, 59); service.SetGrade(c.Id, math.Id, 75);
        return service;
    }
}