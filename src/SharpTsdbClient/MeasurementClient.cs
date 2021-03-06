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
using System.Threading.Tasks;
using SharpTsdbTypes.Communication;
using Timeenator.Extensions;
using Timeenator.Interfaces;

namespace SharpTsdbClient
{
    public class MeasurementClient : ClientBase
    {
        public MeasurementClient(DbClient db, string measurementName)
        {
            Db = db;
            MeasurementName = measurementName;
        }

        public string MeasurementName { get; set; }

        /// <summary>
        ///     Add points to a measurement
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="points">points to add</param>
        /// <param name="truncateDbToFirstElement">
        ///     false: No check, if there are already points after the start of the new points
        ///     true: The DB deletes all points after the timestamp of the first element of the data to add
        /// </param>
        /// <returns></returns>
        public async Task<string> AppendAsync<T>(IEnumerable<ISingleDataRow<T>> points, bool truncateDbToFirstElement)
        {
            //points = points.OrderBy(i => i.)
            string url = $"db/{Db.DbName}/{MeasurementName}/appendRows?type={typeof (T).ToShortCode()}";
            if (truncateDbToFirstElement)
            {
                url += "&truncateDbToFirstElement=true";
            }
            return await PostRequestAsync<string>(url, new DataRows(points));
        }

        public async Task<string> MergeAsync<T>(IEnumerable<ISingleDataRow<T>> points, string minInterval = null, bool onlyChangedValues = false)
        {
            //points = points.OrderBy(i => i.)
            string url = $"db/{Db.DbName}/{MeasurementName}/mergeRows?type={typeof(T).ToShortCode()}";
            if (minInterval != null)
            {
                url += $"&minInterval={minInterval}";
            }
            if (onlyChangedValues)
            {
                url += $"&onlyChangedValues=true";
            }
            return await PostRequestAsync<string>(url, new DataRows(points));
        }

        public async Task<string> CreateAsync(Type type)
        {
            return
                await
                    GetRequestAsync($"db/{Db.DbName}/{MeasurementName}/create?type={type.ToShortCode()}");
        }

        public async Task<string> ClearAsync(DateTime? after = null)
        {
            return
                await
                    GetRequestAsync($"db/{Db.DbName}/{MeasurementName}/clear?after={after?.ToFileTimeUtc()}");
        }

        public async Task<string> DeleteAsync()
        {
            return
                await
                    GetRequestAsync($"db/{Db.DbName}/{MeasurementName}/delete");
        }

        public async Task<string> DeleteByRegexAsync()
        {
            return
                await
                    GetRequestAsync($"db/{Db.DbName}/{MeasurementName}/deleteByRegex");
        }

        public async Task<string> DeleteByNameRegexAsync()
        {
            return
                await
                    GetRequestAsync($"db/{Db.DbName}/{MeasurementName}/deleteByNameRegex");
        }

        public async Task<bool> AddAliasesAsync(string newAliasReplacePattern)
        {
            return (await GetRequestAsync($"db/{Db.DbName}/{MeasurementName}/addAlias/{newAliasReplacePattern}") == "ok");
        }

        public async Task<bool> RemoveAliasesAsync()
        {
            return (await GetRequestAsync($"db/{Db.DbName}/{MeasurementName}/removeAliases") == "ok");
        }

    }
}