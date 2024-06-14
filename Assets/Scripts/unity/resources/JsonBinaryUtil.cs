using UnityEngine;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using System;

namespace snorri
{
    [ExecuteInEditMode]
    public class JsonBinaryUtil : MonoBehaviour
    {
        public bool isExecute;  
        public enum State
        {
            ToBin,
            ToJson
        }
        public State currentState;
        public string binaryFilePath;

        void Update()
        {
            if (isExecute)
            {
                if (currentState == State.ToBin)
                {
                    ConvertJsonToBinary("Assets/Resources/json/meshes");
                } else
                {
                    string jsonString = DeserializeBinaryToJson();
                    LOG.Console(jsonString);
                }

                isExecute = false;
            }
        }

        void ConvertJsonToBinary(string folderPath)
        {
            // Check if the directory exists
            if (!Directory.Exists(folderPath))
            {
                LOG.Console("Directory does not exist: " + folderPath);
                return;
            }

            // Get all JSON files in the directory
            string[] jsonFiles = Directory.GetFiles(folderPath, "*.json");
            foreach (string jsonFilePath in jsonFiles)
            {
                string jsonContent = File.ReadAllText(jsonFilePath);
                string binaryFilePath = Path.ChangeExtension(jsonFilePath, ".bin");

                // Convert JSON content to binary format and save
                BinaryFormatter formatter = new BinaryFormatter();
                FileStream fileStream = new FileStream(binaryFilePath, FileMode.Create);
                formatter.Serialize(fileStream, jsonContent);
                fileStream.Close();

                // Delete the original JSON file
                File.Delete(jsonFilePath);

                LOG.Console("Converted and deleted: " + jsonFilePath);
            }
        }

        public string DeserializeBinaryToJson()
        {
            // Check if the file exists
            if (!File.Exists(binaryFilePath))
            {
                LOG.Console("File does not exist: " + binaryFilePath);
                return null;
            }

            try
            {
                // Open the binary file
                using (FileStream fileStream = new FileStream(binaryFilePath, FileMode.Open))
                {
                    // Create a BinaryFormatter to deserialize the binary data
                    BinaryFormatter formatter = new BinaryFormatter();
                    
                    // Deserialize the binary data back into a JSON string
                    string jsonString = (string)formatter.Deserialize(fileStream);
                    
                    return jsonString;
                }
            }
            catch (Exception ex)
            {
                LOG.Console("Error during deserialization: " + ex.Message);
                return null;
            }
        }
    }

}
