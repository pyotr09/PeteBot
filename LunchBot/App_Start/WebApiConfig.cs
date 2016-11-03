﻿using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using System.Configuration;
using System.Web.Http;

namespace LunchBot
{
    public static class WebApiConfig
    {
        public static void Register(HttpConfiguration config)
        {
            // Json settings
            config.Formatters.JsonFormatter.SerializerSettings.NullValueHandling = NullValueHandling.Ignore;
            config.Formatters.JsonFormatter.SerializerSettings.ContractResolver = new CamelCasePropertyNamesContractResolver();
            config.Formatters.JsonFormatter.SerializerSettings.Formatting = Formatting.Indented;
            JsonConvert.DefaultSettings = () => new JsonSerializerSettings()
            {
                ContractResolver = new CamelCasePropertyNamesContractResolver(),
                Formatting = Newtonsoft.Json.Formatting.Indented,
                NullValueHandling = NullValueHandling.Ignore,
            };

            // Web API configuration and services

            // Web API routes
            config.MapHttpAttributeRoutes();

            config.Routes.MapHttpRoute(
                name: "DefaultApi",
                routeTemplate: "api/{controller}/{id}",
                defaults: new { id = RouteParameter.Optional }
            );

            string appSetting = ConfigurationManager.AppSettings["AdminUsers"];
            if (!string.IsNullOrEmpty(appSetting))
            {
                string[] users = appSetting.Split(';');
                foreach (string user in users)
                {
                    DataStore.Instance.MakeAdmin(user);
                }
            }
        }
    }
}
