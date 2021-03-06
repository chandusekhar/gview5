﻿using gView.Framework.Geometry;
using gView.Interoperability.GeoServices.Rest.Reflection;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace gView.Interoperability.GeoServices.Rest.Json
{
    [ServiceMethod("Query", "query")]
    [ServiceMethod("Add Feature", "addfeatures")]
    [ServiceMethod("Update Feature", "updatefeatures")]
    [ServiceMethod("Delete Feature", "deletefeatures")]
    public class JsonFeatureServerLayer : JsonLayer
    {
        public JsonFeatureServerLayer()
            : base()
        {

        }
    }
}
