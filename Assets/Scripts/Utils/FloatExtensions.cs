using System;
using UnityEngine;
using Random = System.Random;

namespace Utils
{
    public static class FloatExtensions
    {
        public static float RoundToDecimalPlaces(this float num, int places)
        {
            double multiplier = Math.Pow(10, places);
            return (int)(num * multiplier) / (float) multiplier;
        }

        public static float RandomBetween(float min, float max)
        {
            Random random = new Random();
            var value = random.Next(1000000);
            return Mathf.Lerp(min, max, value / 1000000f);
        }

        public static bool ApproxEquals(this float a, float b)
        {
            if (a == b) // basically just good for infinity
                return true;
            
            float absA = Math.Abs(a);
            float absB = Math.Abs(b);
            float diff = Math.Abs(a - b);

            if (a == 0 || b == 0 || absA + absB < float.Epsilon)
                return diff < float.Epsilon;
            
            return diff / (absA + absB) < float.Epsilon;
        }
    }
}