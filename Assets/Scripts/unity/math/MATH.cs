using UnityEngine;
using System;

namespace snorri
{
    public static class MATH
    {
        public static int FindNearestMultiple(int targetInt, int baseInt)
        {
            if (baseInt == 0)
            {
                throw new ArgumentException("baseInt cannot be zero.", nameof(baseInt));
            }

            int nearestMultipleFactor = (int)Mathf.Round((float)targetInt / baseInt);

            int nearestMultiple = nearestMultipleFactor * baseInt;

            return nearestMultiple;
        }
    }
}