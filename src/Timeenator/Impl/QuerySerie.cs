﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Timeenator.Extensions.Converting;
using Timeenator.Interfaces;

namespace Timeenator.Impl
{
    [DebuggerDisplay("{FullName} ({Rows.Count()})")]
    public class QuerySerie<T> : QuerySerieBase<T>, IQuerySerie<T> where T : struct
    {
        public IReadOnlyList<ISingleDataRow<T>> Rows { get; }

        public override object this[int index]
        {
            get { return Rows[index].Value; }
            set
            {
                Rows[index].Value = (T)Convert.ChangeType(value, typeof(T));
            }
        }

        public IEnumerable<T> Values => Rows.Select(i => i.Value); 

        public QuerySerie(IReadOnlyList<ISingleDataRow<T>> rows, DateTime? startTime, DateTime? endTime)
            :base(startTime, endTime)

        {
            Rows = rows;
        }

        public QuerySerie(IReadOnlyList<ISingleDataRow<T>> rows, IQuerySerieBase<T> oldSerie) : base(oldSerie)
        {
            Rows = rows;
        }

        IReadOnlyList<IObjectSingleDataRow> IObjectQuerySerie.Rows => Rows;


        public IQuerySerie<T> IncludeLastRow()
        {
            if (Rows.Any())
            {
                LastRow = Rows.Last();
            }
            return this;
        }

        public IQuerySerie<T> Where(Func<ISingleDataRow<T>, bool> predicate)
        {
            if (Rows.Any())
            {
                return new QuerySerie<T>(Rows.Where(predicate).ToList(), this);
            }
            return this;
        }

        public IQuerySerie<T> WhereValue(Func<T, bool> predicate)
        {
            if (Rows.Any())
            {
                return new QuerySerie<T>(Rows.Where(i => predicate(i.Value)).ToList(), this);
            }
            return this;
        }

        public IQuerySerie<T> Alias(string name)
        {
            SetAlias(name);
            return this;
        }

        public INullableQuerySerie<T> Transform(Func<T, T?> transformFunc)
        {
            var newRows = new List<ISingleDataRow<T?>>(Rows.Count);
            newRows.AddRange(Rows.Select(r => new SingleDataRow<T?>(r.TimeUtc, transformFunc(r.Value))));
            return new NullableQuerySerie<T>(newRows, this);
        }

        /// <summary>
        /// Normalize a saw tooth like series due to overflows or resets
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="resetValue">let it null to take the first value after the overflow, otherwise set the value explicitly</param>
        /// <returns></returns>
        public IQuerySerie<T> NormalizeOverflows(double? resetValue = null)
        {
            if (Rows.Any())
            {
                var newRows = new List<ISingleDataRow<T>>(Rows.Count);
                double offset = 0;
                double previousValue = Rows.First().Value.ToDouble();
                foreach (var row in Rows)
                {
                    double rowValue = row.Value.ToDouble();
                    if (previousValue > rowValue)
                    {
                        if (resetValue != null)
                        {
                            offset += previousValue - (rowValue - resetValue.Value);
                        }
                        else
                        {
                            offset += previousValue;
                        }

                    }
                    newRows.Add(new SingleDataRow<T>(row.TimeUtc, (rowValue + offset).ToType<T>()));
                    previousValue = rowValue;
                }
                return new QuerySerie<T>(newRows, this);
            }
            return this;
        }

        public INullableQuerySerie<T> ToNullable()
        {
            return new NullableQuerySerie<T>(Rows.Select(i => new SingleDataRow<T?>(i.TimeUtc, i.Value)).ToList(), this);
        }
    }
}