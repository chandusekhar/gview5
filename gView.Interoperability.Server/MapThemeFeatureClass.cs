﻿using gView.Framework.Core.Data;
using gView.Framework.Core.MapServer;
using gView.Framework.Core.system;
using gView.Framework.IO;
using gView.Framework.system;
using gView.Server.Connector;
using System;
using System.Threading.Tasks;

namespace gView.Interoperability.Server
{
    internal class MapThemeFeatureClass : gView.Framework.XML.AXLFeatureClass
    {
        public MapThemeFeatureClass(IDataset dataset, string id)
            : base(dataset, id)
        {
            if (dataset is IFeatureDataset)
            {
                this.SpatialReference = ((IFeatureDataset)dataset).GetSpatialReference().Result;
            }
        }

        protected override Task<string> SendRequest(IUserData userData, string axlRequest)
        {
            if (_dataset == null)
            {
                return Task.FromResult(String.Empty);
            }

            string server = ConfigTextStream.ExtractValue(_dataset.ConnectionString, "server");
            string service = ConfigTextStream.ExtractValue(_dataset.ConnectionString, "service");

            IServiceRequestContext context = (userData != null) ? userData.GetUserData("IServiceRequestContext") as IServiceRequestContext : null;
            string user = ConfigTextStream.ExtractValue(_dataset.ConnectionString, "user");
            string pwd = Identity.HashPassword(ConfigTextStream.ExtractValue(_dataset.ConnectionString, "pwd"));

            if ((user == "#" || user == "$") &&
                    context != null && context.ServiceRequest != null && context.ServiceRequest.Identity != null)
            {
                string roles = String.Empty;
                if (user == "#" && context.ServiceRequest.Identity.UserRoles != null)
                {
                    foreach (string role in context.ServiceRequest.Identity.UserRoles)
                    {
                        if (String.IsNullOrEmpty(role))
                        {
                            continue;
                        }

                        roles += "|" + role;
                    }
                }
                user = context.ServiceRequest.Identity.UserName + roles;
                // ToDo:
                //pwd = context.ServiceRequest.Identity.HashedPassword;
            }

            ServerConnection conn = new ServerConnection(server);
            string resp = conn.Send(service, axlRequest, "BB294D9C-A184-4129-9555-398AA70284BC", user, pwd);

            try
            {
                return Task.FromResult(conn.Send(service, axlRequest, "BB294D9C-A184-4129-9555-398AA70284BC", user, pwd));
            }
            catch (Exception ex)
            {
                MapServerClass.ErrorLog(context, "Query", server, service, ex);
                return Task.FromResult(String.Empty);
            }
        }
    }
}