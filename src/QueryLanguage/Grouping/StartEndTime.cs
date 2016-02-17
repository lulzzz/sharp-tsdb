using System;

namespace QueryLanguage.Grouping
{
    public class StartEndTime
    {
        public DateTime Start { get; set; }
        public DateTime End { get; set; }

        public StartEndTime(DateTime start, DateTime end)
        {
            Start = start;
            End = end;
        }

        public DateTime GetTimeStampByType(TimeStampType timeStampType = TimeStampType.Start)
        {
            if (timeStampType == TimeStampType.Start) return Start;
            if (timeStampType == TimeStampType.End)
            {
                return End;
            }
            if (timeStampType == TimeStampType.Middle)
            {
                return Start + TimeSpan.FromMinutes((End - Start).TotalMinutes / 2);
            }
            return Start;
        }
    }
}