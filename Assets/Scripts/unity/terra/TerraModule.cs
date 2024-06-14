using UnityEngine;
using System.Collections.Generic;
using System.Collections;

namespace snorri
{
    public class TerraModule : Module
    {
        Terrain terrain;
        TerrainCollider collider;      

        Map terraGenVars;
        Map terraSettingsVars;
        Map noises;

        ComputeBuffer cellsBuffer;

        TerraChunk[,] chunks;

        Coroutine routineRefresh;

        ScenePassTerra scenePassTerra;

        Bag<Map> bagOfObjectives;

        protected override void AddClasses()
        {
            terrain = this.gameObject.AddComponent<Terrain>();
            collider = this.gameObject.AddComponent<TerrainCollider>();
        }
        protected override void Setup()
        {
            base.Setup();

            terraGenVars = Map.FromJson("terra_gen");
            terraSettingsVars = Map.FromJson("terra_settings");
            noises = Map.FromJson("noises");

            scenePassTerra = Node.GetActor<ScenePassTerra>();

            TerrainSetup();
        }
        protected override void Launch()
        {
            base.Launch();

            Vec origin = Vars.Get<Vec>("origin");
            Node.Point.Position = origin;

            bagOfObjectives = Vars.Get<Bag<Map>>("objectives");

            Run();
        }
        void TerrainSetup()
        {
            TerrainData terrainData = new TerrainData();
            terrain.terrainData = terrainData;
            collider.terrainData = terrainData;

            int terrainHeight = terraSettingsVars.Get<int>("terrain_height", 400);
            int numPointsPerAxis = terraSettingsVars.Get<int>("num_points_per_axis", 257);
            int chunkResolution = terraSettingsVars.Get<int>("chunk_resolution", 256);
            Bag<int> numChunks = terraSettingsVars.Get<Bag<int>>("num_chunks", new Bag<int>(4,4));
            int terrainSize = terraSettingsVars.Get<int>("terrain_size", 512);

            Material terrainMaterial = RESOURCES.GetMaterial(terraSettingsVars.Get<string>("material"));

            int terrainResolution = (chunkResolution)*numChunks.x; // should be both heightmap and alphamap

            Bag<string> terrainLayerNames = terraSettingsVars.Get<Bag<string>>("terrain_layers", new Bag<string>());
            TerrainLayer[] terrainLayers = new TerrainLayer[terrainLayerNames.Length];
            int i = 0;
            foreach (string terrainLayerName in terrainLayerNames)
            {
                terrainLayers[i] = RESOURCES.Load<TerrainLayer>($"terrain_layers/{terrainLayerName}");
                i++;
            }

            terrain.terrainData.heightmapResolution = terrainResolution;
            terrain.terrainData.alphamapResolution = terrainResolution;
            terrain.terrainData.size = new Vector3(terrainSize, terrainHeight, terrainSize);

            terrain.materialTemplate = terrainMaterial;
            terrain.terrainData.terrainLayers = terrainLayers;
        }
        void CreateBuffers() {
            int numPointsPerAxis = terraSettingsVars.Get<int>("num_points_per_axis", 257);
            int numCells = numPointsPerAxis * numPointsPerAxis;

            ReleaseBuffers();
            
            cellsBuffer = new ComputeBuffer(numCells, sizeof(float)*4 + sizeof(int)*4);
        
        }
        void ReleaseBuffers() {
            if (cellsBuffer != null) {
                cellsBuffer.Release();
            }
        }
        void OnDestroy() {
            if (Application.isPlaying) {
                ReleaseBuffers();
            }
        }


        public void SetHeights(float[,] heights, int xStart = 0, int yStart = 0)
        {
            terrain.terrainData.SetHeights(xStart, yStart, heights);
        }
        public void SetTextures(int[,] textures, int xStart = 0, int yStart = 0)
        {
            int width = textures.GetLength(0);
            int height = textures.GetLength(1);
            int layers = terrain.terrainData.terrainLayers.Length;
            float[,,] alphaMap = new float[width, height, layers];

            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    int dominantLayer = textures[x, y];
                    for (int layer = 0; layer < layers; layer++)
                    {
                        alphaMap[x, y, layer] = (dominantLayer == layer) ? 1 : 0;
                    }
                }
            }

            terrain.terrainData.SetAlphamaps(xStart, yStart, alphaMap);
        }
        public void UpdateChunk(TerraChunk chunk) 
        {
            int numPointsPerAxis = terraSettingsVars.Get<int>("num_points_per_axis", 257);
            int chunkResolution = terraSettingsVars.Get<int>("chunkResolution", 256);
            int chunkSize = terraSettingsVars.Get<int>("chunk_size", 128);
                
            float pointSpacing = (float)chunkSize / (float)(numPointsPerAxis - 1);

            Vector3Int coord = chunk.coord;
            Vector3 origin = coord*chunkSize + transform.position;

            Map args = new Map();
            args.Set<Vector3>("origin", origin);
            args.Set<float>("spacing", pointSpacing);

            args.Set<bool>("is_edge_chunk", chunk.isEdgeChunk);

            RunCompute(args);

            // unpack cells buffer and assign to terrain
            TerraCell[] cells = new TerraCell[numPointsPerAxis * numPointsPerAxis];
            cellsBuffer.GetData(cells);

            chunk.SetCells(cells);
        }
        void RunCompute(Map args)
        {
            ComputeShader densityShader = RESOURCES.Load<ComputeShader>("shaders/" + terraGenVars.Get<string>("shader"));
            int numPointsPerAxis = terraSettingsVars.Get<int>("num_points_per_axis", 257);
            int chunkResolution = terraSettingsVars.Get<int>("chunkResolution", 256);
            int seed = terraGenVars.Get<int>("seed", 342);
            int seaLevel = terraGenVars.Get<int>("sea_level", 257);

            int threadGroupSize = terraSettingsVars.Get<int>("thread_group_size");

            List<ComputeBuffer> buffersToRelease = new List<ComputeBuffer>();

            TerraNoise[] terraNoises = new TerraNoise[noises.Elements.Count];
            
            int i = 0;
            foreach (string key in noises.Elements.Keys)
            {
                TerraNoise noise = new TerraNoise(noises.Get<Map>(key), seed, i);
                terraNoises[i] = noise;

                i++;
            }

            var noisesBuffer = new ComputeBuffer(terraNoises.Length, sizeof(int) * 6 + sizeof(float) * 5);
            noisesBuffer.SetData(terraNoises);
            buffersToRelease.Add(noisesBuffer);

            densityShader.SetBuffer(0, "noises", noisesBuffer);

            densityShader.SetInt("seed", seed);
            densityShader.SetFloat("seaLevel", seaLevel);

            int chunkSize = terraSettingsVars.Get<int>("chunk_size");
            Vector3 origin = args.Get<Vector3>("origin");
            float spacing = args.Get<float>("spacing");

            int numThreadsPerAxis = Mathf.CeilToInt (numPointsPerAxis / (float) threadGroupSize);

            densityShader.SetBuffer (0, "cells", cellsBuffer);
            densityShader.SetFloat ("numPointsPerAxis", numPointsPerAxis);
            densityShader.SetFloat ("chunkSize", chunkSize);
            densityShader.SetVector ("origin", origin);
            densityShader.SetFloat ("spacing", spacing);

            // Dispatch shader
            for (int j = 0; j < 1; j++)
            {
                densityShader.Dispatch (j, numThreadsPerAxis, 1, numThreadsPerAxis);
            }

            if (buffersToRelease != null) {
                foreach (var b in buffersToRelease) {
                    b.Release();
                }
            }
        }



        public void Run() {
            if (routineRefresh == null)
            {
                routineRefresh = StartCoroutine(RoutineRefreshTerrain());
            }
        }
        void ProcessObjective(Map objectiveMap)
        {
            int numPointsPerAxis = terraSettingsVars.Get<int>("num_points_per_axis", 257);
            int chunkResolution = terraSettingsVars.Get<int>("chunkResolution", 256);
            int chunkSize = terraSettingsVars.Get<int>("chunk_size", 128);
            Bag<int> numChunks = terraSettingsVars.Get<Bag<int>>("num_chunks", new Bag<int>());
            int terraHeight = terraSettingsVars.Get<int>("terrain_height");

            int worldTerraSize = numChunks[0]*chunkSize;

            float pointSpacing = (float)chunkResolution / (float)(chunkSize);

            Vec minTerra = Node.Point.Position;
            minTerra.y = 0f;

            TerraObjective objective = objectiveMap.Get<TerraObjective>("objective");
            float range = objectiveMap.Get<float>("range");

            int bufferZone = 15;

            VecInt bufferZoneMin = new VecInt(bufferZone, bufferZone);
            VecInt bufferZoneMax = new VecInt(bufferZone, bufferZone);

            Vec minObjective = (new Vec(objective.position)).Subtract(new Vec(range+bufferZoneMin.x,0,range+bufferZoneMin.y));
            Vec maxObjective = minObjective.Add(new Vec(range*2+1+bufferZoneMax.x,0,range*2+1+bufferZoneMax.y));

            Vec minLocalObjective = minObjective.Subtract(minTerra);
            Vec maxLocalObjective = maxObjective.Subtract(minTerra);

            if (minLocalObjective.x >= worldTerraSize || minLocalObjective.z >= worldTerraSize)
                return;

            if (maxLocalObjective.x <= 0 || maxLocalObjective.z <= 0)
                return;
            
            if (minLocalObjective.x < 0)
            {
                float difference = Mathf.Abs(minLocalObjective.x);
                if (difference >= bufferZoneMin.x)
                {
                    bufferZoneMin.x = 0;
                } else
                {
                    bufferZoneMin.x = bufferZoneMin.x - (int)difference;
                }
                minLocalObjective.x = 0;
            }

            if (minLocalObjective.z < 0)
            {
                float difference = Mathf.Abs(minLocalObjective.z);
                if (difference >= bufferZoneMin.y)
                {
                    bufferZoneMin.y = 0;
                } else
                {
                    bufferZoneMin.y = bufferZoneMin.y - (int)difference;
                }
                minLocalObjective.z = 0;
            }

            if (maxLocalObjective.x >= worldTerraSize)
            {
                float difference = maxLocalObjective.x - worldTerraSize;
                if (difference >= bufferZoneMax.x)
                {
                    bufferZoneMax.x = 0;
                } else
                {
                    bufferZoneMax.x = bufferZoneMax.x - (int)difference;
                }
                maxLocalObjective.x = worldTerraSize;
            }

            if (maxLocalObjective.z >= worldTerraSize)
            {
                float difference = maxLocalObjective.z - worldTerraSize;
                if (difference >= bufferZoneMax.y)
                {
                    bufferZoneMax.y = 0;
                } else
                {
                    bufferZoneMax.y = bufferZoneMax.y - (int)difference;
                }
                maxLocalObjective.z = worldTerraSize;
            }

            LOG.Console($"processing objective, buffers: {bufferZoneMin.vec2}, {bufferZoneMax.vec2}");

            Vec minHeightObjective = minLocalObjective.Multiply(pointSpacing);
            Vec maxHeightObjective = maxLocalObjective.Multiply(pointSpacing);

            float[,] heights = new float[(int)(maxHeightObjective.z-minHeightObjective.z), (int)(maxHeightObjective.x-minHeightObjective.x)];
            float[,] heightsCurrent = terrain.terrainData.GetHeights((int)minHeightObjective.x, (int)minHeightObjective.z, heights.GetLength(1), heights.GetLength(0));

            LOG.Console($"processing objective, local height bounds: {minHeightObjective.vec3}, {maxHeightObjective.vec3}");
            LOG.Console($"processing objective, heights size: {heights.GetLength(0)}, {heights.GetLength(1)}, with current heights size: {heightsCurrent.GetLength(0)}, {heightsCurrent.GetLength(1)}");
            float newHeight = objective.groundHeight / (float)terraHeight;
            for (int x = 0; x < heights.GetLength(1); x++)
            {
                for (int z = 0; z < heights.GetLength(0); z++)
                {
                    float xLerpFactor = 1.0f;
                    float zLerpFactor = 1.0f;

                    if (x < bufferZoneMin.x)
                        xLerpFactor = (float)x / bufferZoneMin.x;
                    else if (x >= heights.GetLength(1) - bufferZoneMax.x)
                        xLerpFactor = (float)(heights.GetLength(1) - x) / bufferZoneMax.x;

                    if (z < bufferZoneMin.y)
                        zLerpFactor = (float)z / bufferZoneMin.y;
                    else if (z >= heights.GetLength(0) - bufferZoneMax.y)
                        zLerpFactor = (float)(heights.GetLength(0) - z) / bufferZoneMax.y;

                    float modifiedNewHeight = Mathf.Lerp(heightsCurrent[z, x], newHeight, Mathf.Min(xLerpFactor, zLerpFactor));
                    heights[z, x] = modifiedNewHeight;
                }
            }

            SetHeights(heights, (int)minHeightObjective.x, (int)minHeightObjective.z);
        }
        private IEnumerator RoutineRefreshTerrain()
        {
            CreateBuffers();
            int i = 0;
            
            Bag<int> numChunks = terraSettingsVars.Get<Bag<int>>("num_chunks", new Bag<int>());
            chunks = new TerraChunk[numChunks[0], numChunks[1]];

            for (int x = 0; x < numChunks[0]; x++)
            {
                for (int z = 0; z < numChunks[1]; z++)
                {
                    Map args = new Map();
                    args.Set<Map>("settings", terraSettingsVars);
                    args.Set<Vector3Int>("coord", new Vector3Int(x, 0, z));

                    bool isEdgeChunk = false;
                    if (x == numChunks[0] - 1 || z == numChunks[1] - 1)
                        isEdgeChunk = true;

                    args.Set<bool>("is_edge_chunk", isEdgeChunk);

                    TerraChunk chunk = new TerraChunk(this, args);

                    chunks[x,z] = chunk;

                    UpdateChunk(chunk);

                    i++;

                    if (i == 2)
                    {
                        i = 0;
                        yield return null;
                    }

                    LOG.Console(
                        $"env generator refresher: {x}, {z}"
                    );
                }
            }

            terrain.terrainData.SyncHeightmap();

            yield return null;

            foreach (Map m in bagOfObjectives)
            {
                ProcessObjective(m);

                TerraObjective objective = m.Get<TerraObjective>("objective");
                LOG.Console(
                    $"terra objective processed at {objective.position}, with height: {objective.groundHeight}"
                );

                yield return null;
            }

            scenePassTerra.IsComplete = true;
        }
    }
}