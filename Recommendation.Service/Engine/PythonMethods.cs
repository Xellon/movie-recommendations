using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Python.Runtime;

namespace Recommendation.Service
{
    public static class PythonMethods
    {
        public static Task<IEnumerable<string[]>> FindKeywords(IEnumerable<string> descriptions)
        {
            return Task.Run(() => 
            {
                var keywords = new List<string[]>();
                using (Py.GIL())
                {
                    dynamic rake_nltk = Py.Import("rake_nltk");

                    foreach (var description in descriptions)
                    {
                        dynamic rake = rake_nltk.Rake();
                        rake.extract_keywords_from_text(description);
                        PyList keys = PyList.AsList(rake.get_word_degrees().keys());
                        keywords.Add((string[])keys.AsManagedObject(typeof(string[])));
                    }
                }

                return keywords.AsEnumerable();
            });
        }

        public static Task<long[,]> VectorizeDocuments(IEnumerable<string> documents)
        {
            return Task.Run(() => 
            {
                try
                {
                    using (Py.GIL())
                    {
                        dynamic sklear_feature_extraction_text = Py.Import("sklearn.feature_extraction.text");
                        dynamic countVectorizer = sklear_feature_extraction_text.CountVectorizer();
                        dynamic np = Py.Import("numpy");

                        dynamic countMatrixObject = countVectorizer.fit_transform(documents);
                        PyList countMatrix = PyList.AsList(countMatrixObject.toarray());
                        var matrix = (long[][])countMatrix.AsManagedObject(typeof(long[][]));

                        return ConvertMatrix(matrix);
                    }
                }
                catch(PythonException e)
                {
                    if (e.Message.Contains("empty vocabulary"))
                        return new long[0, 0];
                    else
                        throw e;
                }
            });
        }

        public static Task<double[,]> FindSimilarities(string stringifiedMatrix)
        {
            return Task.Run(() => 
            {
                using (Py.GIL())
                {
                    dynamic pairwise = Py.Import("sklearn.metrics.pairwise");
                    dynamic cosine_similarity = pairwise.cosine_similarity;
                    dynamic np = Py.Import("numpy");

                    dynamic matrix = np.array(np.mat(stringifiedMatrix));

                    PyDict locals = new PyDict();
                    locals.SetItem("X", matrix);
                    locals.SetItem("cosine_similarity", cosine_similarity);

                    dynamic cosineSimilarityObj = PythonEngine.Eval("cosine_similarity(X, X)", null, locals.Handle);

                    PyList cosineSimilarity = PyList.AsList(cosineSimilarityObj);

                    return ConvertSimilarityListToManaged(cosineSimilarity);
                }
            });
        }

        private static double[,] ConvertSimilarityListToManaged(PyList similarityList)
        {
            var length = similarityList.Length();
            var similarities = new double[length, length];

            for (int i = 0; i < length; i++)
            {
                var columnRows = PyList.AsList(similarityList[i]);
                var row = (double[])columnRows.AsManagedObject(typeof(double[]));
                for (int j = 0; j < length; j++)
                {
                    similarities[i, j] = row[j];
                }
            }

            return similarities;
        }

        private static T[,] ConvertMatrix<T>(T[][] matrix)
        {
            var xLength = matrix.Length;
            var yLength = matrix[0].Length;

            var convertedMatrix = new T[xLength, yLength];
            for (int i = 0; i < xLength; i++)
            {
                for (int j = 0; j < yLength; j++)
                {
                    convertedMatrix[i, j] = matrix[i][j];
                }
            }
            return convertedMatrix;
        }
    }
}
