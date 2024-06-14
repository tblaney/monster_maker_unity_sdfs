using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine;

namespace snorri
{
    public static class UTIL
    {
        public static Type GetType(string typeName)
        {
            Type componentType = Type.GetType(typeName);
            return componentType;
        }
        public static Type GetSnorriType(string jsonFormat)
        {
            string typeString = ConvertSnorriStringToUnity(jsonFormat);
            if (!typeString.Contains("snorri."))
            {
                typeString = "snorri." + typeString;                
            }
            Type componentType = Type.GetType(typeString);
            return componentType;
        }
        public static string ConvertStringToMethod(string typeString)
        {
            string[] words = typeString.Split("_");
            for (int i = 0; i < words.Length; i++)
            {
                if (!string.IsNullOrEmpty(words[i]))
                {
                    words[i] = words[i].Substring(0, 1).ToUpper() + words[i].Substring(1).ToLower();
                }
            }
            return string.Concat(words);
        }
        public static string ConvertSnorriStringToUnity(string typeString)
        {
            string[] words = typeString.Split("_");
            for (int i = 0; i < words.Length; i++)
            {
                if (!string.IsNullOrEmpty(words[i]))
                {
                    words[i] = words[i].Substring(0, 1).ToUpper() + words[i].Substring(1).ToLower();
                }
            }
            return string.Concat(words);
        }
        public static object CreateInstance(Type type)
        {
            try
            {
                return Activator.CreateInstance(type);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error creating instance of {type.Name} using default constructor: {ex.Message}");
                return null;
            }
        }
        public static object CreateInstanceWithConstructor(Type type, Map parameters)
        {
            if (parameters.Length == 0)
            {
                try
                {
                    return Activator.CreateInstance(type);
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error creating instance of {type.Name} using default constructor: {ex.Message}");
                    return null;
                }
            }

            var constructors = type.GetConstructors();

            foreach (var constructor in constructors)
            {
                var constructorParams = constructor.GetParameters();
                var args = new object[constructorParams.Length];

                int i = 0;
                bool allParamsMatch = true;

                foreach (var param in constructorParams)
                {
                    if (parameters.Has(param.Name) && 
                        TryConvertParameter(parameters.GetObject(param.Name), param.ParameterType, out args[i]))
                    {
                        i++;
                    }
                    else
                    {
                        allParamsMatch = false;
                        break;
                    }
                }

                if (allParamsMatch)
                {
                    return constructor.Invoke(args);
                }
            }

            return null;
        }

        private static bool TryConvertParameter(object input, Type targetType, out object result)
        {
            try
            {
                if (targetType.IsAssignableFrom(input.GetType()))
                {
                    result = input;
                    return true;
                }

                result = Convert.ChangeType(input, targetType);
                return true;
            }
            catch
            {
                result = null;
                return false;
            }
        }

        public static void StringToPosition(string input, out int x, out int y)
        {
            // Split the string based on the '.' delimiter
            string[] parts = input.Split('.');

            // Parse the first part as x
            x = int.Parse(parts[0]);

            // Parse the second part as y, if it exists
            if (parts.Length > 1)
            {
                y = int.Parse(parts[1]);
            }
            else
            {
                // If there's no second part, set y to 0 or some default value
                y = 0;
            }
        }
        public static string PositionToString(int x, int y)
        {
            // Using string interpolation to concatenate x and y into the desired format
            return $"{x}.{y}";
        }

        public static float GetVelocity(float initial, float time)
        {
            return initial + time*UnityEngine.Physics.gravity.y;
        }

        public static float Sqrt(float val)
        {
            return Mathf.Sqrt(val);
        }

        public static Color GetColorFromHex(string colorHex)
        {
            Color colorHexU;
            if (ColorUtility.TryParseHtmlString(colorHex, out colorHexU))
            {
                return colorHexU;
            }
            return Color.black;
        }

        public static float CalculateSineY(float x, float x1, float x2, float h)
        {
            // Normalize x so that the wave fits between x1 and x2
            float normalizedX = (x - x1) / (x2 - x1);

            // Calculate the sine value for this normalized x
            // The sine function normally has a range from -1 to 1 and a period of 2*PI
            // Multiplying the angle by PI to make the wave fit exactly once between x1 and x2
            float sineValue = Mathf.Sin(normalizedX * Mathf.PI);

            // Adjust the height by multiplying the sine value by h/2 (since sine ranges from -1 to 1, making the total height h)
            return h / 2 * sineValue;
        }

        public static Vector3 GetRandomVector(Vector3 minValues, Vector3 maxValues)
        {
            float randomX = UnityEngine.Random.Range(minValues.x, maxValues.x);
            float randomY = UnityEngine.Random.Range(minValues.y, maxValues.y);
            float randomZ = UnityEngine.Random.Range(minValues.z, maxValues.z);
            return new Vector3(randomX, randomY, randomZ);
        }
    }
}