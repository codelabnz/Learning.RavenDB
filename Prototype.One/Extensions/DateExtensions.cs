using System;
using NodaTime;

namespace Prototype.One.Extensions
{
    public static class DateExtensions
    {
        public static LocalDate MonthBegin(this LocalDate date)
        {
            return new LocalDate(date.Year, date.Month, 1);
        }

        public static bool FallsInMonth(this LocalDate toTest, LocalDate month)
        {
            return toTest.Month == month.Month && toTest.Year == month.Year;
        }

        public static DateTime MonthBegin(this DateTime date)
        {
            return new DateTime(date.Year, date.Month, 1);
        }
    }
}
