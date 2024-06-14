using Unity.Mathematics;
using Unity.Collections;

namespace snorri
{
    public struct GaussianBlur
    {
        private NativeArray<float> kernel;

        public GaussianBlur(Allocator allocator = Allocator.Persistent)
        {
            // Initialize the 5x5 Gaussian kernel with the pre-calculated weights
            kernel = new NativeArray<float>(25, allocator)
            {
                [0] = 0.00296902f, [1] = 0.01330621f, [2] = 0.02193823f, [3] = 0.01330621f, [4] = 0.00296902f,
                [5] = 0.01330621f, [6] = 0.0596343f,  [7] = 0.09832033f, [8] = 0.0596343f,  [9] = 0.01330621f,
                [10] = 0.02193823f, [11] = 0.09832033f, [12] = 0.16210282f, [13] = 0.09832033f, [14] = 0.02193823f,
                [15] = 0.01330621f, [16] = 0.0596343f,  [17] = 0.09832033f, [18] = 0.0596343f,  [19] = 0.01330621f,
                [20] = 0.00296902f, [21] = 0.01330621f, [22] = 0.02193823f, [23] = 0.01330621f, [24] = 0.00296902f
            };
        }
        public float Blur(NativeArray<float> noiseVals)
        {
            // Implementation goes here
            float sum = 0f;
            for (int i = 0; i < noiseVals.Length; i++)
            {
                sum += kernel[i] * noiseVals[i];
            }

            return sum;
        }
        public void Dispose()
        {
            if (kernel.IsCreated)
            {
                kernel.Dispose();
            }
        }
    }
}
