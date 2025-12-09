using System.Diagnostics;

namespace AssetRegistry.Interfaces
{
    public interface IDateTimeService
    {
        string FormatDate(DateTime Date);
        string FormatTime(DateTime Date);
        string FormatTime(TimeSpan Time);
        DateTime GetCurrentTime(string TimeZoneId = "Sri Lanka Standard Time");
        DateTime? ConvertToSriLankanTime(DateTime? Date);
        DateTime ConvertFromLocalToUTC(DateTime Date, string LocalTimeZoneId = "Sri Lanka Standard Time");
        TimeZoneInfo GetTimeZone(string TimeZoneId);
        long GetUnixTime(DateTime Date);
        long GetUnixTime(string TimeZoneId = "Sri Lanka Standard Time");
        DateTime GetFromUnixTime(long TimeStamp);
        double CalculatWorkHours(DateTime From, DateTime To);
        string ConvertTimeToReadFormat(double hours);
    }
}
