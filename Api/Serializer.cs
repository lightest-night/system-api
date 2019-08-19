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

        public Serializer()
        {
            JsonConvert.DefaultSettings = () => new JsonSerializerSettings
            {
                Formatting = Formatting.Indented,
                TypeNameHandling = TypeNameHandling.Auto,
                ContractResolver = new CamelCasePropertyNamesContractResolver()
            };
            SupportedContentTypes = new[] {"application/json"};
            DataFormat = DataFormat.Json;
        }

        public string Serialize(object obj)
            => JsonConvert.SerializeObject(obj);

        public T Deserialize<T>(IRestResponse response)
            => JsonConvert.DeserializeObject<T>(response.Content);

        public string Serialize(Parameter parameter)
            => JsonConvert.SerializeObject(parameter.Value);
    }
}