using System;
using System.Text.Json.Serialization;
using OpenTK.Mathematics;
using System.Text.Json;

namespace Toys
{
    public class Vector3JSONConverter : JsonConverter<Vector3>
    {
        public override Vector3 Read(ref Utf8JsonReader reader, Type typeToConvert,  JsonSerializerOptions options)
        {
            reader.Read();
            var x = JsonSerializer.Deserialize<float>(ref reader, options);

            reader.Read();
            var y = JsonSerializer.Deserialize<float>(ref reader, options);

            reader.Read();
            var z = JsonSerializer.Deserialize<float>(ref reader, options);

            reader.Read();

            return new Vector3(x, y, z);
        }


        public override void Write(Utf8JsonWriter writer, Vector3 value, JsonSerializerOptions options)
        {
            writer.WriteStartObject();

            writer.WritePropertyName("X");
            JsonSerializer.Serialize(writer,value.X, options);

            writer.WritePropertyName("Y");
            JsonSerializer.Serialize(writer, value.Y, options);

            writer.WritePropertyName("Z");
            JsonSerializer.Serialize(writer, value.Z, options);
            writer.WriteEndObject();
        }
    }
}
