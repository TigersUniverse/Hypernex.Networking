using System;
using System.Globalization;

namespace Hypernex.Sandboxing.SandboxedTypes
{
    public class UtcTime
    {
        internal DateTime d = DateTime.UtcNow;
        
        public void Load(int year, int month, int day) => d = new DateTime(year, month, day);
        public void Load(int year, int month, int day, int hour, int minute, int second) =>
            d = new DateTime(year, month, day, hour, minute, second);
        public void Load(int year, int month, int day, int hour, int minute, int second, int millisecond) =>
            d = new DateTime(year, month, day, hour, minute, second, millisecond);

        public void Load(Time u) => d = TimeZoneInfo.ConvertTimeToUtc(u.d, TimeZoneInfo.Local);

        public string GetDayOfWeek() => d.DayOfWeek.ToString();
        public string GetMonthName() => d.ToString("MM");
        public string GetAMPM() => d.ToString("tt", CultureInfo.InvariantCulture).ToUpper();
        
        public int GetMilliseconds() => d.Millisecond;
        public int GetSeconds() => d.Second;
        public int GetMinutes() => d.Minute;
        public int GetHours() => d.Hour;
        public int GetDay() => d.Day;
        public int GetMonth() => d.Month;
        public int GetYear() => d.Year;
    }
}