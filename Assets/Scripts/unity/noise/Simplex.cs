using Unity.Mathematics;

namespace snorri
{
    public struct Simplex
    {
        public long seed;

        public Simplex(long seed)
        {
            this.seed = seed;
        }

        
        private float2 Seedify(float x, float y)
        {
            float3 seedOffset = new float3(seed % 43 * 0.0001f, seed % 235 * 0.0001f, seed % 123 * 0.0001f);
            float3 seedScale = new float3(1 + (seed % 103 * 0.0001f), 1 + (seed % 265 * 0.0001f), 1 + (seed % 23 * 0.0001f));

            return new float2((x+seedOffset.x)*seedScale.x, (y+seedOffset.y)*seedScale.y);
        }
        public float3 Seedify(float x, float y, float z)
        {
            float3 seedOffset = new float3(seed % 100 * 0.0001f, seed % 101 * 0.0001f, seed % 102 * 0.0001f);
            float3 seedScale = new float3(1 + (seed % 103 * 0.0001f), 1 + (seed % 104 * 0.0001f), 1 + (seed % 105 * 0.0001f));

            return new float3((x+seedOffset.x)*seedScale.x, (y+seedOffset.y)*seedScale.y, (z+seedOffset.z)*seedScale.z);
        }


        public float Evaluate(float x, float y, float z)
        {
            float3 seededCoords = Seedify(x,y,z);

            float noiseValue1 = noise.snoise(seededCoords);

            float noiseValue2 = noise.snoise(seededCoords * 1.6f + new float3(5.2f, -3.1f, 0.9f));
            float noiseValue3 = noise.snoise(seededCoords * 0.7f + new float3(-7.3f, 4.4f, 2.2f));
            
            float combinedNoise = (noiseValue1 + noiseValue2 * 0.5f + noiseValue3 * 0.25f) / 1.75f;
            
            return combinedNoise;
        }
        public float Evaluate(float x, float y)
        {
            float2 seededCoords = Seedify(x,y);

            float noiseValue1 = noise.snoise(seededCoords);

            float noiseValue2 = noise.snoise(seededCoords * 1.6f + new float2(5.2f, -3.1f));
            float noiseValue3 = noise.snoise(seededCoords * 0.7f + new float2(-7.3f, 4.4f));
            
            float combinedNoise = (noiseValue1 + noiseValue2 * 0.5f + noiseValue3 * 0.25f) / 1.75f;
            
            return combinedNoise;
        }
    }
}
