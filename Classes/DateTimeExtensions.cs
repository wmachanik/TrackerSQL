// Decompiled with JetBrains decompiler
// Type: TrackerDotNet.classes.DateTimeExtensions
// Assembly: TrackerDotNet, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 2B5ACBFB-45EE-46B9-81D2-DBD1194F39CE
// Assembly location: C:\SRC\Apps\qtracker\bin\TrackerDotNet.dll

using System;

//- only form later versions #nullable disable
namespace TrackerDotNet.Classes
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