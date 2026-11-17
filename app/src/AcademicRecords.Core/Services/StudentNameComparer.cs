//Порівнює студентів за українським алфавітом і номером залікової книжки
using System.Globalization;
using AcademicRecords.Core.Models;

namespace AcademicRecords.Core.Services;

public sealed class StudentNameComparer : IComparer<Student>
{
    private static readonly StringComparer Names = StringComparer.Create(CultureInfo.GetCultureInfo("uk-UA"), true);
//Порівнює студентів за ПІБ, а за однакових імен — за номером залікової
    public int Compare(Student? x, Student? y)
    {
        if (ReferenceEquals(x, y)) return 0;
        if (x is null) return -1;
        if (y is null) return 1;
        var result = Names.Compare(x.FullName, y.FullName);
        return result != 0 ? result : StringComparer.OrdinalIgnoreCase.Compare(x.RecordBookNumber, y.RecordBookNumber);
    }
}