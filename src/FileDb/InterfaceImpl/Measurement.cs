﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization;
using DbInterfaces.Interfaces;
using FileDb.Properties;

namespace FileDb.InterfaceImpl
{
    [DataContract]
    public class Measurement : IMeasurement
    {
        private static RowReadWriterFactory _rowReadWriterFactory;

        [DataMember]
        private readonly Db _db;

        public string BinaryFilePath { get; private set; }

        [DataMember]
        public MeasurementMetadata MetadataInternal { get; set; }

        public void ClearDataPoints()
        {
            lock (this)
            {
                using (var fs = File.Open(BinaryFilePath, FileMode.Create, FileAccess.Write, FileShare.None))
                {
                  
                }
            }
        }

        public IMeasurementMetadata Metadata => MetadataInternal;

        private RowReaderWriter _rowReaderWriter;

        private const int MinSearchRange = 1000;

        /// <summary>
        /// Only for deserialization
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
        /// Creates a measurement with the specified name, DateTime as Key and the specified type as Value
        /// </summary>
        /// <param name="name"></param>
        /// <param name="type"></param>
        /// <param name="db"></param>
        public Measurement(string name, Type type, Db db)
        {
            _db = db;
            MetadataInternal = new MeasurementMetadata(name);
            MetadataInternal.ColumnsInternal.Add(new Column(Settings.Default.KeyColumnName, typeof(DateTime)));
            MetadataInternal.ColumnsInternal.Add(new Column(Settings.Default.ValueColumnName, type));
            Init();
        }

        [OnDeserialized]
        private void OnDeserialized(StreamingContext c)
        {
            Init();
        }

        private void Init()
        {
            BinaryFilePath = Path.Combine(_db.MeasurementDirectory, $"{MetadataInternal.Id}{Settings.Default.BinaryFileExtension}");
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

        public void AppendDataPoints(IEnumerable<IDataRow> row)
        {
            lock (this)
            {
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
        }

        public IEnumerable<IDataRow> GetDataPoints(DateTime? @from = null, DateTime? to = null)
        {
            lock (this)
            {
                DateTime start = from ?? DateTime.MinValue;
                DateTime stop = to ?? DateTime.MaxValue;

                using (var fs = File.Open(BinaryFilePath, FileMode.Open, FileAccess.Read, FileShare.Read))
                {
                    var items = fs.Length / _rowReaderWriter.RowLength;
                    var currentItem = items / 2;

                    using (var binaryReader = new BinaryReader(fs))
                    {
                        long itemToStart = GetItemToStart(start, fs, binaryReader, currentItem, currentItem, 0);
                        fs.Position = itemToStart * _rowReaderWriter.RowLength;

                        IDataRow readRow = null;
                        IDataRow firstRow = null;

                        while (fs.Position < fs.Length)
                        {
                            readRow = _rowReaderWriter.ReadRow(binaryReader);

                            if (readRow.Key >= start)
                            {
                                if (firstRow != null)
                                {
                                    yield return firstRow;
                                    firstRow = null;
                                }
                                yield return readRow;
                            }
                            else
                            {
                                firstRow = readRow;
                            }

                            if (readRow.Key >= stop)
                            {
                                //if fill(next) should be interesting, maybe this row should be returned too
                                break;
                            }
                        }
                    }
                }
            }
        }

        private long GetItemToStart(DateTime start, FileStream fs, BinaryReader binaryReader, long currentIndex, long currentRange, long lastValidIndex)
        {
            if (currentRange < MinSearchRange)
            {
                return lastValidIndex;
            }
            fs.Position = currentIndex * _rowReaderWriter.RowLength;
            var time = DateTime.FromBinary(binaryReader.ReadInt64());
            currentRange = currentRange / 2;
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