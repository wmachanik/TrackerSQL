using System.Web.Http;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace TrackerSQL.Api
{
    /// <summary>Mobile REST API (/api/v1). Registered from Global.asax Application_Start.</summary>
    public static class WebApiConfig
    {
        public static readonly JsonSerializerSettings JsonSettings = new JsonSerializerSettings
        {
            ContractResolver = new CamelCasePropertyNamesContractResolver(),
            NullValueHandling = NullValueHandling.Ignore,
            Formatting = Formatting.None,
            DateFormatString = "yyyy-MM-ddTHH:mm:ss"
        };

        public static void Register(HttpConfiguration config)
        {
            config.MapHttpAttributeRoutes();

            config.Formatters.Remove(config.Formatters.XmlFormatter);
            config.Formatters.JsonFormatter.SerializerSettings = JsonSettings;

            // Outermost first: CORS answers preflights, security headers cover every reply (401/429 too), the log sees the bytes
            // actually sent, compression wraps everything, auth runs last.
            config.MessageHandlers.Add(new MobileApiCorsHandler());
            config.MessageHandlers.Add(new MobileApiSecurityHeadersHandler());
            config.MessageHandlers.Add(new MobileApiLogHandler());
            config.MessageHandlers.Add(new MobileApiCompressionHandler());
            config.MessageHandlers.Add(new MobileApiAuthHandler());

            config.Filters.Add(new MobileApiExceptionFilter());
            config.IncludeErrorDetailPolicy = IncludeErrorDetailPolicy.LocalOnly;
        }
    }
}
