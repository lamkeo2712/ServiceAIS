//using System;
//using System.Text.Json;
//using System.Text.Json.Serialization;

//namespace myAISapi.Data
//{
//	public class NullConverter<T> : JsonConverter<T> where T : class
//	{
//		public override T? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
//    {
//        if (reader.TokenType == JsonTokenType.StartObject)
//        {
//            using (JsonDocument doc = JsonDocument.ParseValue(ref reader))
//            {
//                // Kiểm tra nếu object rỗng thì trả về null
//                if (doc.RootElement.EnumerateObject().Any() == false)
//                {
//                    return null; // Trả về null
//                }
//            }
//        }

//        return JsonSerializer.Deserialize<T>(ref reader, options);
//    }

//    public override void Write(Utf8JsonWriter writer, T? value, JsonSerializerOptions options)
//    {
//        if (value == null)
//        {
//            writer.WriteNullValue();
//            return;
//        }

//        // Kiểm tra nếu object rỗng
//        string json = JsonSerializer.Serialize(value, options);
//        if (json == "{}")
//        {
//            writer.WriteNullValue(); // Ghi null thay vì {}
//            return;
//        }

//        JsonSerializer.Serialize(writer, value, options);
//    }
//	}
//}
