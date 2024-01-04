using Newtonsoft.Json.Linq;
using System;

namespace Paint.Converter
{
    public class ColorConverterJson : Newtonsoft.Json.JsonConverter
    {
        public override bool CanConvert(Type objectType)
        {
            return (objectType == typeof(System.Windows.Media.Color));
        }

        public override object ReadJson(Newtonsoft.Json.JsonReader reader, Type objectType, object existingValue, Newtonsoft.Json.JsonSerializer serializer)
        {
            JObject jo = JObject.Load(reader);
            byte a = jo["A"].Value<byte>();
            byte r = jo["R"].Value<byte>();
            byte g = jo["G"].Value<byte>();
            byte b = jo["B"].Value<byte>();
            return System.Windows.Media.Color.FromArgb(a, r, g, b);
        }

        public override void WriteJson(Newtonsoft.Json.JsonWriter writer, object value, Newtonsoft.Json.JsonSerializer serializer)
        {
            System.Windows.Media.Color color = (System.Windows.Media.Color)value;
            JObject jo = new JObject
        {
            { "A", color.A },
            { "R", color.R },
            { "G", color.G },
            { "B", color.B }
        };
            jo.WriteTo(writer);
        }
    }
}
