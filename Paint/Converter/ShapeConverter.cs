using MyLib;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json;
using System;
using EllipseLib;
using LineLib;
using RectangleLib;

namespace Paint
{
    public class ShapeConverter : JsonConverter
    {
        public override bool CanConvert(Type objectType)
        {
            return (objectType == typeof(IShape));
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            JObject jo = JObject.Load(reader);
            IShape shape;
            switch (jo["Name"].Value<string>())
            {
                case "Ellipse":
                    shape = new EllipseShape();
                    break;
                case "Line":
                    shape = new LineShape();
                    break;
                case "Rectangle":
                    shape = new RectangleShape();
                    break;
                default:
                    throw new Exception();
            }
            serializer.Populate(jo.CreateReader(), shape);
            return shape;
        }

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            throw new NotImplementedException(); // Implement this if you need to serialize back to JSON
        }
    }
}
