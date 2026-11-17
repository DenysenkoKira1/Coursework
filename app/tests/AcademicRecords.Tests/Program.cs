//Містить автономні перевірки правил академічної системи та JSON-сховища
using System.Text.Json;
using AcademicRecords.Core.Exceptions;
using AcademicRecords.Core.Models;
using AcademicRecords.Core.Services;

//Самодостатній виконавець перевірок: без NuGet, з кодом завершення 1 при помилці
var tests = new List<(string Name, Action Run)>();
//Додає перевірку до списку тестів
void Test(string name, Action action) => tests.Add((name, action));
//Перевіряє рівність очікуваного та фактичного значень
void Equal<T>(T expected, T actual)
{
    if (!EqualityComparer<T>.Default.Equals(expected, actual)) throw new Exception($"Очікувалося {expected}; отримано {actual}.");
}
//Перевіряє, що дія завершується винятком указаного типу
void Throws<T>(Action action) where T : Exception
{
    try { action(); } catch (T) { return; }
    throw new Exception($"Очікувався виняток {typeof(T).Name}.");
}
//Готує сервіс із викладачем, студентом, дисципліною та зарахуванням
(AcademicService Service, Student Student, Course Course) Fixture(int capacity = 2)
{
    var s = new AcademicService();
    var teacher = s.AddTeacher("Викладач", "ІПЗ");
    var student = s.AddStudent("Студент", "001", "ІПЗ-21");
    var course = s.AddCourse("ООП", teacher.Id, capacity);
    s.Enroll(student.Id, course.Id);
    return (s, student, course);
}
//Створює тимчасовий файл для перевірки та видаляє створену папку після неї
void WithFile(Action<string> action)
{
    var folder = Path.Combine(Path.GetTempPath(), "academic-tests-" + Guid.NewGuid().ToString("N"));
    Directory.CreateDirectory(folder);
    try { action(Path.Combine(folder, "data.json")); }
    finally { Directory.Delete(folder, true); } // Лише створений цим тестом унікальний каталог.
}

Test("Оцінки 0, 59, 60, 100 приймаються", () =>
{
    var (s, student, course) = Fixture();
    foreach (var points in new[] { 0, 59, 60, 100 }) { s.SetGrade(student.Id, course.Id, points); Equal(points, s.Grades.Single().Points); }
});
Test("Оцінки -1 та 101 викликають власний виняток", () =>
{
    var (s, student, course) = Fixture();
    foreach (var points in new[] { -1, 101 }) Throws<InvalidGradeValueException>(() => s.SetGrade(student.Id, course.Id, points));
    Equal(0, s.Grades.Count);
});
Test("59 є боргом, 60 не є боргом", () =>
{
    var (s, student, course) = Fixture();
    s.SetGrade(student.Id, course.Id, 59); Equal(1, s.GetResult(student.Id).DebtCount);
    s.SetGrade(student.Id, course.Id, 60); Equal(0, s.GetResult(student.Id).DebtCount);
});
Test("Відсутня оцінка є боргом, середній бал відсутній", () =>
{
    var (s, student, _) = Fixture(); Equal(1, s.GetResult(student.Id).DebtCount); Equal<decimal?>(null, s.GetResult(student.Id).Average);
});
Test("Студент без зарахувань не боржник і не учасник рейтингу", () =>
{
    var s = new AcademicService(); var student = s.AddStudent("А", "1", "Г");
    Equal(0, s.GetResult(student.Id).DebtCount); Equal(0, s.GetDebtors().Count); Equal(0, s.GetScholarshipRanking().Count);
});
Test("Середній бал демоданих 90, 90 та 67", () =>
{
    var s = DemoData.Create();
    Equal<decimal?>(90m, s.GetResult(s.Students.Single(x => x.RecordBookNumber == "ІПЗ-001").Id).Average);
    Equal<decimal?>(90m, s.GetResult(s.Students.Single(x => x.RecordBookNumber == "ІПЗ-002").Id).Average);
    Equal<decimal?>(67m, s.GetResult(s.Students.Single(x => x.RecordBookNumber == "ІПЗ-003").Id).Average);
});
Test("Округлення середнього бала до двох знаків", () =>
{
    var (s, student, course) = Fixture();
    s.SetGrade(student.Id, course.Id, 60);
    foreach (var value in new[] { 60, 61 }) { var c = s.AddCourse("Д", course.TeacherId, 2); s.Enroll(student.Id, c.Id); s.SetGrade(student.Id, c.Id, value); }
    Equal<decimal?>(60.33m, s.GetResult(student.Id).Average);
});
Test("Відсутня оцінка не підміняється нулем у середньому", () =>
{
    var (s, student, course) = Fixture(); s.SetGrade(student.Id, course.Id, 80);
    var c = s.AddCourse("Друга", course.TeacherId, 2); s.Enroll(student.Id, c.Id);
    Equal<decimal?>(80m, s.GetResult(student.Id).Average); Equal(1, s.GetResult(student.Id).DebtCount); Equal(0, s.GetScholarshipRanking().Count);
});
Test("Рейтинг виключає боржників та враховує рівні місця 1,1,3", () =>
{
    var s = DemoData.Create(); var course = s.Courses.First();
    var third = s.AddStudent("Яременко Іван", "005", "ІПЗ-21"); s.Enroll(third.Id, course.Id); s.SetGrade(third.Id, course.Id, 80);
    var rank = s.GetScholarshipRanking(); Equal(3, rank.Count); Equal(1, rank[0].Rank); Equal(1, rank[1].Rank); Equal(3, rank[2].Rank);
    Equal("ІПЗ-001", rank[0].Student.RecordBookNumber);
});
Test("Фільтр знаходить числовий борг та відсутню оцінку", () => Equal(2, DemoData.Create().GetDebtors().Count));
Test("Переповнення дисципліни не змінює зарахувань", () =>
{
    var (s, _, course) = Fixture(1); var another = s.AddStudent("Інший", "2", "Г");
    Throws<CourseCapacityExceededException>(() => s.Enroll(another.Id, course.Id)); Equal(1, s.Enrollments.Count);
});
Test("Повторне зарахування заборонено", () =>
{
    var (s, student, course) = Fixture(); Throws<InvalidOperationException>(() => s.Enroll(student.Id, course.Id)); Equal(1, s.Enrollments.Count);
});
Test("Оцінка без зарахування заборонена", () =>
{
    var (s, _, course) = Fixture(); var other = s.AddStudent("Інший", "2", "Г");
    Throws<InvalidOperationException>(() => s.SetGrade(other.Id, course.Id, 99)); Equal(0, s.Grades.Count);
});
Test("Зміна оцінки зберігає ідентифікатор та один запис", () =>
{
    var (s, student, course) = Fixture(); s.SetGrade(student.Id, course.Id, 59); var id = s.Grades.Single().Id;
    s.SetGrade(student.Id, course.Id, 95); Equal(1, s.Grades.Count); Equal(id, s.Grades.Single().Id); Equal(0, s.GetDebtors().Count);
});
Test("Видалення оцінки повертає борг", () =>
{
    var (s, student, course) = Fixture(); s.SetGrade(student.Id, course.Id, 60); s.RemoveGrade(student.Id, course.Id); Equal(1, s.GetDebtors().Count);
});
Test("Скасування зарахування прибирає оцінку та звільняє місце", () =>
{
    var (s, student, course) = Fixture(1); s.SetGrade(student.Id, course.Id, 80); s.Unenroll(student.Id, course.Id);
    Equal(0, s.Grades.Count); Equal(0, s.EnrolledCount(course.Id)); s.Enroll(student.Id, course.Id);
});
Test("Видалення студента каскадне", () =>
{
    var (s, student, course) = Fixture(); s.SetGrade(student.Id, course.Id, 80); s.RemoveStudent(student.Id);
    Equal(0, s.Students.Count); Equal(0, s.Enrollments.Count); Equal(0, s.Grades.Count); Equal(1, s.Courses.Count);
});
Test("Видалення дисципліни каскадне", () =>
{
    var (s, student, course) = Fixture(); s.SetGrade(student.Id, course.Id, 80); s.RemoveCourse(course.Id);
    Equal(0, s.Courses.Count); Equal(0, s.Enrollments.Count); Equal(0, s.Grades.Count); Equal(1, s.Students.Count);
});
Test("Пов'язаного викладача не можна видалити", () =>
{
    var (s, _, course) = Fixture(); Throws<InvalidOperationException>(() => s.RemoveTeacher(course.TeacherId)); Equal(1, s.Teachers.Count);
});
Test("Місткість не можна зменшити нижче заповнення", () =>
{
    var (s, _, course) = Fixture(); var other = s.AddStudent("Інший", "2", "Г"); s.Enroll(other.Id, course.Id);
    Throws<InvalidOperationException>(() => s.UpdateCourse(course.Id, "ООП", course.TeacherId, 1)); Equal(2, s.Courses.Single().Capacity);
});
Test("Порожнє ПІБ, некоректна місткість та Guid заборонені", () =>
{
    Throws<ArgumentException>(() => new Student(Guid.NewGuid(), " ", "1", "Г"));
    Throws<ArgumentException>(() => new Teacher(Guid.Empty, "А", "Б"));
    Throws<ArgumentException>(() => new Course(Guid.NewGuid(), "Д", Guid.NewGuid(), 0));
    Throws<ArgumentException>(() => new Course(Guid.NewGuid(), "Д", Guid.NewGuid(), 1001));
    Throws<ArgumentException>(() => new Student(Guid.NewGuid(), "А\nБ", "1", "Г"));
});
Test("Номер залікової унікальний незалежно від регістру", () =>
{
    var s = new AcademicService(); s.AddStudent("А", "ABC", "Г"); Throws<InvalidOperationException>(() => s.AddStudent("Б", "abc", "Г")); Equal(1, s.Students.Count);
});
Test("Редагування студента зберігає зв'язки", () =>
{
    var (s, student, _) = Fixture(); s.UpdateStudent(student.Id, "Нове ім'я", "002", "ІПЗ-22");
    Equal("Нове ім'я", s.Students.Single().FullName); Equal(student.Id, s.Enrollments.Single().StudentId);
});
Test("Пошук за ПІБ, групою та заліковою", () =>
{
    var s = DemoData.Create(); Equal(1, s.FindStudents("бондар").Count); Equal(3, s.FindStudents("іпз-21").Count); Equal(1, s.FindStudents("ІПЗ-004").Count);
});
Test("Неіснуючий викладач не створює дисципліну", () =>
{
    var s = new AcademicService(); Throws<KeyNotFoundException>(() => s.AddCourse("ООП", Guid.NewGuid(), 2)); Equal(0, s.Courses.Count);
});
Test("Поліморфний опис Person та стандартний IComparer", () =>
{
    Person student = new Student(Guid.NewGuid(), "А", "1", "Г"); Person teacher = new Teacher(Guid.NewGuid(), "Б", "ІПЗ");
    Equal(true, student.Describe().Contains("група")); Equal(true, teacher.Describe().Contains("кафедра"));
    Equal(true, new StudentNameComparer().Compare((Student)student, null) > 0);
});
Test("JSON: повний цикл зберігає ID, українські рядки та зв'язки", () => WithFile(path =>
{
    var original = DemoData.Create(); var store = new JsonDataStore(path); store.Save(original.Export());
    var loaded = AcademicService.FromData(store.Load()); Equal(4, loaded.Students.Count); Equal(7, loaded.Enrollments.Count);
    Equal(original.Students[0].Id, loaded.Students[0].Id); Equal(2, loaded.GetDebtors().Count); Equal(2, loaded.GetScholarshipRanking().Count);
    Equal(true, File.ReadAllText(path).Contains("Бондар"));
}));
Test("JSON: відсутній файл дає порожні дані", () => WithFile(path => Equal(0, new JsonDataStore(path).Load().Students.Count)));
Test("JSON: повторне збереження утворює попередню копію", () => WithFile(path =>
{
    var store = new JsonDataStore(path); var s = DemoData.Create(); store.Save(s.Export());
    s.AddStudent("Новий", "999", "Г"); store.Save(s.Export());
    Equal(5, store.Load().Students.Count); Equal(4, new JsonDataStore(path + ".bak").Load().Students.Count);
}));
Test("JSON: пошкоджений файл залишається незмінним", () => WithFile(path =>
{
    File.WriteAllText(path, "{broken"); Throws<JsonException>(() => new JsonDataStore(path).Load()); Equal("{broken", File.ReadAllText(path));
}));
Test("JSON: відсутній обов'язковий параметр конструктора відхиляється", () => WithFile(path =>
{
    File.WriteAllText(path, "{\"Students\":[{\"Id\":\"" + Guid.NewGuid() + "\",\"FullName\":\"А\",\"GroupName\":\"Г\"}]}");
    Throws<JsonException>(() => new JsonDataStore(path).Load());
}));
Test("JSON: невідома властивість та null відхиляються", () => WithFile(path =>
{
    File.WriteAllText(path, "{\"Typo\":1}"); Throws<JsonException>(() => new JsonDataStore(path).Load());
    File.WriteAllText(path, "null"); Throws<JsonException>(() => new JsonDataStore(path).Load());
}));
Test("Завантаження: непідтримувана версія та null-колекція", () =>
{
    Throws<InvalidOperationException>(() => AcademicService.FromData(new AcademicData { SchemaVersion = 2 }));
    Throws<InvalidOperationException>(() => AcademicService.FromData(new AcademicData { Grades = null! }));
});
Test("Завантаження: дубль Id та номера залікової", () =>
{
    var data = DemoData.Create().Export(); data.Students.Add(data.Students[0]); Throws<InvalidOperationException>(() => AcademicService.FromData(data));
});
Test("Завантаження: розірвані зв'язки, дубль оцінки, перевищення місткості", () =>
{
    var data = DemoData.Create().Export(); data.Teachers.Clear(); Throws<KeyNotFoundException>(() => AcademicService.FromData(data));
    data = DemoData.Create().Export(); data.Grades.Add(data.Grades[0]); Throws<InvalidOperationException>(() => AcademicService.FromData(data));
    data = DemoData.Create().Export(); data.Enrollments.Clear(); Throws<InvalidOperationException>(() => AcademicService.FromData(data));
    data = DemoData.Create().Export(); var course = data.Courses.Single(x => x.Capacity == 3);
    data.Courses[data.Courses.IndexOf(course)] = new Course(course.Id, course.Title, course.TeacherId, 1);
    Throws<CourseCapacityExceededException>(() => AcademicService.FromData(data));
});
Test("Невалідні дані не перезаписують справний файл", () => WithFile(path =>
{
    var store = new JsonDataStore(path); store.Save(DemoData.Create().Export()); var before = File.ReadAllText(path);
    Throws<InvalidOperationException>(() => store.Save(new AcademicData { SchemaVersion = 99 })); Equal(before, File.ReadAllText(path));
}));

var failed = 0;
foreach (var test in tests)
{
    try { test.Run(); Console.WriteLine($"PASS {test.Name}"); }
    catch (Exception ex) { failed++; Console.WriteLine($"FAIL {test.Name}: {ex.GetType().Name}: {ex.Message}"); }
}
Console.WriteLine($"Перевірок: {tests.Count}; успішно: {tests.Count - failed}; помилок: {failed}.");
return failed == 0 ? 0 : 1;