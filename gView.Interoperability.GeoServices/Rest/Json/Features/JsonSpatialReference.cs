﻿using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace gView.Interoperability.GeoServices.Rest.Json.Features
{
    public class JsonSpatialReference
    {
        public JsonSpatialReference() { }

        public JsonSpatialReference(int id)
        {
            this.Wkid = id;
        }

        [JsonProperty("wkt")]
        public string Wkt { get; set; }

        [JsonProperty("wkid")]
        public int Wkid { get; set; }
    }
}
