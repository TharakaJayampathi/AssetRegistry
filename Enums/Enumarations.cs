namespace AssetRegistry.Enums
{
    internal class Enumarations
    {
    }

    public enum ClaimCategories
    {
        MANAGE_USERS = 1,
        MANAGE_COMPANIES = 2,
        MANAGE_DIVISIONS = 3,
        MANAGE_NOTIFICATIONS = 4,
        WAREHOUSE = 5,
        FINANCE = 6,
    }



    public enum NumberFormatType
    {
        /// <summary>
        /// Format without Thousand Seperator
        /// Ex: 12345
        /// </summary>
        NO_THOUSAND_SPERATOR = 1,
        /// <summary>
        /// Format to 2 Decimal Places
        /// Ex: 1,234.56
        /// </summary>
        TWO_DECIMAL_POINTS = 2,
        /// <summary>
        /// Format to 3 Decimal Places
        /// Ex: 1,234.567
        /// </summary>
        THREE_DECIMAL_POINTS = 3,
        /// <summary>
        /// Format Currentcy with Thousend Seperator (K) Value
        /// Ex: 1,234,56 => 123.45 K
        /// </summary>
        K_SEPERATOR = 4,
        /// <summary>
        /// Format Currentcy with Million Seperator (M) Value
        /// Ex: 1,234,567 => 1.23 M
        /// </summary>
        M_SEPERATOR = 5
    }

    public enum DateFormatType
    {
        /// <summary>
        /// 01-Jan
        /// </summary>
        DD_MM = 1,
        /// <summary>
        /// 01-Jan-2018
        /// </summary>
        DD_MM_YYYY = 2,
        /// <summary>
        /// 01/01/2018
        /// </summary>
        DD_MM_YYYY_NUMBER = 3,

    }

    public enum TimeFormatType
    {
        /// <summary>
        /// 24 Hour Time Format
        /// </summary>
        HRS = 1,
        /// <summary>
        /// Local (Am/Pm) Time Format
        /// </summary>
        AM_PM = 2
    }

    public enum AttendanceReportEnum
    {
        PROJECT = 1,
        USER = 2
    }

    public enum ApiLogEnum
    {
        LOG = 1,
        ERROR = 2
    }
}
