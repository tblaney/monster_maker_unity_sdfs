using System;
using UnityEngine;
using System.IO;
using Newtonsoft.Json;


namespace snorri
{
    public static class RESOURCES
    {
        public static T Load<T>(string path) where T : UnityEngine.Object
        {
            T obj = UnityEngine.Resources.Load<T>(path);
            return obj;
        }

        public static string FilePathJson = "json/";

        private static string FilePathTextures = "textures/";
        private static string FilePathPrefabs = "prefabs/";
        public static string FilePathMaterials = "materials/";


        public static Texture2D GetTexture(string name, string typeName = "")
        {
            string texName = FilePathTextures + name;
            if (typeName != "")
                texName = FilePathTextures + $"{typeName}/" + name;

            Texture2D tex = Load<Texture2D>(texName);

            return tex;
        }
        public static Material GetMaterial(string name)
        {
            Material mat = Load<Material>(FilePathMaterials + name);
            return mat;
        }
        public static GameObject GetGameObject(string name)
        {
            GameObject obj = Load<GameObject>(FilePathPrefabs + name);
            return obj;
        }


        public static void SaveTextureAsPNG(Texture2D texture, string fileName)
        {
            byte[] bytes = texture.EncodeToPNG();

            string path = Path.Combine(Application.persistentDataPath, fileName);

            File.WriteAllBytes(path, bytes);

            Debug.Log($"texture saved as PNG to {path}");
        }
    }
}