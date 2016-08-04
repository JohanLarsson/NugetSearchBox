namespace NugetSearchBox
{
    using System;
    using System.IO;
    using System.Text;
    using System.Threading;
    using Newtonsoft.Json;

    public class PackageInfoConverter : JsonConverter
    {
        public static readonly PackageInfoConverter Default = new PackageInfoConverter();

        private PackageInfoConverter()
        {
        }

        private static readonly ThreadLocal<StringBuilder> Stringbuilder = new ThreadLocal<StringBuilder>(() => new StringBuilder());

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            throw new System.NotImplementedException("don't think this will be used");
        }

        public override bool CanConvert(Type objectType)
        {
            return objectType == typeof(PackageInfo);
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            var sb = Stringbuilder.Value;
            sb.Clear();
            var id = ReadElementTo(reader, sb);
            NugetCache.JsonAndPackageInfo cachedItem;
            if (NugetCache.TryGet(id, out cachedItem))
            {
                if (Equals(cachedItem.Json, sb))
                {
                    return cachedItem.Package;
                }
            }

            var json = sb.ToString();
            var packageInfo = JsonConvert.DeserializeObject<PackageInfo>(json);
            NugetCache.UpdatePackageCache(id, new NugetCache.JsonAndPackageInfo(json, packageInfo));
            return packageInfo;
        }

        private static string ReadElementTo(JsonReader reader, StringBuilder sb)
        {
            string id = null;
            using (var writer = new StringWriter(sb))
            {
                using (JsonTextWriter jsonTextWriter = new JsonTextWriter(writer))
                {
                    jsonTextWriter.WriteStartObject();
                    var startDepth = reader.Depth;
                    while (reader.Read() && reader.Depth > startDepth)
                    {
                        switch (reader.TokenType)
                        {
                            case JsonToken.StartObject:
                                jsonTextWriter.WriteStartObject();
                                break;
                            case JsonToken.EndObject:
                                jsonTextWriter.WriteEndObject();
                                break;
                            case JsonToken.StartArray:
                                jsonTextWriter.WriteStartArray();
                                break;
                            case JsonToken.EndArray:
                                jsonTextWriter.WriteEndArray();
                                break;
                            case JsonToken.PropertyName:
                                jsonTextWriter.WritePropertyName((string)reader.Value);
                                if ((string)reader.Value == "id")
                                {
                                    id = reader.ReadAsString();
                                    jsonTextWriter.WriteValue(id);
                                }
                                break;
                            case JsonToken.Integer:
                            case JsonToken.String:
                            case JsonToken.Date:
                            case JsonToken.Float:
                            case JsonToken.Boolean:
                                jsonTextWriter.WriteValue(reader.Value);
                                break;
                            case JsonToken.Raw:
                            case JsonToken.Null:
                            case JsonToken.Comment:
                            case JsonToken.StartConstructor:
                            case JsonToken.Undefined:
                            case JsonToken.EndConstructor:
                            case JsonToken.None:
                            case JsonToken.Bytes:
                            default:
                                throw new ArgumentOutOfRangeException();
                        }
                    }
                }
            }

            return id;
        }

        private static bool Equals(string json, StringBuilder sb)
        {
            if (json.Length != sb.Length)
            {
                return false;
            }

            for (int i = 0; i < json.Length; i++)
            {
                if (json[i] != sb[i])
                {
                    return false;
                }
            }

            return true;
        }
    }
}
