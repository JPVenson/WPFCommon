using System;
using System.Globalization;

namespace JPB.Extentions.Extensions
{
    public static class DateHelperExtention
    {
        public static int GetCalendarWeek(this DateTime date)
        {
            var currentCulture = CultureInfo.CurrentCulture;

            var calendar = currentCulture.Calendar;

            var calendarWeek = calendar.GetWeekOfYear(date,
                currentCulture.DateTimeFormat.CalendarWeekRule,
                currentCulture.DateTimeFormat.FirstDayOfWeek);

            if (calendarWeek > 52)
            {
                date = date.AddDays(7);
                var testCalendarWeek = calendar.GetWeekOfYear(date,
                    currentCulture.DateTimeFormat.CalendarWeekRule,
                    currentCulture.DateTimeFormat.FirstDayOfWeek);
                if (testCalendarWeek == 2)
                    calendarWeek = 1;
            }

            //var year = date.Year;
            //if (calendarWeek == 1 && date.Month == 12)
            //{
            //    year++;
            //}

            //if (calendarWeek >= 52 && date.Month == 1)
            //{
            //    year--;
            //}

            return calendarWeek;
        }
    }
}