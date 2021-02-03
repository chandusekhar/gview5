﻿using gView.Framework.Data;
using gView.Framework.Geometry;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace gView.DataSources.GeoJson
{
    class GeoJsonServiceFeatureClass : IFeatureClass
    {
        private readonly GeoJsonServiceDataset _dataset;

        private GeoJsonServiceFeatureClass(GeoJsonServiceDataset dataset)
        {
            _dataset = dataset;

            
        }

        async public static Task<GeoJsonServiceFeatureClass> CreateInstance(GeoJsonServiceDataset dataset, geometryType geometryType)
        {
            var instance = new GeoJsonServiceFeatureClass(dataset);
            instance.SpatialReference = await dataset.GetSpatialReference();
            instance.GeometryType = geometryType;
            instance.Name = $"{ dataset.DatasetName }-{ geometryType.ToString().ToLower() }";

            #region Loop all features for Fields

            var fields = new Fields();
            if (dataset.Source != null)
            {
                foreach (var feature in await dataset.Source.GetFeatures(geometryType))
                {
                    foreach(var fieldValue in feature.Fields)
                    {
                        if(fields.FindField(fieldValue.Name) == null)
                        {
                            var val = fieldValue.Value;
                            if(val?.GetType() == typeof(int))
                            {
                                fields.Add(new Field(fieldValue.Name, FieldType.integer));
                            }
                            else if (val?.GetType() == typeof(double))
                            {
                                fields.Add(new Field(fieldValue.Name, FieldType.Double));
                            }
                            else
                            {
                                // ToDo: Check for Date?
                                fields.Add(new Field(fieldValue.Name, FieldType.String));
                            }
                        }
                    }
                }
            }
            instance.Fields = fields;

            #endregion

            return instance;
        }

        #region IFeatureClass

        public string ShapeFieldName => "geometry";

        public IEnvelope Envelope => _dataset.Source?.Envelope ?? new Envelope();

        public IFields Fields { get; private set; }

        public string IDFieldName => "id";

        public string Name { get; private set; }

        public string Aliasname => this.Name;

        public IDataset Dataset => _dataset;

        public bool HasZ => false;

        public bool HasM => false;

        public ISpatialReference SpatialReference { get; internal set; }

        public geometryType GeometryType { get; private set;  }

        public Task<int> CountFeatures()
        {
            throw new NotImplementedException();
        }

        public IField FindField(string name)
        {
            return Fields.FindField(name);
        }

        async public Task<IFeatureCursor> GetFeatures(IQueryFilter filter)
        {
            if (filter is DistinctFilter)
                return new GeoJsonDistinctFeatureCursor(await _dataset.Source?.GetFeatures(this.GeometryType), (DistinctFilter)filter);

            return new GeoJsonFeatureCursor(await _dataset.Source?.GetFeatures(this.GeometryType), filter);
        }

        async public Task<ICursor> Search(IQueryFilter filter)
        {
            return (ICursor)await GetFeatures(filter);
        }

        public Task<ISelectionSet> Select(IQueryFilter filter)
        {
            return Task.FromResult<ISelectionSet>(null);
        }

        #endregion
    }
}