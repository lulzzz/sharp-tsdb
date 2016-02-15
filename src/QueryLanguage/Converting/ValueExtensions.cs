﻿using System;
using System.Collections.Generic;
using System.Linq;
using DbInterfaces.Interfaces;
using FileDb.InterfaceImpl;

namespace QueryLanguage.Converting
{
    public static class ValueExtensions
    {
        public static INullableQuerySerie<T> CalcValue<T>(this INullableQuerySerie<T> serie,
            Func<T?, T?> calculationFunc, string newSerieName = null) where T : struct
        {
            var rows = new List<ISingleDataRow<T?>>(serie.Rows.Count);
            if (serie.Rows.Any())
            {
                rows.AddRange(serie.Rows.Select(row => new SingleDataRow<T?>(row.Key, calculationFunc(row.Value))));
            }
            var newSerie = new NullableQuerySerie<T>(rows, serie);
            if (newSerieName != null)
            {
                newSerie.Name = newSerieName;
            }
            return newSerie;
        }

        public static IQuerySerie<T> CalcValue<T>(this IQuerySerie<T> serie,
    Func<T, T> calculationFunc, string newSerieName = null) where T : struct
        {
            var rows = new List<ISingleDataRow<T>>(serie.Rows.Count);
            if (serie.Rows.Any())
            {
                rows.AddRange(serie.Rows.Select(row => new SingleDataRow<T>(row.Key, calculationFunc(row.Value))));
            }
            var newSerie = new QuerySerie<T>(rows, serie);
            if (newSerieName != null)
            {
                newSerie.Name = newSerieName;
            }
            return newSerie;
        }

        public static INullableQuerySerie<T> CalcNullableValue<T>(this IQuerySerie<T> serie,
    Func<T, T?> calculationFunc, string newSerieName = null) where T : struct
        {
            var rows = new List<ISingleDataRow<T?>>(serie.Rows.Count);
            if (serie.Rows.Any())
            {
                rows.AddRange(serie.Rows.Select(row => new SingleDataRow<T?>(row.Key, calculationFunc(row.Value))));
            }
            var newSerie = new NullableQuerySerie<T>(rows, serie);
            if (newSerieName != null)
            {
                newSerie.Name = newSerieName;
            }
            return newSerie;
        }
    }
}