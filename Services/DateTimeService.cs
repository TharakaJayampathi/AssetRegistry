using AssetRegistry.Interfaces;
using System.Diagnostics;

namespace AssetRegistry.Services
{
    public class DateTimeService : IDateTimeService
    {
        /// <summary>
        /// Get Time Zone by Id
        /// </summary>
        /// <param name="TimeZoneId"></param>
        /// <returns></returns>
        public TimeZoneInfo GetTimeZone(string TimeZoneId)
        {
            return TimeZoneInfo.FindSystemTimeZoneById(TimeZoneId);
        }


        /// <summary>
        /// Convert UTC date to Local Date
        /// </summary>
        /// <param name="Date"></param>
        /// <returns></returns>
        public DateTime? ConvertToSriLankanTime(DateTime? Date)
        {
            string TimeZoneId = "Sri Lanka Standard Time";

            if (Date.HasValue)
            {
                return TimeZoneInfo.ConvertTimeFromUtc(Date.Value, GetTimeZone(TimeZoneId));
            }

            return null;
        }

        /// <summary>
        /// Get Current Local Time by TimeZone
        /// </summary>
        /// <param name="TimeZoneId"></param>
        /// <returns></returns>
        public DateTime GetCurrentTime(string TimeZoneId = "Sri Lanka Standard Time")
        {
            //if (string.IsNullOrEmpty(TimeZoneId))
            //{
            //    TimeZoneId = "Sri Lanka Standard Time";
            //    //TimeZoneId = TimeZone.CurrentTimeZone.StandardName;
            //}

            return TimeZoneInfo.ConvertTime(DateTime.Now, GetTimeZone(TimeZoneId));
        }

        public DateTime ConvertFromLocalToUTC(DateTime Date, string LocalTimeZoneId = "Sri Lanka Standard Time")
        {
            //var _utc = DateTime.UtcNow;
            //var _serverTime = DateTime.Now;

            //var _dateDiff = _utc - _serverTime;
            //DateTime date = Date.Add(_dateDiff);

            // Define the time zones
            TimeZoneInfo sourceTimeZone = TimeZoneInfo.FindSystemTimeZoneById(LocalTimeZoneId);
            TimeZoneInfo targetTimeZone = TimeZoneInfo.FindSystemTimeZoneById("UTC");

            // Create a DateTime representing the original time
            //DateTime originalTime = DateTime.Now; // Replace with your original DateTime

            // Convert the DateTime from the source timezone to the target timezone
            DateTime targetTime = TimeZoneInfo.ConvertTime(Date, sourceTimeZone, targetTimeZone);

            //var _dateZone = TimeZoneInfo.ConvertTimeBySystemTimeZoneId(Date, "UTC");

            return targetTime;
        }

        /// <summary>
        /// Formatted Date
        /// Ex: 01-JAN-2020
        /// </summary>
        /// <param name="Date"></param>
        /// <returns></returns>
        public string FormatDate(DateTime Date)
        {
            var cult = System.Globalization.CultureInfo.CurrentCulture;
            return Date.Day + "-" + cult.DateTimeFormat.GetAbbreviatedMonthName(Date.Month) + "-" + Date.Year;
        }

        /// <summary>
        /// get Formatted Time
        /// </summary>
        /// <param name="Date"></param>
        /// <returns></returns>
        public string FormatTime(DateTime Date)
        {
            var cult = System.Globalization.CultureInfo.CurrentCulture;

            var str = Date.ToString("tt", System.Globalization.CultureInfo.InvariantCulture);
            return Date.Hour + ":" + Date.Minute + ":" + Date.Second + " " + str;
        }

        /// <summary>
        /// Format Time with Hour and Minutes
        /// </summary>
        /// <param name="Time"></param>
        /// <returns></returns>
        public string FormatTime(TimeSpan Time)
        {
            var cult = System.Globalization.CultureInfo.CurrentCulture;

            //var str = Time.ToString("tt", System.Globalization.CultureInfo.InvariantCulture);
            //string _min = Time.Minutes.ToString();

            //if(_min.Length == 1)
            //{
            //    _min = $"0{_min}";
            //}

            return $"{Time.Hours}:{Time.Minutes}";
        }

        /// <summary>
        /// Converts Current Time of the Specified Time Zone to Unix Time
        /// </summary>
        /// <param name="Date"></param>
        /// <returns></returns>
        public long GetUnixTime(string TimeZoneId = "Sri Lanka Standard Time")
        {
            return new DateTimeOffset(TimeZoneInfo.ConvertTime(DateTime.Now, GetTimeZone(TimeZoneId))).ToUnixTimeSeconds();
        }

        public long GetUnixTime(DateTime Date)
        {
            return new DateTimeOffset(Date).ToUnixTimeSeconds();
        }

        /// <summary>
        /// Get Friendly Date Time format from Unix Time
        /// </summary>
        /// <param name="TimeStamp"></param>
        /// <returns></returns>
        /// <exception cref="NotImplementedException"></exception>
        public DateTime GetFromUnixTime(long TimeStamp)
        {
            return DateTimeOffset.FromUnixTimeSeconds(TimeStamp).DateTime;
        }

        /// <summary>
        /// Calculate Work Hours
        /// </summary>
        /// <param name="From"></param>
        /// <param name="To"></param>
        /// <returns></returns>
        public double CalculatWorkHours(DateTime From, DateTime To)
        {
            TimeSpan _workDiff = To - From;

            return _workDiff.TotalMinutes;
        }

        /// <summary>
        /// Convert Work Hours to Read
        /// </summary>
        /// <param name="totalMinutes"></param>
        /// <returns></returns>
        public string ConvertTimeToReadFormat(double totalMinutes)
        {
            if (totalMinutes >= 60)
            {
                // Extract whole number part (hours)
                //int wholeHours = (int)totalMinutes;

                //// Calculate minutes and seconds
                //double minutesDouble = (totalMinutes - wholeHours) * 60;
                //int minutes = (int)minutesDouble;
                //double secondsDouble = (minutesDouble - minutes) * 60;
                //int seconds = (int)secondsDouble;

                //// Format as hh:mm:ss
                //return $"{wholeHours:D2}:{minutes:D2}:{seconds:D2}";

                TimeSpan timeSpan = TimeSpan.FromMinutes(totalMinutes);

                return $"{(int)timeSpan.TotalHours:D2}:{timeSpan.Minutes:D2}:{timeSpan.Seconds:D2}";
            }
            else
            {
                // Extract whole number part (hours)
                int wholeHours = 0;

                // Calculate minutes and seconds
                double minutesDouble = totalMinutes;
                int minutes = (int)minutesDouble;
                double secondsDouble = (minutesDouble - minutes) * 60;
                int seconds = (int)secondsDouble;

                // Format as hh:mm:ss
                return $"{wholeHours:D2}:{minutes:D2}:{seconds:D2}";
            }



            //Console.WriteLine($"{hours} hours is equal to {formattedTime}");
        }

        public DateTime GetLocalTime(DateTime dateTimeToConvert, string TimeZoneId = "Sri Lanka Standard Time")
        {
            if (string.IsNullOrEmpty(TimeZoneId))
            {
                TimeZoneId = TimeZoneInfo.Local.Id;
            }
            return TimeZoneInfo.ConvertTime(dateTimeToConvert, GetTimeZone(TimeZoneId));
        }
    }
}
