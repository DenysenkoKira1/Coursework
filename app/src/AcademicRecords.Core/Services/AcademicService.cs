//Основна логіка керування академічними даними та перевірки зв'язків між ними
using AcademicRecords.Core.Exceptions;
using AcademicRecords.Core.Interfaces;
using AcademicRecords.Core.Models;

namespace AcademicRecords.Core.Services;

//Єдина точка зміни даних та контролю цілісності зв'язків
public sealed class AcademicService
{
    public static int PassingGrade { get; } = 60;
    private readonly IRepository<Student> _students = new InMemoryRepository<Student>();
    private readonly IRepository<Teacher> _teachers = new InMemoryRepository<Teacher>();
    private readonly IRepository<Course> _courses = new InMemoryRepository<Course>();
    private readonly IRepository<Enrollment> _enrollments = new InMemoryRepository<Enrollment>();
    private readonly IRepository<GradeRecord> _grades = new InMemoryRepository<GradeRecord>();

    public IReadOnlyList<Student> Students => _students.GetAll().Order(new StudentNameComparer()).ToArray();
    public IReadOnlyList<Teacher> Teachers => _teachers.GetAll().OrderBy(x => x.FullName).ToArray();
    public IReadOnlyList<Course> Courses => _courses.GetAll().OrderBy(x => x.Title).ToArray();
    public IReadOnlyList<Enrollment> Enrollments => _enrollments.GetAll();
    public IReadOnlyList<GradeRecord> Grades => _grades.GetAll();

//Створює студента, перевіряє унікальність номера та додає його
    public Student AddStudent(string name, string number, string group)
    {
        var student = new Student(Guid.NewGuid(), name, number, group);
        CheckStudentNumber(student);
        _students.Add(student);
        return student;
    }
//Оновлює дані студента після перевірки номера залікової
    public void UpdateStudent(Guid id, string name, string number, string group)
    {
        var student = new Student(id, name, number, group);
        CheckStudentNumber(student);
        _students.Update(student);
    }
//Перевіряє, що номер залікової унікальний серед студентів
    private void CheckStudentNumber(Student student)
    {
        if (Students.Any(x => x.Id != student.Id &&
            string.Equals(x.RecordBookNumber, student.RecordBookNumber, StringComparison.OrdinalIgnoreCase)))
            throw new InvalidOperationException("Номер залікової книжки вже використано.");
    }
//Створює та додає викладача
    public Teacher AddTeacher(string name, string department)
    {
        var teacher = new Teacher(Guid.NewGuid(), name, department);
        _teachers.Add(teacher);
        return teacher;
    }
//Оновлює дані викладача
    public void UpdateTeacher(Guid id, string name, string department)
        => _teachers.Update(new Teacher(id, name, department));

//Перевіряє викладача, створює та додає дисципліну
    public Course AddCourse(string title, Guid teacherId, int capacity)
    {
        _teachers.Get(teacherId);
        var course = new Course(Guid.NewGuid(), title, teacherId, capacity);
        _courses.Add(course);
        return course;
    }
//Оновлює дисципліну, якщо її нова місткість достатня
    public void UpdateCourse(Guid id, string title, Guid teacherId, int capacity)
    {
        _teachers.Get(teacherId);
        var course = new Course(id, title, teacherId, capacity);
        if (EnrolledCount(id) > capacity)
            throw new InvalidOperationException("Місткість менша за кількість уже зарахованих студентів.");
        _courses.Update(course);
    }
//Повертає кількість студентів, зарахованих на дисципліну
    public int EnrolledCount(Guid courseId) => Enrollments.Count(x => x.CourseId == courseId);

//Зараховує студента, перевіряючи дубль і наявність вільних місць
    public void Enroll(Guid studentId, Guid courseId)
    {
        _students.Get(studentId);
        var course = _courses.Get(courseId);
        if (Enrollments.Any(x => x.StudentId == studentId && x.CourseId == courseId))
            throw new InvalidOperationException("Студента вже зараховано на цю дисципліну.");
        if (EnrolledCount(courseId) >= course.Capacity)
            throw new CourseCapacityExceededException(course.Title, course.Capacity);
        _enrollments.Add(new Enrollment(Guid.NewGuid(), studentId, courseId));
    }
//Скасовує зарахування та видаляє оцінку за цю дисципліну
    public void Unenroll(Guid studentId, Guid courseId)
    {
        var enrollment = Enrollments.FirstOrDefault(x => x.StudentId == studentId && x.CourseId == courseId)
            ?? throw new InvalidOperationException("Студент не зарахований на цю дисципліну.");
        foreach (var grade in Grades.Where(x => x.StudentId == studentId && x.CourseId == courseId)) _grades.Remove(grade.Id);
        _enrollments.Remove(enrollment.Id);
    }
//Додає або змінює оцінку студента, якщо він зарахований
    public void SetGrade(Guid studentId, Guid courseId, int points)
    {
        if (!Enrollments.Any(x => x.StudentId == studentId && x.CourseId == courseId))
            throw new InvalidOperationException("Спочатку зарахуйте студента на дисципліну.");
        var old = Grades.FirstOrDefault(x => x.StudentId == studentId && x.CourseId == courseId);
        var grade = new GradeRecord(old?.Id ?? Guid.NewGuid(), studentId, courseId, points);
        if (old is null) _grades.Add(grade); else _grades.Update(grade);
    }
//Видаляє наявну оцінку студента з дисципліни
    public void RemoveGrade(Guid studentId, Guid courseId)
    {
        var grade = Grades.FirstOrDefault(x => x.StudentId == studentId && x.CourseId == courseId)
            ?? throw new InvalidOperationException("Оцінка ще не виставлена.");
        _grades.Remove(grade.Id);
    }
//Видаляє студента разом із зарахуваннями та оцінками
    public void RemoveStudent(Guid id)
    {
        _students.Get(id);
        foreach (var enrollment in Enrollments.Where(x => x.StudentId == id)) Unenroll(id, enrollment.CourseId);
        _students.Remove(id);
    }
//Видаляє дисципліну разом із зарахуваннями та оцінками
    public void RemoveCourse(Guid id)
    {
        _courses.Get(id);
        foreach (var enrollment in Enrollments.Where(x => x.CourseId == id)) Unenroll(enrollment.StudentId, id);
        _courses.Remove(id);
    }
//Видаляє викладача, якщо його дисципліни більше не посилаються на нього
    public void RemoveTeacher(Guid id)
    {
        if (Courses.Any(x => x.TeacherId == id))
            throw new InvalidOperationException("Спочатку змініть викладача пов'язаних дисциплін або видаліть ці дисципліни.");
        _teachers.Remove(id);
    }
//Шукає студентів за ПІБ, групою або номером залікової
    public IReadOnlyList<Student> FindStudents(string query)
        => Students.Where(x => x.FullName.Contains(query, StringComparison.OrdinalIgnoreCase)
            || x.GroupName.Contains(query, StringComparison.OrdinalIgnoreCase)
            || x.RecordBookNumber.Contains(query, StringComparison.OrdinalIgnoreCase)).ToArray();

//Обчислює середній бал і кількість боргів одного студента
    public StudentResult GetResult(Guid studentId)
    {
        var student = _students.Get(studentId);
        var enrollments = Enrollments.Where(x => x.StudentId == studentId).ToArray();
        var grades = Grades.Where(x => x.StudentId == studentId).ToArray();
        //Відсутня оцінка є боргом, але не підміняється числовим нулем у середньому
        var debt = enrollments.Count(e => !grades.Any(g => g.CourseId == e.CourseId && g.Points >= PassingGrade));
        decimal? average = grades.Length == 0 ? null : Math.Round(grades.Average(x => (decimal)x.Points), 2, MidpointRounding.AwayFromZero);
        return new StudentResult(student, average, enrollments.Length, debt);
    }
//Повертає студентів, які мають хоча б один академічний борг
    public IReadOnlyList<StudentResult> GetDebtors()
        => Students.Select(x => GetResult(x.Id)).Where(x => x.DebtCount > 0).ToArray();

//Формує рейтинг студентів без боргів із правильними спільними місцями
    public IReadOnlyList<RankingEntry> GetScholarshipRanking()
    {
        var candidates = Students.Select(x => GetResult(x.Id))
            .Where(x => x.EnrollmentCount > 0 && x.DebtCount == 0 && x.Average.HasValue)
            .OrderByDescending(x => x.Average).ThenBy(x => x.Student, new StudentNameComparer()).ToArray();
        var entries = new List<RankingEntry>();
        var rank = 0;
        for (var i = 0; i < candidates.Length; i++)
        {
            if (i == 0 || candidates[i].Average != candidates[i - 1].Average) rank = i + 1;
            entries.Add(new RankingEntry(rank, candidates[i].Student, candidates[i].Average!.Value));
        }
        return entries;
    }
//Створює об'єкт із копіями всіх даних для збереження
    public AcademicData Export() => new()
    {
        Students = [.. Students], Teachers = [.. Teachers], Courses = [.. Courses],
        Enrollments = [.. Enrollments], Grades = [.. Grades]
    };
//Перевіряє зв'язки та створює сервіс із завантажених даних
    public static AcademicService FromData(AcademicData data)
    {
        ArgumentNullException.ThrowIfNull(data);
        if (data.SchemaVersion != 1) throw new InvalidOperationException("Непідтримувана версія файлу даних.");
        if (data.Students is null || data.Teachers is null || data.Courses is null || data.Enrollments is null || data.Grades is null)
        throw new InvalidOperationException("Файл містить null замість колекції.");
        var service = new AcademicService();
        foreach (var s in data.Students) { ArgumentNullException.ThrowIfNull(s); service.CheckStudentNumber(s); service._students.Add(s); }
        foreach (var t in data.Teachers) service._teachers.Add(t);
        foreach (var c in data.Courses)
        {
            ArgumentNullException.ThrowIfNull(c);
            service._teachers.Get(c.TeacherId);
            service._courses.Add(c);
        }
        foreach (var e in data.Enrollments)
        {
            ArgumentNullException.ThrowIfNull(e);
            //Перевірка пари, місткості та зовнішніх ключів без зміни збереженого Id
            service._students.Get(e.StudentId);
            var course = service._courses.Get(e.CourseId);
            if (service.Enrollments.Any(x => x.StudentId == e.StudentId && x.CourseId == e.CourseId))
                throw new InvalidOperationException("Дубль зарахування у файлі.");
            if (service.EnrolledCount(e.CourseId) >= course.Capacity)
                throw new CourseCapacityExceededException(course.Title, course.Capacity);
            service._enrollments.Add(e);
        }
        foreach (var g in data.Grades)
        {
            ArgumentNullException.ThrowIfNull(g);
            if (!service.Enrollments.Any(x => x.StudentId == g.StudentId && x.CourseId == g.CourseId))
                throw new InvalidOperationException("У файлі є оцінка без зарахування.");
            if (service.Grades.Any(x => x.StudentId == g.StudentId && x.CourseId == g.CourseId))
                throw new InvalidOperationException("Дубль оцінки у файлі.");
            service._grades.Add(g);
        }
        return service;
    }
}