using UnityEngine;
namespace snorri
{
    public class TerraChunk
    {
        Map terraSettingsVars;

        public Vector3Int coord;
        TerraModule terra;

        public bool isEdgeChunk = false;

        public TerraChunk(TerraModule terra, Map args)
        {
            this.terra = terra;

            coord = args.Get<Vector3Int>("coord");
            terraSettingsVars = args.Get<Map>("settings");

            isEdgeChunk = args.Get<bool>("is_edge_chunk", false);
        }

        public void SetCells(TerraCell[] cells)
        {
            int numPointsPerAxis = terraSettingsVars.Get<int>("num_points_per_axis");
            int chunkResolution = terraSettingsVars.Get<int>("chunk_resolution");

            if (isEdgeChunk)
                chunkResolution += 1;

            float[,] heights = new float[chunkResolution, chunkResolution];
            int[,] textures = new int[chunkResolution, chunkResolution];

            int terraHeight = terraSettingsVars.Get<int>("terrain_height");

            foreach (TerraCell cell in cells)
            {
                int x = cell.x;
                int z = cell.z;

                if (x >= heights.GetLength(0) || z >= heights.GetLength(1))
                    continue;

                heights[z,x] = cell.groundHeight/(float)terraHeight;
                textures[z,x] = cell.colorId;
            }

            int coordDimension = chunkResolution;
            if (isEdgeChunk)
            {
                coordDimension -= 1;
            }

            LOG.Console($"terra chunk complete, with size: {heights.GetLength(0)}, {heights.GetLength(0)} - origin {coord.x*(coordDimension)}, {coord.z*(coordDimension)}");

            terra.SetHeights(heights, coord.x*(coordDimension), coord.z*(coordDimension));
        }
    }
}