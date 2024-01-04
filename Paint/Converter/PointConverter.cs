using Newtonsoft.Json.Linq;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Paint.Converter
{
    public class PointConverterJson : JsonConverter
    {
        public override bool CanConvert(Type objectType)
        {
            return (objectType == typeof(System.Windows.Point));
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            JObject jo = JObject.Load(reader);
            double x = jo["X"].Value<double>();
            double y = jo["Y"].Value<double>();
            return new System.Windows.Point(x, y);
        }

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            System.Windows.Point point = (System.Windows.Point)value;
            JObject jo = new JObject
        {
            { "X", point.X },
            { "Y", point.Y }
        };
            jo.WriteTo(writer);
        }
    }
}
