﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using DbInterfaces.Interfaces;
using FileDb.Properties;
using Infrastructure;

namespace FileDb.InterfaceImpl
{
    public class Db : IDb
    {
        public void SaveMetadata()
        {
            MetadataInternal.SaveToFile(MetadataInternal.DbMetadataPath);
        }

        public string MeasurementDirectory => Path.Combine(MetadataInternal.DbPath, Settings.Default.MeasurementDirectory);

        public Db(DbMetadata metadata)
        {
            MetadataInternal = metadata;
            Directory.CreateDirectory(MeasurementDirectory);
        }

        public DbMetadata MetadataInternal { get; set; }

        public IDbMetadata Metadata
        {
            get { return MetadataInternal; }
        }

        public void CreateMeasurement(IMeasurementMetadata metadata)
        {
            MetadataInternal.SetMeasurement(metadata.Name, new Measurement((MeasurementMetadata) metadata, this));
        }

        public void CreateMeasurement(string name, Type valueType)
        {
            var meas = new Measurement(name, valueType, this);
            MetadataInternal.SetMeasurement(name, meas);
            SaveMetadata();
        }

        public IMeasurement GetMeasurement(string name)
        {
            return MetadataInternal.GetMeasurement(name);
        }

        public IReadOnlyList<string> GetMeasurementNames()
        {
            return MetadataInternal.Measurements.Keys.ToList();
        }

        public void DeleteMeasurement(string name)
        {
            MetadataInternal.Measurements.Remove(name);
        }

        public void DeleteAllMeasurements()
        {
            MetadataInternal.Measurements.Clear();
        }
    }
}