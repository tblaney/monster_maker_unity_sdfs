using UnityEngine;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using System;
using System.IO.Compression;
using System.Text;


namespace snorri
{
    [ExecuteInEditMode]
    public class JsonCompressUtil : MonoBehaviour
    {
        public bool isExecute;  
        public enum State
        {
            ToCompress,
            ToJson
        }
        public State currentState;
        public string compressionFilePath;

        void Update()
        {
            if (isExecute)
            {
                if (currentState == State.ToCompress)
                {
                    CompressJson("Assets/Resources/json/meshes");
                } else
                {
                    string jsonString = DeserializeCompression();
                    LOG.Console(jsonString);
                }

                isExecute = false;
            }
        }

        void CompressJson(string folderPath)
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
                string compressedFilePath = jsonFilePath + ".gz";

                // Open the original JSON file for reading
                using (FileStream originalFileStream = new FileStream(jsonFilePath, FileMode.Open))
                {
                    // Create a FileStream for the compressed file
                    using (FileStream compressedFileStream = File.Create(compressedFilePath))
                    {
                        // Create a GZipStream for compression
                        using (GZipStream compressionStream = new GZipStream(compressedFileStream, CompressionMode.Compress))
                        {
                            // Copy the JSON data to the compression stream
                            originalFileStream.CopyTo(compressionStream);
                        }
                    }
                }

                LOG.Console("Compressed and saved: " + compressedFilePath);

                // Optional: Delete the original JSON file
                File.Delete(jsonFilePath);
            }
        }

        public string DeserializeCompression()
        {
            // Ensure the compressed file exists
            if (!File.Exists(compressionFilePath))
            {
                LOG.Console("Compressed file does not exist: " + compressionFilePath);
                return null;
            }

            // Initialize a StringBuilder to hold the decompressed data
            StringBuilder jsonContent = new StringBuilder();

            // Open the compressed file
            using (FileStream compressedFileStream = new FileStream(compressionFilePath, FileMode.Open))
            {
                // Create a GZipStream for decompression
                using (GZipStream decompressionStream = new GZipStream(compressedFileStream, CompressionMode.Decompress))
                {
                    // Use a StreamReader to read the decompressed data
                    using (StreamReader reader = new StreamReader(decompressionStream))
                    {
                        string line;
                        while ((line = reader.ReadLine()) != null)
                        {
                            jsonContent.AppendLine(line);
                        }
                    }
                }
            }

            LOG.Console("Decompressed JSON content retrieved successfully.");

            // Return the decompressed JSON string
            return jsonContent.ToString();
        }
    }
}