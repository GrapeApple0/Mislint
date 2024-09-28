using Misharp.Model;
using System;
using System.Collections.Generic;
using System.Net.Http;
using static Misharp.Controls.MetaApi;

namespace Mislint.Core
{
    public static class Shared
    {
        public static HttpClient HttpClient { get; } = new HttpClient();
        public static Misharp.App MisharpApp { get; set; }
        public static MeDetailed I { get; set; }
        public static MetaResponse Meta { get; set; }
        public static List<EmojiSimple> Emojis { get; set; }
        public static void TimeSpanToDateParts(DateTime d1, DateTime d2, out int years, out int months, out int days, out int hours, out int minutes, out int seconds)
        {
            if (d1 < d2)
            {
                (d1, d2) = (d2, d1);
            }

            var span = d1 - d2;

            months = 12 * (d1.Year - d2.Year) + (d1.Month - d2.Month);

            //month may need to be decremented because the above calculates the ceiling of the months, not the floor.
            //to do so we increase d2 by the same number of months and compare.
            //(500ms fudge factor because datetimes are not precise enough to compare exactly)
            if (d1.CompareTo(d2.AddMonths(months).AddMilliseconds(-500)) <= 0)
            {
                --months;
            }

            years = months / 12;
            months -= years * 12;

            if (months == 0 && years == 0)
            {
                days = span.Days;
            }
            else
            {
                var md1 = new DateTime(d1.Year, d1.Month, d1.Day);
                // Fixed to use d2.Day instead of d1.Day
                var md2 = new DateTime(d2.Year, d2.Month, d2.Day);
                var mDays = (int)(md1 - md2).TotalDays;

                if (mDays > span.Days)
                {
                    mDays = (int)(md1.AddMonths(-1) - md2).TotalDays;
                }

                days = span.Days - mDays;
            }
            hours = span.Hours;
            minutes = span.Minutes;
            seconds = span.Seconds;
        }

        public static string GetTimeSpan(DateTime time)
        {
            TimeSpanToDateParts(DateTime.UtcNow, time, out var years, out var months, out var days, out var hours, out var minutes, out var seconds);
            if (years > 0)
                return $"{years}年前";
            if (months > 0)
                return $"{months}ヶ月前";
            if (days > 0)
                return $"{days}日前";
            if (hours > 0)
                return $"{hours}時間前";
            if (minutes > 0)
                return $"{minutes}分前";
            if (seconds > 0)
                return $"{seconds}秒前";
            return "たった今";
        }
    }
}
