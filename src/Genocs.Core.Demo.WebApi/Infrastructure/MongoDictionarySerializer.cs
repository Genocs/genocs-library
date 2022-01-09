using MongoDB.Bson;
using MongoDB.Bson.Serialization;
using MongoDB.Bson.Serialization.Serializers;
using Newtonsoft.Json;
using System.Collections.Generic;

namespace Genocs.Core.Demo.WebApi.Infrastructure
{
    public class MongoDictionarySerializer : SerializerBase<IDictionary<string, object>>
    {
        public override Dictionary<string, object> Deserialize(BsonDeserializationContext context, BsonDeserializationArgs args)
        {

            var serializer = BsonSerializer.LookupSerializer(typeof(BsonDocument));

            var document = serializer.Deserialize(context, args);

            var bsonDocument = document.ToBsonDocument();

            var result = BsonExtensionMethods.ToJson(bsonDocument);

            return JsonConvert.DeserializeObject<Dictionary<string, object>>(result);
        }


        public override void Serialize(BsonSerializationContext context, BsonSerializationArgs args, IDictionary<string, object> value)
        {

            var jsonDocument = JsonConvert.SerializeObject(value);

            var bsonDocument = BsonSerializer.Deserialize<BsonDocument>(jsonDocument);

            var serializer = BsonSerializer.LookupSerializer(typeof(BsonDocument));

            serializer.Serialize(context, bsonDocument.AsBsonValue);
        }
    }
}
