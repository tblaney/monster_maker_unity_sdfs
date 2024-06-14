using UnityEngine;
using System;

namespace snorri
{
        
    [System.Serializable]
    public struct TerraNoise
    {
        public int id;

        public int noiseType;
        public int seed;

        public float frequency;
        public float weight;

        public int fractalType;
        public int fractalOctaves;
        public float fractalLacunarity;
        public float fractalGain;

        public float offset;

        public int isFractal;

        public TerraNoise(Map args, int seedIn, int i = 0)
        {
            id = args.Get<int>("id");
            noiseType = args.Get<int>("noise_type");
            frequency = args.Get<float>("frequency");
            weight = args.Get<float>("weight");
            fractalType = args.Get<int>("fractal_type");
            fractalOctaves = args.Get<int>("fractal_octaves");
            fractalLacunarity = args.Get<float>("fractal_lacunarity");
            fractalGain = args.Get<float>("fractal_gain");
            offset = args.Get<float>("offset");
            bool isFractalBool = args.Get<bool>("is_fractal");
            isFractal = 0;
            if (isFractalBool)
            {
                isFractal = 1;
            }

            seed = 1;
            SetSeed(seedIn, i);
        }

        public void SetSeed(int seedIn, int i)
        {
            seed = RandomizeSeed(seedIn, i);
        }
        public int RandomizeSeed(int seed, int i)
        {
            switch (i)
            {
                case 0:
                    return seed;
                case 1:
                    return seed / 2 + 1;
                case 2:
                    return seed / 3 + 1;
                case 3:
                    return seed / 25 + 100;
                case 4:
                    return seed * 8;
            }
            return seed;
        }
    }

}
