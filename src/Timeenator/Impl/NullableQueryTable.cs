using System.Collections.Generic;
using Timeenator.Interfaces;

namespace Timeenator.Impl
{
    public class NullableQueryTable<T> : QueryTableBase<T>, INullableQueryTable<T> where T : struct
    {
        public new IDictionary<string, INullableQuerySerie<T>> Series { get; } = new Dictionary<string, INullableQuerySerie<T>>();
        protected override IEnumerable<IObjectQuerySerie> GetSeries() => Series.Values;

        public override IObjectQuerySerieBase TryGetSerie(string name)
        {
            return ((INullableQueryTable<T>)this).TryGetSerie(name);
        }

        IEnumerable<INullableQuerySerie<T>> INullableQueryTable<T>.Series => Series.Values;

        IEnumerable<IObjectQuerySerie> IObjectQueryTable.Series => Series.Values;

        public INullableQueryTable<T> AddSerie(INullableQuerySerie<T> serie)
        {
            Series[serie.Name] = serie;
            return this;
        }

        public INullableQueryTable<T> RemoveSerie(string name)
        {
            Series.Remove(name);
            return this;
        }

        public INullableQueryTable<T> MergeTable(INullableQueryTable<T> otherTable)
        {
            foreach (var serie in otherTable.Series)
            {
                Series[serie.Name] = serie;
            }
            return this;
        }

        INullableQuerySerie<T> INullableQueryTable<T>.TryGetSerie(string name)
        {
            foreach (var serie in Series.Values)
            {
                if (serie.Key == name) return serie;
                if (serie.Name == name) return serie;
            }
            
            return null;
        }
    }
}