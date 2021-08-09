using System;
using System.Text.Json.Serialization;
using OpenTK.Mathematics;
using System.Text.Json;

namespace Toys
{
    public class QuaternionJSONConverter : JsonConverter<Quaternion>
    {

        public override Quaternion Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            reader.Read();
            var x = JsonSerializer.Deserialize<float>(ref reader, options);

            reader.Read();
            var y = JsonSerializer.Deserialize<float>(ref reader, options);

            reader.Read();
            var z = JsonSerializer.Deserialize<float>(ref reader, options);

            reader.Read();
            var w = JsonSerializer.Deserialize<float>(ref reader, options);

            reader.Read();

            return new Quaternion(x, y, z, w);
        }


        public override void Write(Utf8JsonWriter writer, Quaternion value, JsonSerializerOptions options)
        {
            writer.WriteStartObject();

            writer.WritePropertyName("X");
            JsonSerializer.Serialize(writer, value.X, options);

            writer.WritePropertyName("Y");
            JsonSerializer.Serialize(writer, value.Y, options);

            writer.WritePropertyName("Z");
            JsonSerializer.Serialize(writer, value.Z, options);

            writer.WritePropertyName("W");
            JsonSerializer.Serialize(writer, value.W, options);
            writer.WriteEndObject();
        }
    }
}
