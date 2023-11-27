﻿using gView.Framework.Core.Geometry;
using gView.Interoperability.GeoServices.Rest.Reflection;
using Newtonsoft.Json;

namespace gView.Interoperability.GeoServices.Rest.Json
{
    [ServiceMethod("Query", "query")]
    public class JsonLayer
    {
        public JsonLayer()
        {
            this.SubLayers = new JsonLayerLink[0];
            this.Capabilities = "Map,Query";
        }

        [JsonProperty("currentVersion")]
        public double CurrentVersion { get; set; }

        [JsonProperty("id")]
        public int Id { get; set; }

        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("type")]
        public string Type { get; set; }

        [JsonProperty("geometryType")]
        public string GeometryType { get; set; }

        [JsonProperty("subLayers")]
        public JsonLayerLink[] SubLayers { get; set; }

        [JsonProperty("parentLayer")]
        public JsonLayerLink ParentLayer { get; set; }

        [JsonProperty("minScale")]
        public double MinScale { get; set; }

        [JsonProperty("maxScale")]
        public double MaxScale { get; set; }

        [JsonProperty("defaultVisibility")]
        public bool DefaultVisibility { get; set; }

        [JsonProperty("fields")]
        public JsonField[] Fields { get; set; }

        [JsonProperty("extent")]
        public JsonExtent Extent { get; set; }

        [JsonProperty("drawingInfo")]
        public JsonDrawingInfo DrawingInfo { get; set; }

        [JsonProperty("capabilities")]
        public string Capabilities { get; set; }

        [JsonProperty("description")]
        public string Description { get; set; }

        [JsonProperty("copyrightText")]
        public string CopyrightText { get; set; }

        //[JsonIgnore]
        //public string FullName
        //{
        //    get
        //    {
        //        StringBuilder sb = new StringBuilder();

        //        if (this.ParentLayer != null)
        //        {
        //            sb.Append(this.ParentLayer.FullName);
        //            sb.Append("\\");
        //        }
        //        sb.Append(this.Name);

        //        return sb.ToString();
        //    }
        //}

        //[JsonIgnore]
        //public string ParentFullName
        //{
        //    get
        //    {
        //        if (this.ParentLayer != null)
        //            return ParentLayer.FullName + @"/";

        //        return String.Empty;
        //    }
        //}

        public GeometryType GetGeometryType()
        {
            switch (GeometryType)
            {
                case "esriGeometryPoint":
                    return Framework.Core.Geometry.GeometryType.Point;
                case "esriGeometryMultipoint":
                    return Framework.Core.Geometry.GeometryType.Multipoint;
                case "esriGeometryPolyline":
                    return Framework.Core.Geometry.GeometryType.Polyline;
                case "esriGeometryPolygon":
                    return Framework.Core.Geometry.GeometryType.Polygon;
                case "esriGeometryBag":
                    return Framework.Core.Geometry.GeometryType.Aggregate;
                case "esriGeometryEnvelope":
                    return Framework.Core.Geometry.GeometryType.Envelope;
            }

            return Framework.Core.Geometry.GeometryType.Unknown;
        }

        #region Static Members

        static public EsriGeometryType ToGeometryType(GeometryType type)
        {
            switch (type)
            {
                case Framework.Core.Geometry.GeometryType.Point:
                    return EsriGeometryType.esriGeometryPoint;
                case Framework.Core.Geometry.GeometryType.Multipoint:
                    return EsriGeometryType.esriGeometryMultipoint;
                case Framework.Core.Geometry.GeometryType.Polyline:
                    return EsriGeometryType.esriGeometryPolyline;
                case Framework.Core.Geometry.GeometryType.Polygon:
                    return EsriGeometryType.esriGeometryPolygon;
                case Framework.Core.Geometry.GeometryType.Aggregate:
                    return EsriGeometryType.esriGeometryBag;
                case Framework.Core.Geometry.GeometryType.Envelope:
                    return EsriGeometryType.esriGeometryEnvelope;
                case Framework.Core.Geometry.GeometryType.Unknown:
                case Framework.Core.Geometry.GeometryType.Network:
                default:
                    return EsriGeometryType.esriGeometryNull;
            }
        }

        #endregion
    }

    public class JsonLayerLink
    {
        [JsonProperty("id")]
        public int Id { get; set; }

        [JsonProperty("name")]
        public string Name { get; set; }
    }
}
