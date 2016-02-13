﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization;
using System.Threading;
using DbInterfaces.Interfaces;
using FileDb.Properties;

namespace FileDb.InterfaceImpl
{
    [DataContract]
    public class Measurement : IMeasurement
    {
        private const int MinSearchRange = 1000;
        private static RowReadWriterFactory _rowReadWriterFactory;
        private static readonly TimeSpan _readWriteTimeOut = TimeSpan.FromMinutes(3);

        [DataMember] private readonly Db _db;

        private readonly ReaderWriterLock _rwl = new ReaderWriterLock();
        private RowReaderWriter _rowReaderWriter;

        /// <summary>
        ///     Only for deserialization
        /// </summary>
        private Measurement()
        {
        }

        public Measurement(MeasurementMetadata metadataInternal, Db db)
        {
            _db = db;
            MetadataInternal = metadataInternal;
            Init();
        }

        /// <summary>
        ///     Creates a measurement with the specified name, DateTime as Key and the specified type as Value
        /// </summary>
        /// <param name="name"></param>
        /// <param name="type"></param>
        /// <param name="db"></param>
        public Measurement(string name, Type type, Db db)
        {
            _db = db;
            MetadataInternal = new MeasurementMetadata(name);
            MetadataInternal.ColumnsInternal.Add(new Column(Settings.Default.KeyColumnName, typeof (DateTime)));
            MetadataInternal.ColumnsInternal.Add(new Column(Settings.Default.ValueColumnName, type));
            Init();
        }

        [DataMember]
        public MeasurementMetadata MetadataInternal { get; set; }

        public string BinaryFilePath { get; private set; }

        public IQueryData<T> GetDataPoints<T>(string timeExpression) where T : struct
        {
            var expression = new TimeExpression(timeExpression);
            return GetDataPoints<T>(expression.From, expression.To);
        }

        public void ClearDataPoints()
        {
            try
            {
                _rwl.AcquireWriterLock(_readWriteTimeOut);
                using (var fs = File.Open(BinaryFilePath, FileMode.Create, FileAccess.Write, FileShare.None))
                {
                }
            }
            finally
            {
                _rwl.ReleaseWriterLock();
            }
        }

        public IMeasurementMetadata Metadata => MetadataInternal;

        public void AppendDataPoints(IEnumerable<IDataRow> row)
        {
            try
            {
                _rwl.AcquireWriterLock(_readWriteTimeOut);
                using (var fs = File.Open(BinaryFilePath, FileMode.Append, FileAccess.Write, FileShare.None))
                {
                    using (var bw = new BinaryWriter(fs))
                    {
                        foreach (var dataRow in row)
                        {
                            _rowReaderWriter.WriteRow(bw, dataRow);
                        }
                    }
                }
            }
            finally
            {
                _rwl.ReleaseWriterLock();
            }
        }

        public IQueryData<T> GetDataPoints<T>(DateTime? @from = null, DateTime? to = null) where T : struct
        {
            try
            {
                _rwl.AcquireReaderLock(_readWriteTimeOut);
                var start = from ?? DateTime.MinValue;
                var stop = to ?? DateTime.MaxValue;

                using (var fs = File.Open(BinaryFilePath, FileMode.Open, FileAccess.Read, FileShare.Read))
                {
                    var items = fs.Length/_rowReaderWriter.RowLength;
                    var currentItem = items/2;

                    using (var binaryReader = new BinaryReader(fs))
                    {
                        var itemToStart = GetItemToStart(start, fs, binaryReader, currentItem, currentItem, 0);
                        if (itemToStart > 0)
                        {
                            itemToStart--;
                        }
                        fs.Position = itemToStart*_rowReaderWriter.RowLength;

                        return ReadRows<T>(fs, binaryReader, start, stop, from, to);
                    }
                }
            }
            finally
            {
                _rwl.ReleaseReaderLock();
            }
        }

        [OnDeserialized]
        private void OnDeserialized(StreamingContext c)
        {
            Init();
        }

        private void Init()
        {
            BinaryFilePath = Path.Combine(_db.MeasurementDirectory,
                $"{MetadataInternal.Id}{Settings.Default.BinaryFileExtension}");
            if (!File.Exists(BinaryFilePath))
            {
                using (File.Create(BinaryFilePath))
                {
                }
            }
            if (_rowReadWriterFactory == null)
            {
                _rowReadWriterFactory = new RowReadWriterFactory();
            }
            _rowReaderWriter = _rowReadWriterFactory.CreateRowReaderWriter(MetadataInternal);
        }

        private IQueryData<T> ReadRows<T>(FileStream fs, BinaryReader binaryReader, DateTime start,
            DateTime stop, DateTime? @from = null, DateTime? to = null) where T : struct
        {
            ISingleDataRow<T> firstRow = null;
            var rows = new List<ISingleDataRow<T>>();
            var data = new QueryData<T>(rows, from, to);

            while (fs.Position < fs.Length)
            {
                var readRow = _rowReaderWriter.ReadRow<T>(binaryReader);

                if (readRow.Key >= start)
                {
                    if (readRow.Key <= stop)
                    {
                        rows.Add(readRow);
                    }
                }
                else
                {
                    firstRow = readRow;
                }

                if (readRow.Key >= stop)
                {
                    data.NextRow = readRow;
                    break;
                }
            }

            data.Name = Metadata.Name;
            data.PreviousRow = firstRow;

            return data;
        }

        private long GetItemToStart(DateTime start, FileStream fs, BinaryReader binaryReader, long currentIndex,
            long currentRange, long lastValidIndex)
        {
            if (currentRange < MinSearchRange)
            {
                return lastValidIndex;
            }
            fs.Position = currentIndex*_rowReaderWriter.RowLength;
            var time = DateTime.FromBinary(binaryReader.ReadInt64());
            currentRange = currentRange/2;
            if (time >= start)
            {
                currentIndex = currentIndex - currentRange;
            }
            else
            {
                lastValidIndex = Math.Max(currentIndex, 0);
                currentIndex = lastValidIndex + currentRange;
            }

            return GetItemToStart(start, fs, binaryReader, currentIndex, currentRange, lastValidIndex);
        }
    }
}