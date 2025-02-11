using Pgvector;

namespace Mistral.Utils
{
    internal class EmbeddingUtils
    {
        public static IEnumerable<decimal> ConvertToDecimal(Vector vector)
        {
            return vector.ToArray().Select(d => (decimal)d).ToList();
        }

        public static IEnumerable<float> ConvertToFloat(IEnumerable<decimal> decimalColl)
        {
            return decimalColl.Select(d => (float)d).ToArray();
        }

        public static Vector ConvertToVector(IEnumerable<decimal> decimalColl)
        {
            return new Vector(ConvertToFloat(decimalColl).ToArray());
        }
    }
}
