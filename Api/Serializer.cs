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
        private const string JsonContentType = "application/json";
        private readonly JsonSerializerSettings _serializerSettings;
        
        public string ContentType { get; set; } = JsonContentType;
        public string[] SupportedContentTypes { get; }
        public DataFormat DataFormat { get; }

        public Serializer(Action<JsonSerializerSettings> settingsAction = null, DataFormat dataFormat = DataFormat.Json, params string[] supportedContentTypes)
        {
            _serializerSettings = new JsonSerializerSettings
            {
                Formatting = Formatting.Indented,
                TypeNameHandling = TypeNameHandling.None,
                ContractResolver = new CamelCasePropertyNamesContractResolver()
            };
            
            settingsAction?.Invoke(_serializerSettings);

            DataFormat = dataFormat;
            SupportedContentTypes = supportedContentTypes;
            if (!SupportedContentTypes.Contains(JsonContentType))
                SupportedContentTypes = SupportedContentTypes.Union(new[] {JsonContentType}).ToArray();
        }

        public string Serialize(object obj)
            => JsonConvert.SerializeObject(obj, _serializerSettings);

        public T Deserialize<T>(IRestResponse response)
            => JsonConvert.DeserializeObject<T>(response.Content, _serializerSettings);

        public string Serialize(Parameter parameter)
            => JsonConvert.SerializeObject(parameter.Value, _serializerSettings);
    }
}