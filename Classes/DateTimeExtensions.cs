//------------------------------------------------------------------------------
// TrackerSQL v3.x — DateTimeExtensions
// Shared infrastructure / utility: DateTimeExtensions.
//------------------------------------------------------------------------------

using System;

//- only form later versions #nullable disable
namespace TrackerSQL.Classes
{
    public static class DateTimeExtensions
    {
        public static DateTime GetFirstDayOfWeek(this DateTime sourceDateTime)
        {
            int diff = -(int)sourceDateTime.DayOfWeek;
            return sourceDateTime.AddDays(diff).Date;
        }


        public static DateTime GetLastDayOfWeek(this DateTime sourceDateTime)
        {
            DayOfWeek dayOfWeek = 6 - sourceDateTime.DayOfWeek;
            sourceDateTime = sourceDateTime.AddDays((double)dayOfWeek);
            return sourceDateTime;
        }
    }
}
