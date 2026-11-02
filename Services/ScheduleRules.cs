using FinalProjApp.Models;
using System.Globalization;
using System.Text.RegularExpressions;

namespace FinalProjApp.Services
{
    /// <summary>
    /// Expands the simple recurrence rules the app writes
    /// ("FREQ=DAILY|WEEKLY|MONTHLY;INTERVAL=1;UNTIL=yyyyMMddT000000Z;") so pages
    /// can ask what happens on a given day.
    /// </summary>
    public static class ScheduleRules
    {
        public static bool OccursOn(ScheduleEvent e, DateTime day)
        {
            day = day.Date;
            var first = e.StartTime.Date;
            if (day < first) return false;
            if (string.IsNullOrEmpty(e.RecurrenceRule)) return day == first;

            var until = Until(e.RecurrenceRule);
            if (until is { } last && day > last) return false;

            return Frequency(e.RecurrenceRule) switch
            {
                "DAILY" => true,
                "WEEKLY" => day.DayOfWeek == first.DayOfWeek,
                "MONTHLY" => day.Day == first.Day,
                _ => day == first
            };
        }

        /// <summary>The event moved onto <paramref name="day"/> (same time of day and length).</summary>
        public static (DateTime Start, DateTime End) On(ScheduleEvent e, DateTime day) =>
            (day.Date + e.StartTime.TimeOfDay, day.Date + e.StartTime.TimeOfDay + (e.EndTime - e.StartTime));

        /// <summary>Upcoming occurrences from <paramref name="from"/> over the next <paramref name="days"/> days, soonest first.</summary>
        public static List<(ScheduleEvent Event, DateTime Start, DateTime End)> Upcoming(
            IEnumerable<ScheduleEvent> events, DateTime from, int days, int max)
        {
            var result = new List<(ScheduleEvent, DateTime, DateTime)>();
            for (var d = 0; d < days && result.Count < max; d++)
            {
                var day = from.Date.AddDays(d);
                foreach (var e in events.Where(e => OccursOn(e, day)).OrderBy(e => e.StartTime.TimeOfDay))
                {
                    var (start, end) = On(e, day);
                    if (end < from) continue; // already over today
                    result.Add((e, start, end));
                    if (result.Count == max) break;
                }
            }
            return result;
        }

        private static string Frequency(string rule) =>
            Regex.Match(rule, @"FREQ=(\w+)") is { Success: true } m ? m.Groups[1].Value : "";

        private static DateTime? Until(string rule) =>
            Regex.Match(rule, @"UNTIL=(\d{8})") is { Success: true } m
            && DateTime.TryParseExact(m.Groups[1].Value, "yyyyMMdd", CultureInfo.InvariantCulture,
                DateTimeStyles.None, out var until)
                ? until.Date
                : null;
    }
}
