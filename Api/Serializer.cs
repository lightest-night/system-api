using System;
using System.Linq;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using RestSharp;
using RestSharp.Serialization;

namespace LightestNight.System.Api
{
    public class Serializer : IRestSerializer
    {
        public string ContentType { get; set; } = "application/json";
        public string[] SupportedContentTypes { get; }
        public DataFormat DataFormat { get; }

        public Serializer(Action<JsonSerializerSettings> settingsAction = null, DataFormat dataFormat = DataFormat.Json, params string[] supportedContentTypes)
        {
            var settings = new JsonSerializerSettings
            {
                Formatting = Formatting.Indented,
                TypeNameHandling = TypeNameHandling.None,
                ContractResolver = new CamelCasePropertyNamesContractResolver()
            };
            
            settingsAction?.Invoke(settings);

            DataFormat = dataFormat;
            SupportedContentTypes = supportedContentTypes;
            if (!SupportedContentTypes.Contains("application/json"))
                SupportedContentTypes = SupportedContentTypes.Union(new[] {"application/json"}).ToArray();
        }

        public string Serialize(object obj)
            => JsonConvert.SerializeObject(obj);

        public T Deserialize<T>(IRestResponse response)
            => JsonConvert.DeserializeObject<T>(response.Content);

        public string Serialize(Parameter parameter)
            => JsonConvert.SerializeObject(parameter.Value);
    }
}