// /*******************************************************************************
//  * Copyright (c) 2016 by RF77 (https://github.com/RF77)
//  * All rights reserved. This program and the accompanying materials
//  * are made available under the terms of the Eclipse Public License v1.0
//  * which accompanies this distribution, and is available at
//  * http://www.eclipse.org/legal/epl-v10.html
//  *
//  * Contributors:
//  *    RF77 - initial API and implementation and/or initial documentation
//  *******************************************************************************/ 

using System;
using System.Collections.Generic;
using System.Linq;
using Timeenator.Extensions;
using Timeenator.Interfaces;

namespace Timeenator.Impl.Grouping.Configurators
{
    public class GroupByStartEndTimesConfigurator<T> : GroupConfigurator<T>, IGroupByStartEndTimesConfigurator<T>,
        IGroupByStartEndTimesConfiguratorOptional<T>, IGroupTimesCreator where T : struct
    {
        public GroupByStartEndTimesConfigurator(IQuerySerie<T> serie) : base(serie)
        {
        }

        public TimeSpan? StartOffsetTimeSpan { get; set; }
        public TimeSpan? EndOffsetTimeSpan { get; set; }
        public TimeStampType TimeStampType { get; set; }
        public bool EndTimeIsStartTime { get; set; }
        public bool StartTimeIsEndTime { get; set; }
        public TimeSpan? Tolerance { get; set; }
        protected List<StartEndTime> GroupTimes { get; set; }
        public double? ExpandFactor { get; set; }
        public TimeSpan? ExpandTime { get; set; }

        public IGroupByStartEndTimesConfiguratorOptional<T> ByTimeRanges(IReadOnlyList<StartEndTime> groupTimes)
        {
            GroupTimes = (groupTimes as List<StartEndTime>) ?? groupTimes.ToList();
            return this;
        }

        public IGroupByStartEndTimesConfiguratorOptional<T> StartOffset(TimeSpan offset)
        {
            StartOffsetTimeSpan = offset;
            return this;
        }

        public IGroupByStartEndTimesConfiguratorOptional<T> EndOffset(TimeSpan offset)
        {
            EndOffsetTimeSpan = offset;
            return this;
        }

        public IGroupByStartEndTimesConfiguratorOptional<T> StartOffset(string offset)
        {
            return StartOffset(offset.ToTimeSpan());
        }

        public IGroupByStartEndTimesConfiguratorOptional<T> EndOffset(string offset)
        {
            return EndOffset(offset.ToTimeSpan());
        }

        public IGroupByStartEndTimesConfiguratorOptional<T> TimeStampIsStart()
        {
            TimeStampType = TimeStampType.Start;
            return this;
        }

        public IGroupByStartEndTimesConfiguratorOptional<T> TimeStampIsMiddle()
        {
            TimeStampType = TimeStampType.Middle;
            return this;
        }

        public IGroupByStartEndTimesConfiguratorOptional<T> TimeStampIsEnd()
        {
            TimeStampType = TimeStampType.End;
            return this;
        }

        public IGroupByStartEndTimesConfiguratorOptional<T> EndIsStart()
        {
            EndTimeIsStartTime = true;
            return this;
        }

        public IGroupByStartEndTimesConfiguratorOptional<T> StartIsEnd()
        {
            StartTimeIsEndTime = true;
            return this;
        }

        public IGroupByStartEndTimesConfiguratorOptional<T> ExpandTimeRangeByFactor(double factor)
        {
            ExpandFactor = factor;
            return this;
        }

        public IGroupByStartEndTimesConfiguratorOptional<T> ExpandTimeRange(TimeSpan timeSpan)
        {
            ExpandTime = timeSpan;
            return this;
        }

        public IGroupByStartEndTimesConfiguratorOptional<T> ExpandTimeRange(string timeSpanExpression)
        {
            return ExpandTimeRange(timeSpanExpression.ToTimeSpan());
        }

        public IGroupByStartEndTimesConfiguratorOptional<T> TimeTolerance(TimeSpan tolerance)
        {
            Tolerance = tolerance;
            return this;
        }

        public IGroupByStartEndTimesConfiguratorOptional<T> TimeTolerance(string tolerance)
        {
            return TimeTolerance(tolerance.ToTimeSpan());
        }

        public virtual IReadOnlyList<StartEndTime> CreateGroupTimes()
        {
            GroupTimes =
                (GroupTimes?.Select(i => CreateGroupTime(i.Start, i.End)) ?? Enumerable.Empty<StartEndTime>()).ToList();
            return GroupTimes;
        }

        public override INullableQuerySerie<T> ExecuteGrouping()
        {
            var rows = Serie.Rows;
            if (!rows.Any() || !GroupTimes.Any())
                return new NullableQuerySerie<T>(new List<ISingleDataRow<T?>>(), Serie);

            int index = 0;
            var max = rows.Count;

            var result = new List<ISingleDataRow<T?>>();
            int? nextTimeRangeIndex = null;

            for (int groupIndex = 0; groupIndex < GroupTimes.Count; groupIndex++)
            {
                StartEndTime groupTime = GroupTimes[groupIndex];
                StartEndTime nextGroupTime = (groupIndex + 1 < GroupTimes.Count) ? GroupTimes[groupIndex + 1] : null;
                var group = new List<ISingleDataRow<T>>();
                while (index < max && rows[index].TimeUtc < groupTime.Start)
                {
                    index++;
                }
                var startIndex = index;

                while (index < max && rows[index].TimeUtc < groupTime.End)
                {
                    group.Add(rows[index]);
                    if (nextTimeRangeIndex == null && nextGroupTime != null && rows[index].TimeUtc > nextGroupTime.Start)
                    {
                        nextTimeRangeIndex = Math.Max(index - 1, 0);
                    }

                    index++;
                }
                var aggregationData = new QuerySerie<T>(@group, groupTime.Start, groupTime.End)
                {
                    PreviousRow = startIndex > 0 ? rows[startIndex - 1] : Serie.PreviousRow,
                    NextRow = index < max ? rows[index] : Serie.NextRow
                };
                result.Add(new SingleDataRow<T?>(groupTime.GetTimeStampByType(TimeStampType),
                    AggregationFunc(aggregationData)));
                if (nextTimeRangeIndex != null)
                {
                    index = nextTimeRangeIndex.Value;
                    nextTimeRangeIndex = null;
                }
            }

            var resultData = new NullableQuerySerie<T>(result, Serie);
            return resultData;
        }

        protected StartEndTime CreateGroupTime(DateTime startTime, DateTime endTime)
        {
            if (EndTimeIsStartTime)
            {
                endTime = startTime;
            }
            if (StartTimeIsEndTime)
            {
                startTime = endTime;
            }
            if (StartOffsetTimeSpan != null)
            {
                startTime = startTime + StartOffsetTimeSpan.Value;
            }
            if (EndOffsetTimeSpan != null)
            {
                endTime = endTime + EndOffsetTimeSpan.Value;
            }
            if (ExpandFactor != null)
            {
                double ms = (endTime - startTime).TotalMilliseconds;
                double oneSide = ((ExpandFactor.Value - 1)/2)*ms;
                startTime -= TimeSpan.FromMilliseconds(oneSide);
                endTime += TimeSpan.FromMilliseconds(oneSide);
            }
            if (ExpandTime != null)
            {
                startTime -= ExpandTime.Value;
                endTime += ExpandTime.Value;
            }
            return new StartEndTime(startTime, endTime);
        }
    }
}