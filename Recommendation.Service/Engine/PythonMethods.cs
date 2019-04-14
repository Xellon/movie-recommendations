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
            return Task.Run(() => {
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

        //static string MatrixToString<T>(T[,] matrix)
        //{
        //    var matrixString = "";
        //    for (int i = 0; i < matrix.GetLength(0); i++)
        //    {
        //        for (int j = 0; j < matrix.GetLength(1); j++)
        //        {
        //            matrixString += matrix[i, j] + " ";
        //        }

        //        if (i + 1 < matrix.GetLength(0))
        //            matrixString = matrixString.TrimEnd() + "; ";
        //    }
        //    return matrixString;
        //}

        public static Task<double[,]> FindSimilarities(string stringifiedMatrix)
        {
            return Task.Run(() => {
                using (Py.GIL())
                {
                    dynamic pairwise = Py.Import("sklearn.metrics.pairwise");
                    dynamic cosine_similarity = pairwise.cosine_similarity;
                    dynamic np = Py.Import("numpy");


                    //dynamic countVectorizer = sklearn.feature_extraction.text.CountVectorizer();

                    //dynamic count_matrix = countVectorizer.fit_transform("");

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

            for (int i = 0; i < similarityList.Length(); i++)
            {
                var columnRows = PyList.AsList(similarityList[i]);
                var row = (double[])columnRows.AsManagedObject(typeof(double[]));
                for (int j = 0; j < row.Length; j++)
                {
                    similarities[i, j] = row[j];
                }
            }

            return similarities;
        }
    }
}
