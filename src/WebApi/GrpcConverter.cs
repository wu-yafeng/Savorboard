using Google.Protobuf;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace WebApi
{
    public class ByteStringConverter : JsonConverter<ByteString>
    {
        public override ByteString? Read(ref Utf8JsonReader reader, System.Type typeToConvert, JsonSerializerOptions options)
        {
            return ByteString.CopyFromUtf8(reader.GetString());
        }

        public override void Write(Utf8JsonWriter writer, ByteString value, JsonSerializerOptions options)
        {
            writer.WriteStringValue(value.ToStringUtf8());
        }
    }
}
