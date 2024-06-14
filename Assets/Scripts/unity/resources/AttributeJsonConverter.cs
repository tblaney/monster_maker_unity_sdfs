using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Reflection;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace snorri
{
    public class AttributeJsonConverter : JsonConverter
    {
        public override bool CanConvert(Type objectType)
        {
            // LOG.Console("attribute json converter can convert? " + objectType.FullName);

            return objectType == typeof(Element);

            // Check if the type is a generic type and if the generic type definition is Attribute<>
            // return objectType.IsGenericType && objectType.GetGenericTypeDefinition() == typeof(Attribute<>);
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            JObject jsonObject = JObject.Load(reader);
            

            // this is for all Element types now
            string typeDescription = (string)jsonObject["$type"];

            LOG.Console("attribute json converter read json! " + typeDescription + ", default type: " + objectType.FullName);

            if (typeDescription != null && typeDescription != "")
            {
                if (typeDescription.Contains("Attribute"))
                {
                    // do attribute processing
                    Type valueType = ExtractValueType(typeDescription);

                    LOG.Console("attribute json converter set string: " + typeof(Bag<>).FullName);

                    Type specificAttributeType = typeof(Attribute<>).MakeGenericType(valueType);
                    object attributeInstance = Activator.CreateInstance(specificAttributeType);

                    //LOG.Console("attribute json converter attribute type: " + specificAttributeType.GetProperties()[0].ToString());
                    foreach (FieldInfo info in specificAttributeType.GetFields())
                    {
                        LOG.Console("attribute json converter property in class: " + info.ToString());
                    }

                    FieldInfo valProperty = specificAttributeType.GetField("_val");
                    if (jsonObject["_val"] != null)
                    {
                        object val = jsonObject["_val"].ToObject(valueType, serializer);
                        valProperty.SetValue(attributeInstance, val);
                    }
                    FieldInfo nameProperty = specificAttributeType.GetField("_name");
                    if (jsonObject["_name"] != null)
                    {
                        string name = jsonObject["_name"].ToObject<string>();
                        nameProperty.SetValue(attributeInstance, name);
                    }

                    return attributeInstance;
                } else
                {
                    JToken token = JToken.Load(reader);
                    return token.ToObject(objectType, serializer);
                }
            } else
            {
                JToken token = JToken.Load(reader);
                return token.ToObject(objectType, serializer);
            }

            // LOG.Console("attribute json converter read json!");

            // Get the value type of the Attribute<T> (T in Attribute<T>)
            // Type valueType = objectType.GetGenericArguments()[0];
        }

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            // Serialize the Attribute<T> instance using the default serializer logic
            // This ensures that the rest of the object graph is handled normally
            var t = JToken.FromObject(value, serializer);
            t.WriteTo(writer);
        }   

        
        private Type ExtractValueType(string typeDescription)
        {
            var typeArgsStart = typeDescription.IndexOf("[[") + 2;
            var typeArgsEnd = typeDescription.LastIndexOf("]]");
            if (typeArgsStart > 2 && typeArgsEnd > typeArgsStart)
            {
                string innerTypeDescription = typeDescription.Substring(typeArgsStart, typeArgsEnd - typeArgsStart);

                // Handle generic type arguments
                if (innerTypeDescription.Contains("`"))
                {
                    // It's a generic type, so we need to construct it
                    var genericArgsStart = innerTypeDescription.IndexOf("[[") + 2;
                    var genericArgsEnd = innerTypeDescription.LastIndexOf("]]");
                    string genericTypeName = innerTypeDescription.Substring(0, genericArgsStart - 2);
                    string genericArgTypeName = innerTypeDescription.Substring(genericArgsStart, genericArgsEnd - genericArgsStart);
                    if (genericTypeName.Contains('2'))
                    {
                        // Dictionary
                        string[] generaticArgsNew = new string[2];
                        generaticArgsNew[0] = genericArgTypeName.Split("]")[0];
                        string[] splits2 = genericArgTypeName.Split("[");
                        generaticArgsNew[1] = splits2[splits2.Length - 1];

                        Type genericType = Type.GetType(genericTypeName, true);

                        Type[] genericArgs = { Type.GetType(generaticArgsNew[0], true), Type.GetType(generaticArgsNew[1], true) };


                        return genericType.MakeGenericType(genericArgs);

                    } else
                    {
                        LOG.Console("attribute json converter inner generic: " + genericTypeName + ", " + genericArgTypeName);

                        // Get the generic type
                        Type genericType = Type.GetType(genericTypeName, true);

                        // Get the generic argument type(s)
                        Type[] genericArgs = { Type.GetType(genericArgTypeName, true) };

                        // Construct and return the generic type
                        return genericType.MakeGenericType(genericArgs);
                    }
                }
                else
                {
                    // It's a non-generic type, just return it
                    return Type.GetType(innerTypeDescription, true);
                }
            }

            // Fallback to a default type if necessary
            return typeof(object);
        }

        public override bool CanWrite => true;
    }
}