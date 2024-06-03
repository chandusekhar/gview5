﻿using gView.Interoperability.GeoServices.Rest.Json.Features;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace gView.Interoperability.GeoServices.Rest.Json
{
    public class JsonFeatureResponse : JsonStopWatch
    {
        [JsonPropertyName("displayFieldName")]
        public string DisplayFieldName { get; set; }

        [JsonPropertyName("fieldAliases")]
        public dynamic FieldAliases { get; set; } // object

        [JsonPropertyName("geometryType")]
        public string GeometryType { get; set; }

        [JsonPropertyName("spatialReference")]
        public JsonSpatialReference SpatialReference { get; set; }

        [JsonPropertyName("fields")]
        public Field[] Fields { get; set; }

        [JsonPropertyName("features")]
        public JsonFeature[] Features { get; set; }

        [JsonPropertyName("exceededTransferLimit")]
        public bool ExceededTransferLimit { get; set; }

        #region Classes

        public class Field
        {
            [JsonPropertyName("name")]
            public string Name { get; set; }

            [JsonPropertyName("type")]
            public string Type { get; set; }

            [JsonPropertyName("alias")]
            public string Alias { get; set; }

            [JsonPropertyName("length")]
            [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
            public int? Length { get; set; }

            //public class VType
            //{
            //    [JsonPropertyName("value")]
            //    public string Value { get; set; }
            //}
        }

        #endregion
    }
}
