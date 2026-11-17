//Консольне меню для роботи з даними про студентів, викладачів, дисципліни та оцінки
using System.Text.Json;
using AcademicRecords.Core.Interfaces;
using AcademicRecords.Core.Models;
using AcademicRecords.Core.Services;

namespace AcademicRecords.ConsoleApp;
internal sealed class ConsoleMenu(AcademicService service, IDataStore store)
{
//Показує студентів і повертає обраного студента
    private Student PickStudent() => ConsoleInput.Pick(service.Students, x => x.Describe());
//Показує викладачів і повертає обраного викладача
    private Teacher PickTeacher() => ConsoleInput.Pick(service.Teachers, x => x.Describe());
//Показує дисципліни з кількістю вільних місць і повертає обрану
    private Course PickCourse() => ConsoleInput.Pick(service.Courses, x => $"{x.Title} ({service.EnrolledCount(x.Id)}/{x.Capacity})");

//Виконує головний цикл меню та обробляє команди користувача
    public int Run()
    {
        while (true)
        {
            Console.WriteLine("\n1 Студенти       2 Викладачі       3 Дисципліни");
            Console.WriteLine("4 Зарахування    5 Оцінки          6 Картка студента");
            Console.WriteLine("7 Рейтинг        8 Боржники        9 Пошук студентів");
            Console.WriteLine("10 Зберегти      0 Зберегти та вийти");
            try
            {
                switch (ConsoleInput.Number("Команда: ", 0, 10))
                {
                    case 0: Save(); return 0;
                    case 1: StudentsMenu(); break;
                    case 2: TeachersMenu(); break;
                    case 3: CoursesMenu(); break;
                    case 4: EnrollmentMenu(); break;
                    case 5: GradesMenu(); break;
                    case 6: PrintTranscript(PickStudent()); break;
                    case 7: PrintRanking(); break;
                    case 8: PrintDebtors(); break;
                    case 9:
                        PrintStudents(service.FindStudents(ConsoleInput.Text("ПІБ, група або номер
                        залікової: ")));
                        break;
                    case 10: Save(); break;
                }
            }
            catch (EndOfStreamException)
            {
                try { Save(); return 0; }
                catch (Exception ex) when (Expected(ex)) { Console.Error.WriteLine($"Не вдалося
                зберегти дані: {ex.Message}"); return 1; }
            }
            catch (Exception ex) when (Expected(ex))
            {
                Console.WriteLine($"Помилка: {ex.Message}");
            }
        }
    }
//Визначає помилки, які меню може показати як звичайне повідомлення
    private static bool Expected(Exception ex) => ex is ArgumentException or
    InvalidOperationException or KeyNotFoundException or IOException or
    UnauthorizedAccessException or JsonException;

//Зберігає поточні дані та повідомляє про результат
    private void Save()
    {
        store.Save(service.Export());
        Console.WriteLine("Дані збережено у JSON. Попередню версію збережено у .bak, якщо файл уже
        існував.");
    }
//Виводить список студентів і загальну кількість записів
    private static void PrintStudents(IReadOnlyList<Student> students)
    {
        Console.WriteLine("\nСТУДЕНТИ");
        foreach (var student in students) Console.WriteLine(student.Describe());
        Console.WriteLine($"Усього: {students.Count}");
    }
//Обробляє перегляд, додавання, редагування та видалення студентів
    private void StudentsMenu()
    {
        Console.WriteLine("СТУДЕНТИ: 1 Список | 2 Додати | 3 Редагувати | 4 Видалити | 0 Назад");
        switch (ConsoleInput.Number("Дія: ", 0, 4))
        {
            case 1: PrintStudents(service.Students); break;
            case 2:
                service.AddStudent(ConsoleInput.Text("ПІБ:"),ConsoleInput.Text("Номерзалікової:"), 
                ConsoleInput.Text("Група: "));
                Console.WriteLine("Студента додано."); break;
            case 3:
                var student = PickStudent();
                service.UpdateStudent(student.Id, ConsoleInput.Text("Нове ПІБ: "),
                ConsoleInput.Text("Номер залікової: "), ConsoleInput.Text("Група: "));
                Console.WriteLine("Студента оновлено."); break;
            case 4:
                var removed = PickStudent();
                if (ConsoleInput.Confirm("Видалити студента разом із його зарахуваннями та оцінками?"))
                { service.RemoveStudent(removed.Id); Console.WriteLine("Студента видалено."); }
                break;
        }
    }
//Обробляє перегляд, додавання, редагування та видалення викладачів
    private void TeachersMenu()
    {
        Console.WriteLine("ВИКЛАДАЧІ: 1 Список | 2 Додати | 3 Редагувати | 4 Видалити | 0 Назад");
        switch (ConsoleInput.Number("Дія: ", 0, 4))
        {
            case 1:
                foreach (var listedTeacher in service.Teachers)
                Console.WriteLine(listedTeacher.Describe());
                Console.WriteLine($"Усього: {service.Teachers.Count}"); break;
            case 2:
                service.AddTeacher(ConsoleInput.Text("ПІБ: "), ConsoleInput.Text("Кафедра: "));
                Console.WriteLine("Викладача додано."); break;
            case 3:
                var teacher = PickTeacher();
                service.UpdateTeacher(teacher.Id, ConsoleInput.Text("Нове ПІБ: "),
                ConsoleInput.Text("Кафедра: "));
                Console.WriteLine("Викладача оновлено."); break;
            case 4:
                var removed = PickTeacher();
                if (ConsoleInput.Confirm("Видалити викладача?"))
                { service.RemoveTeacher(removed.Id); Console.WriteLine("Викладача видалено."); }
                break;
        }
    }
//Обробляє перегляд, додавання, редагування та видалення дисциплін
    private void CoursesMenu()
    {
        Console.WriteLine("ДИСЦИПЛІНИ: 1 Список | 2 Додати | 3 Редагувати | 4 Видалити | 0 Назад");
        switch (ConsoleInput.Number("Дія: ", 0, 4))
        {
            case 1:
                foreach (var listedCourse in service.Courses)
                {
                    Console.WriteLine($"{listedCourse.Title} | місця 
                    {service.EnrolledCount(listedCourse.Id)}/{listedCourse.Capacity}");
                    Console.WriteLine($"  Викладач: {service.Teachers.Single(x => x.Id ==
                    listedCourse.TeacherId).FullName}");
                }
                Console.WriteLine($"Усього: {service.Courses.Count}"); break;
            case 2:
                var title = ConsoleInput.Text("Назва: ");
                var teacher = PickTeacher();
                service.AddCourse(title, teacher.Id, ConsoleInput.Number("Кількість місць: ", 1, 1000));
                Console.WriteLine("Дисципліну додано."); break;
            case 3:
                var course = PickCourse();
                var newTitle = ConsoleInput.Text("Нова назва: ");
                var newTeacher = PickTeacher();
                service.UpdateCourse(course.Id, newTitle, newTeacher.Id, ConsoleInput.Number("Кількість місць: ", 1, 1000));
                Console.WriteLine("Дисципліну оновлено."); break;
            case 4:
                var removed = PickCourse();
                if (ConsoleInput.Confirm("Видалити дисципліну разом із зарахуваннями та оцінками?"))
                { service.RemoveCourse(removed.Id); Console.WriteLine("Дисципліну видалено."); }
                break;
        }
    }
//Зараховує студента на дисципліну або скасовує зарахування
    private void EnrollmentMenu()
    {
        Console.WriteLine("ЗАРАХУВАННЯ: 1 Зарахувати | 2 Скасувати зарахування | 0 Назад");
        var action = ConsoleInput.Number("Дія: ", 0, 2);
        if (action == 0) return;
        var student = PickStudent();
        var course = PickCourse();
        if (action == 1) { service.Enroll(student.Id, course.Id); Console.WriteLine("Студента зараховано."); }
        else if (ConsoleInput.Confirm("Скасувати зарахування разом з оцінкою?"))
        { service.Unenroll(student.Id, course.Id); Console.WriteLine("Зарахування скасовано."); }
    }
//Додає, змінює або видаляє оцінку студента з дисципліни
    private void GradesMenu()
    {
        Console.WriteLine("ОЦІНКИ: 1 Виставити або змінити | 2 Видалити оцінку | 0 Назад");
        var action = ConsoleInput.Number("Дія: ", 0, 2);
        if (action == 0) return;
        var student = PickStudent();
        var enrolled = service.Enrollments.Where(x => x.StudentId == student.Id).Select(x => x.CourseId).ToHashSet();
        var course = ConsoleInput.Pick(service.Courses.Where(x => enrolled.Contains(x.Id)).ToArray(), x => x.Title);
        if (action == 1)
        {
            //Числовий формат контролює UI, діапазон оцінки захищає також модель
            service.SetGrade(student.Id, course.Id, ConsoleInput.Number("Оцінка (0–100): ", int.MinValue, int.MaxValue));
            Console.WriteLine("Оцінку збережено в пам'яті.");
        }
        else if (ConsoleInput.Confirm("Видалити оцінку? Відсутня оцінка означатиме борг."))
        { service.RemoveGrade(student.Id, course.Id); Console.WriteLine("Оцінку видалено."); }
    }
//Виводить картку студента з оцінками, середнім балом і боргами
    private void PrintTranscript(Student student)
    {
        Console.WriteLine($"\nКАРТКА СТУДЕНТА: {student.Describe()}");
        foreach (var enrollment in service.Enrollments.Where(x => x.StudentId == student.Id))
        {
            var course = service.Courses.Single(x => x.Id == enrollment.CourseId);
            var grade = service.Grades.SingleOrDefault(x => x.StudentId == student.Id && x.CourseId == course.Id);
            Console.WriteLine($"{course.Title}: {(grade is null ? "не виставлено — БОРГ" : $"{grade.Points} — {(grade.Points >=
            AcademicService.PassingGrade ? "зараховано" : "БОРГ")}")}");
        }
        var result = service.GetResult(student.Id);
        Console.WriteLine($"Середній бал: {(result.Average?.ToString("F2") ?? "немає оцінок")}; дисциплін: {result.EnrollmentCount}; боргів:
        {result.DebtCount}");
    }
//Виводить рейтинг студентів без академічних боргів
    private void PrintRanking()
    {
        Console.WriteLine("\nРЕЙТИНГ ДЛЯ СТИПЕНДІЇ (лише студенти без боргів)");
        var ranking = service.GetScholarshipRanking();
        foreach (var entry in ranking)
        Console.WriteLine($"{entry.Rank}. {entry.Student.FullName} | {entry.Student.GroupName} | {entry.Average:F2}");
        Console.WriteLine($"Учасників: {ranking.Count}. Однаковий бал — однакове місце.");
        Console.WriteLine("Рейтинг не визначає кількість стипендій: квота в цій роботі не задана.");
    }
//Виводить студентів із боргами та дисципліни, за якими вони виникли
    private void PrintDebtors()
    {
        Console.WriteLine("\nАКАДЕМІЧНІ БОРЖНИКИ (оцінка < 60 або відсутня)");
        var debtors = service.GetDebtors();
        foreach (var result in debtors)
        {
            Console.WriteLine($"{result.Student.FullName} | {result.Student.GroupName} | боргів: {result.DebtCount}");
            foreach (var e in service.Enrollments.Where(x => x.StudentId == result.Student.Id))
            {
                var grade = service.Grades.SingleOrDefault(x => x.StudentId == e.StudentId && x.CourseId == e.CourseId);
                if (grade is null || grade.Points < AcademicService.PassingGrade)
                    Console.WriteLine($"  {service.Courses.Single(x => x.Id == e.CourseId).Title}: {(grade is null ? "оцінки немає" :
                    grade.Points.ToString())}");
            }
        }
        Console.WriteLine($"Усього боржників: {debtors.Count}");
    }
}