using System;
using DbInterfaces.Interfaces;

namespace QueryLanguage.Grouping.ByTrigger
{
    internal class GroupByTriggerConfigurator<T> : IGroupByTriggerConfigurator<T>, IGroupTimesByTriggerConfigurator<T>,
        IGroupByTriggerAggregation<T>,
        IGroupByTriggerOptional where T : struct
    {
        public Func<ISingleDataRow<T>, bool> PredicateFunc { get; set; }
        public Func<IQuerySerie<T>, T?> AggregationFunc { get; set; }
        public TimeSpan? StartOffsetTimeSpan { get; set; }
        public TimeSpan? EndOffsetTimeSpan { get; set; }
        public TimeStampType TimeStampType { get; set; }
        public bool EndTimeIsStartTime { get; set; }
        public bool StartTimeIsEndTime { get; set; }

        public IGroupByTriggerOptional Aggregate(Func<IQuerySerie<T>, T?> aggregationFunc)
        {
            AggregationFunc = aggregationFunc;
            return this;
        }

        public IGroupByTriggerAggregation<T> TriggerWhen(Func<ISingleDataRow<T>, bool> predicate)
        {
            PredicateFunc = predicate;
            return this;
        }

        public IGroupByTriggerOptional StartOffset(TimeSpan offset)
        {
            StartOffsetTimeSpan = offset;
            return this;
        }

        public IGroupByTriggerOptional EndOffset(TimeSpan offset)
        {
            EndOffsetTimeSpan = offset;
            return this;
        }

        public IGroupByTriggerOptional TimeStampIsStart()
        {
            TimeStampType = TimeStampType.Start;
            return this;
        }

        public IGroupByTriggerOptional TimeStampIsMiddle()
        {
            TimeStampType = TimeStampType.Start;
            return this;
        }

        public IGroupByTriggerOptional TimeStampIsEnd()
        {
            TimeStampType = TimeStampType.End;
            return this;
        }

        public IGroupByTriggerOptional EndIsStart()
        {
            EndTimeIsStartTime = true;
            return this;
        }

        public IGroupByTriggerOptional StartIsEnd()
        {
            StartTimeIsEndTime = true;
            return this;
        }

        IGroupByTriggerOptional IGroupTimesByTriggerConfigurator<T>.TriggerWhen(Func<ISingleDataRow<T>, bool> predicate)
        {
            PredicateFunc = predicate;
            return this;
        }
    }
}