using System;
using System.Text.Json.Serialization;
using OpenTK.Mathematics;
using System.Text.Json;

namespace Toys.Engine.Systen.Saving
{
    public class Vector4JSONConverter : JsonConverter<Vector4>
    {
        public override Vector4 Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
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

            return new Vector4(x, y, z, w);
        }


        public override void Write(Utf8JsonWriter writer, Vector4 value, JsonSerializerOptions options)
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
