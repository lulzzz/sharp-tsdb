using System;
using System.Collections.Generic;

namespace DbInterfaces.Interfaces
{
    public interface IDb
    {
        IDbMetadata Metadata { get; }
        string Name { get; }
        void SaveMetadata();
        string MeasurementDirectory { get; }
        void CreateMeasurement(IMeasurementMetadata metadata);
        IMeasurement CreateMeasurement(string name, Type valueType);
        IMeasurement GetMeasurement(string name);
        IReadOnlyList<string> GetMeasurementNames();
        void DeleteMeasurement(string name);
        void DeleteAllMeasurements();
    }
}