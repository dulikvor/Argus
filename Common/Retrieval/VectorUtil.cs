using Argus.Common.Telemetry;

namespace Argus.Common.Retrieval
{
    public static class VectorUtils
    {
        public static double CosineSimilarity(float[] v1, float[] v2)
        {
            using var activityScope = ActivityScope.Create(nameof(VectorUtils));
            return activityScope.Monitor(() =>
            {
                if (v1.Length != v2.Length) throw new ArgumentException("Vectors must be the same length");

                double dot = 0, normV1 = 0, normV2 = 0;

                for (int i = 0; i < v1.Length; i++)
                {
                    dot += v1[i] * v2[i];
                    normV1 += v1[i] * v1[i];
                    normV2 += v2[i] * v2[i];
                }

                return dot / (Math.Sqrt(normV1) * Math.Sqrt(normV2));
            });
        }
    }
}
