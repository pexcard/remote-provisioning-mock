using Newtonsoft.Json;

namespace RemoteProvisioningServiceMock.Extensions
{
    public static class JsonExtensions
    {
        public static string ToJson(this object obj)
        {
            return obj == null 
                ? null 
                : JsonConvert.SerializeObject(obj);
        }

        public static T DeserializeTo<T>(this string value) where T : class
        {
            return value == null 
                ? null 
                : JsonConvert.DeserializeObject<T>(value);
        }
    }
}