using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Reflection;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace snorri
{
    public class MapJsonConverter : JsonConverter
    {
        public override bool CanConvert(Type objectType)
        {
            return true;
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            JObject jsonObject = JObject.Load(reader);
            return ParseObject(jsonObject);
        }

        private Map ParseObject(JObject jObject, string mapName = "")
        {
            var map = new Map();

            if (mapName != "")
                map.Name = mapName;

            foreach (var prop in jObject.Properties())
            {
                var value = prop.Value;

                if (value.Type == JTokenType.Object)
                {
                    Map nestedMap = ParseObject(value as JObject, prop.Name);
                    map.Set<Map>(prop.Name, nestedMap);
                }
                else
                {
                    AddAttribute(map, prop.Name, value);
                }
            }

            return map;
        }

        private void AddAttribute(Map map, string name, JToken value)
        {
            switch (value.Type)
            {
                case JTokenType.String:
                    map.Set<string>(name, value.ToObject<string>());
                    break;
                case JTokenType.Float:
                    map.Set<float>(name, value.ToObject<float>());
                    break;
                case JTokenType.Integer:
                    map.Set<int>(name, value.ToObject<int>());
                    break;
                case JTokenType.Boolean:
                    map.Set<bool>(name, value.ToObject<bool>());
                    break;
                case JTokenType.Object:
                    Map nestedMap = ParseObject(value as JObject, name);
                    map.Set<Map>(name, nestedMap);
                    break;
                case JTokenType.Array:
                    HandleArray(map, name, value as JArray);
                    break;
                default:
                    // LOG.ConsoleWarning($"Unsupported JSON token type: {value.Type} for key: {name}");
                    break;
            }
        }

        private void HandleArray(Map map, string name, JArray array)
        {
            if (array.Count == 0)
            {
                return;
            }

            switch (array[0].Type)
            {
                case JTokenType.String:
                    Bag<string> listString = new Bag<string>();
                    foreach (var item in array)
                    {
                        listString.Append(item.ToObject<string>(), false);
                    }
                    map.Set<Bag<string>>(name, listString);
                    break;
                case JTokenType.Float:
                    Bag<float> listFloat = new Bag<float>();
                    foreach (var item in array)
                    {
                        listFloat.Append(item.ToObject<float>(), false);
                    }
                    map.Set<Bag<float>>(name, listFloat);
                    break;
                case JTokenType.Integer:
                    Bag<int> listInt = new Bag<int>();
                    foreach (var item in array)
                    {
                        listInt.Append(item.ToObject<int>(), false);
                    }
                    map.Set<Bag<int>>(name, listInt);
                    break;
                case JTokenType.Boolean:
                    Bag<bool> listBool = new Bag<bool>();
                    foreach (var item in array)
                    {
                        listBool.Append(item.ToObject<bool>(), false);
                    }
                    map.Set<Bag<bool>>(name, listBool);
                    break;
                case JTokenType.Object:
                    Bag<Map> listMap = new Bag<Map>();
                    foreach (var item in array)
                    {
                        Map nestedMap = ParseObject(item as JObject);
                        listMap.Append(nestedMap);
                    }
                    map.Set<Bag<Map>>(name, listMap);
                    break;
            }
        }

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {            
            Map map = value as Map;
            if (map == null)
                return;

            Dictionary<string, object> dictionary = MapToDictionary(map);

            WriteDictionary(writer, dictionary, serializer);
        }

        public void WriteDictionary(JsonWriter writer, 
            Dictionary<string, object> dictionary, 
            JsonSerializer serializer)
        {
            writer.WriteStartObject(); // Begin the object

            foreach (KeyValuePair<string, object> kvp in dictionary)
            {
                writer.WritePropertyName(kvp.Key); // Write the key

                // Write the value, checking for common types to handle them appropriately
                if (kvp.Value is null)
                {
                    writer.WriteNull();
                }
                else if (kvp.Value is string stringValue)
                {
                    writer.WriteValue(stringValue);
                }
                else if (kvp.Value is int intValue)
                {
                    writer.WriteValue(intValue);
                }
                else if (kvp.Value is float floatValue)
                {
                    writer.WriteValue(floatValue);
                }
                else if (kvp.Value is bool boolValue)
                {
                    writer.WriteValue(boolValue);
                }
                else if (kvp.Value is Dictionary<string, object> dictValue)
                {
                    WriteDictionary(writer, dictValue, serializer);
                }
                else if (kvp.Value is int[] intArray)
                {
                    writer.WriteStartArray(); // Begin the array

                    foreach (int intArrayValue in intArray)
                    {
                        writer.WriteValue(intArrayValue); // Write each integer value in the array
                    }

                    writer.WriteEndArray(); // End the array
                }
                else if (kvp.Value is float[] floatArray)
                {
                    writer.WriteStartArray(); // Begin the array

                    foreach (float floatArrayValue in floatArray)
                    {
                        writer.WriteValue(floatArrayValue); // Write each integer value in the array
                    }

                    writer.WriteEndArray(); // End the array
                }
                else if (kvp.Value is bool[] boolArray)
                {
                    writer.WriteStartArray(); // Begin the array

                    foreach (bool boolArrayValue in boolArray)
                    {
                        writer.WriteValue(boolArrayValue); // Write each integer value in the array
                    }

                    writer.WriteEndArray(); // End the array
                }
                else if (kvp.Value is string[] stringArray)
                {
                    writer.WriteStartArray(); // Begin the array

                    foreach (string stringArrayValue in stringArray)
                    {
                        writer.WriteValue(stringArrayValue); // Write each integer value in the array
                    }

                    writer.WriteEndArray(); // End the array
                }
                else if (kvp.Value is Dictionary<string, object>[] dictArray)
                {
                    writer.WriteStartArray(); // Begin the array

                    foreach ( Dictionary<string, object> dictArrayValue in dictArray)
                    {
                        WriteDictionary(writer, dictArrayValue, serializer);
                    }

                    writer.WriteEndArray(); // End the array
                }
            }

            writer.WriteEndObject(); // End the object
        }

        private Dictionary<string, object> MapToDictionary(Map map)
        {
            var result = new Dictionary<string, object>();

            if (map.Elements != null)
            {
                foreach (string elementKey in map.Elements.Keys)
                {
                    object element = map.Elements[elementKey];

                    if (element is string stringAttr)
                    {
                        result[elementKey] = stringAttr;
                    }
                    else if (element is float floatAttr)
                    {
                        result[elementKey] = floatAttr;
                    }
                    else if (element is int intAttr)
                    {
                        result[elementKey] = intAttr;
                    }
                    else if (element is bool boolAttr)
                    {
                        result[elementKey] = boolAttr;
                    } 
                    else if (element is Node nodeAttr)
                    {
                        result[elementKey] = MapToDictionary(nodeAttr.Vars);
                    }
                    else if (element is Map nestedMap)
                    {
                        result[elementKey] = MapToDictionary(nestedMap);
                    } else if (element is Bag<int> stackIntAttr)
                    {
                        result[elementKey] = stackIntAttr.All();
                    } else if (element is Bag<float> stackFloatAttr)
                    {
                        result[elementKey] = stackFloatAttr.All();
                    } else if (element is Bag<string> stackStringAttr)
                    {
                        result[elementKey] = stackStringAttr.All();
                    }  else if (element is Bag<bool> stackBoolAttr)
                    {
                        result[elementKey] = stackBoolAttr.All(); 
                    }  else if (element is Bag<Map> stackMapAttr)
                    {
                        Dictionary<string, object>[] dictArray = new Dictionary<string, object>[stackMapAttr.Length];
                        int i = 0;
                        foreach (Map m in stackMapAttr)
                        {
                            Dictionary<string, object> dictMaps = MapToDictionary(m);
                            dictArray[i] = dictMaps;

                            i++;
                        }
                        result[elementKey] = dictArray;
                    }
                }
            }
            return result;
        }
    }
}