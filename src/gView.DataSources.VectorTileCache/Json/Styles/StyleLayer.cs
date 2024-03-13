﻿using System.Text.Json.Serialization;

namespace gView.DataSources.VectorTileCache.Json.Styles;

public class StyleLayer
{
    [JsonPropertyName("id")]
    public string Id { get; set; }

    [JsonPropertyName("type")]
    public string Type { get; set; }

    [JsonPropertyName("source")]
    public string Source { get; set; }

    [JsonPropertyName("source-layer")]
    public string SourceLayerId { get; set; }

    [JsonPropertyName("minzoom")]
    public float? MinZoom { get; set; }

    [JsonPropertyName("maxzoom")]
    public float? MaxZoom { get; set; }

    [JsonPropertyName("layout")]
    public StyleLayerLayout Layout { get; set;}

    [JsonPropertyName("paint")]
    public StyleLayerPaint Paint { get; set; }
}
