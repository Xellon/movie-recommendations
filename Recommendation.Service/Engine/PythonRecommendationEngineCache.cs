using System.Linq;
using System.IO;

namespace Recommendation.Service
{
    public class PythonRecommendationEngineCache
    {
        private readonly PythonEngineOptions _options;

        public PythonRecommendationEngineCache(PythonEngineOptions options)
        {
            _options = options;
        }

        public int[] RetrieveMovieIdsFromCache()
        {
            var path = Path.Join(_options.RecommendationCacheLocation, _options.IdArrayFilename);
            var idsText = File.ReadAllText(path);
            return idsText.Trim().Split(' ').Select(id => int.Parse(id)).ToArray();
        }

        public double[][] RetrieveSimilarityMatrixFromCache(int[] movieIds)
        {
            var matrixText = File.ReadAllLines(Path.Join(_options.RecommendationCacheLocation, _options.SimilarityMatrixFilename));
            if (matrixText.Length != movieIds.Length)
                return new double[0][];

            var similarityMatrix = new double[matrixText.Length][];
            for (int i = 0; i < matrixText.Length; i++)
            {
                similarityMatrix[i] = matrixText[i].Trim().Split(' ').Select(t => double.Parse(t)).ToArray();
            }

            return similarityMatrix;
        }

        public void CacheMovieIdArray(int[] ids)
        {
            var idArrayPath = Path.Join(_options.RecommendationCacheLocation, _options.IdArrayFilename);
            using (StreamWriter file = new StreamWriter(idArrayPath))
            {
                file.Write(ids.Aggregate("", (w1, w2) => w1 + " " + w2));
            }
        }

        public void CacheSimilarityMatrix(double[,] similarityMatrix)
        {
            var similarityMatrixPath = Path.Join(_options.RecommendationCacheLocation, _options.SimilarityMatrixFilename);
            using (StreamWriter file = new StreamWriter(similarityMatrixPath))
            {
                for (int i = 0; i < similarityMatrix.GetLength(0); i++)
                {
                    for (int j = 0; j < similarityMatrix.GetLength(1); j++)
                    {
                        file.Write(similarityMatrix[i, j] + " ");
                    }

                    if (i < similarityMatrix.GetLength(0) - 1)
                        file.Write(file.NewLine);
                }
            }
        }
    }
}
