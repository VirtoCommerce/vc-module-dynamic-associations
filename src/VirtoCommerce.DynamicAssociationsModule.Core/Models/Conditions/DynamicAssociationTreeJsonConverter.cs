using System;
using System.Linq;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using VirtoCommerce.Platform.Core.Common;

namespace VirtoCommerce.DynamicAssociationsModule.Core.Model.Conditions
{
    public class DynamicAssociationTreeJsonConverter : JsonConverter
    {
        private readonly bool _doNotSerializeAvailCondition;
        public DynamicAssociationTreeJsonConverter(bool doNotSerializeAvailCondition = false)
        {
            _doNotSerializeAvailCondition = doNotSerializeAvailCondition;
        }
        public override bool CanWrite
        {
            get
            {
                return _doNotSerializeAvailCondition;
            }
        }
        public override bool CanRead { get; } = true;

        public override bool CanConvert(Type objectType)
        {
            return typeof(IDynamicAssociationTree).IsAssignableFrom(objectType);
        }

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            //need to remove "AvailableChildren" from resulting tree Json to reduce size
            if (value != null)
            {
                var result = JObject.FromObject(value);
                if (result != null)
                {
                    result.Remove(nameof(IDynamicAssociationTree.AvailableChildren));
                    result.WriteTo(writer, this);
                }
            }
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            var obj = JObject.Load(reader);
            var type = objectType.Name;
            var pt = obj.GetValue("Id", StringComparison.InvariantCultureIgnoreCase);
            if (pt != null)
            {
                type = pt.Value<string>();
            }
            object result;

            if (AbstractTypeFactory<IDynamicAssociationTree>.FindTypeInfoByName(type) != null)
            {
                result = AbstractTypeFactory<IDynamicAssociationTree>.TryCreateInstance(type);
            }
            else
            {
                var tryCreateInstance = typeof(AbstractTypeFactory<>).MakeGenericType(objectType).GetMethods().FirstOrDefault(x => x.Name.EqualsInvariant("TryCreateInstance") && x.GetParameters().Length == 0);
                result = tryCreateInstance?.Invoke(null, null);
            }

            if (result == null)
            {
                throw new NotSupportedException("Unknown type: " + type);
            }

            serializer.Populate(obj.CreateReader(), result);
            return result;
        }

    }
}
