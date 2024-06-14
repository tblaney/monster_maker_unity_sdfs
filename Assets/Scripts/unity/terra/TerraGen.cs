using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

namespace snorri
{
    public class TerraGen : Actor 
    {    
        Node[,] terraNodes; 

        Map terraSettingsVars;
        Map terraGenVars;

        Bag<Map> bagOfObjectives;

        protected override void Launch()
        {
            base.Launch();

            terraSettingsVars = Map.FromJson("terra_settings");
            terraGenVars = Map.FromJson("terra_gen");
            
            RunCompute();
            InitTerras();
        }

        Bag<Vector3> GetObjectiveLocations()
        {
            int numObjectives = terraSettingsVars.Get<int>("num_objectives");
            int chunkSize = terraSettingsVars.Get<int>("chunk_size");
            Bag<int> numChunks = terraSettingsVars.Get<Bag<int>>("num_chunks");
            int worldSize = numChunks[0]*chunkSize;

            Bag<Vector3> positionsOfObjectives = new Bag<Vector3>();

            for (int i = 0; i < numObjectives; i++)
            {
                int x = UnityEngine.Random.Range(1, worldSize);
                int z = UnityEngine.Random.Range(1, worldSize);

                positionsOfObjectives.Append(new Vector3(x,0,z));
            }

            return positionsOfObjectives;
        }
        void RunCompute()
        {
            ComputeShader densityShader = RESOURCES.Load<ComputeShader>("shaders/" + terraGenVars.Get<string>("shader_objectives"));
            Bag<Vector3> positionsOfObjectives = GetObjectiveLocations();

            int seed = terraGenVars.Get<int>("seed", 342);
            int seaLevel = terraGenVars.Get<int>("sea_level", 257);

            Map noises = Map.FromJson("noises");

            List<ComputeBuffer> buffersToRelease = new List<ComputeBuffer>();

            TerraNoise[] terraNoises = new TerraNoise[noises.Elements.Count];
            
            int i = 0;
            foreach (string key in noises.Elements.Keys)
            {
                TerraNoise noise = new TerraNoise(noises.Get<Map>(key), seed, i);
                terraNoises[i] = noise;

                i++;
            }

            var objectivesBuffer = new ComputeBuffer(positionsOfObjectives.Length, sizeof(float)*3 + sizeof(float));
            buffersToRelease.Add(objectivesBuffer);

            var noisesBuffer = new ComputeBuffer(terraNoises.Length, sizeof(int) * 6 + sizeof(float) * 5);
            noisesBuffer.SetData(terraNoises);
            buffersToRelease.Add(noisesBuffer);

            var positionsBuffer = new ComputeBuffer(positionsOfObjectives.Length, sizeof(float)*3);
            positionsBuffer.SetData(positionsOfObjectives.All());
            buffersToRelease.Add(positionsBuffer);

            densityShader.SetBuffer(0, "noises", noisesBuffer);
            densityShader.SetBuffer(0, "positions", positionsBuffer);
            densityShader.SetBuffer(0, "objectives", objectivesBuffer);

            densityShader.SetInt("seed", seed);
            densityShader.SetFloat("seaLevel", seaLevel);

            // dispatch shader
            for (int j = 0; j < 1; j++)
            {
                densityShader.Dispatch (j, positionsOfObjectives.Length, 1, 1);
            }

            TerraObjective[] objectivesOut = new TerraObjective[positionsOfObjectives.Length];
            objectivesBuffer.GetData(objectivesOut);

            bagOfObjectives = new Bag<Map>();
            
            // we need to collect the results
            for (int j = 0; j < positionsOfObjectives.Length; j++)
            {
                Map m = new Map();

                TerraObjective objective = objectivesOut[j];

                m.Set<TerraObjective>("objective", objective);
                m.Set<float>("range", 20f);

                LOG.Console(
                    $"new terra objective at {objective.position}, with height: {objective.groundHeight}"
                );

                bagOfObjectives.Append(m);
            }

            if (buffersToRelease != null) {
                foreach (var b in buffersToRelease) {
                    b.Release();
                }
            }
        }

        void InitTerras()
        {
            int numTerrainsPerAxis = terraSettingsVars.Get<int>("num_terrains_per_axis");
            int terrainSize = terraSettingsVars.Get<int>("terrain_size");
            Node[,] terrains = new Node[numTerrainsPerAxis, numTerrainsPerAxis];
            for (int x = 0; x < numTerrainsPerAxis; x++)
            {
                for (int z = 0; z < numTerrainsPerAxis; z++)
                {
                    Vec terrainOrigin = new Vec(
                        terrainSize*x, 0,
                        terrainSize*z
                    );

                    Map args = new Map();
                    args.Set<string>("inherit_from", "terra_chunk");
                    args.Set<Vec>("actors:terra_module:origin", terrainOrigin);
                    args.Set<Bag<Map>>("actors:terra_module:objectives", bagOfObjectives);

                    terrains[x,z] = Node.AddChild($"terra_chunk_{x}_{z}", args);
                }
            }
        }
    }
}