using Unity.Mathematics;

namespace snorri
{
    public struct Perlin
    {
        public long seed;

        public Perlin(long seed)
        {
            this.seed = seed;
        }

        private float2 Seedify(float x, float y)
        {
            float3 seedOffset = new float3(seed % 100 * 0.0001f, seed % 101 * 0.0001f, seed % 102 * 0.0001f);
            float3 seedScale = new float3(1 + (seed % 103 * 0.0001f), 1 + (seed % 104 * 0.0001f), 1 + (seed % 105 * 0.0001f));

            return new float2((x+seedOffset.x)*seedScale.x, (y+seedOffset.y)*seedScale.y);
        }
        private float3 Seedify(float x, float y, float z)
        {
            float3 seedOffset = new float3(seed % 100 * 0.0001f, seed % 101 * 0.0001f, seed % 102 * 0.0001f);
            float3 seedScale = new float3(1 + (seed % 103 * 0.0001f), 1 + (seed % 104 * 0.0001f), 1 + (seed % 105 * 0.0001f));

            return new float3((x+seedOffset.x)*seedScale.x, (y+seedOffset.y)*seedScale.y, (z+seedOffset.z)*seedScale.z);
        }

        public float Noise(float x, float y)
        {
            return noise.cnoise(Seedify(x,y));
        }
        public float Noise3D(float x, float y, float z)
        {
            float AB = noise.cnoise(new float2(x, y));
            float BC = noise.cnoise(new float2(y, z));
            float AC = noise.cnoise(new float2(x, z));

            float BA = noise.cnoise(new float2(y, x));
            float CB = noise.cnoise(new float2(z, y));
            float CA = noise.cnoise(new float2(z, x));

            float ABC = AB + BC + AC + BA + CB + CA;
            return ABC / 6f;
        }
        public float NoiseDistorted(float x, float y, float strength)
        {
            float2 seedifiedCoords = Seedify(x,y);
            x = seedifiedCoords.x;
            y = seedifiedCoords.y;
            float xDistortion = strength * Distort(x + 2.3f, y + 2.9f);
            float yDistortion = strength * Distort(x - 3.1f, y - 4.3f);

            return noise.cnoise(new float2(x + xDistortion, y + yDistortion));
        }
        private float Distort(float x, float y)
        {
            float wiggleDensity = 4.7f;
            return noise.cnoise(new float2(x * wiggleDensity, y * wiggleDensity));
        }
    }
}
