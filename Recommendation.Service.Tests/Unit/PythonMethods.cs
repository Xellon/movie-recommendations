using Microsoft.VisualStudio.TestTools.UnitTesting;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Python.Runtime;
using System;

namespace Recommendation.Service.Tests.Unit
{
    [TestClass]
    public class PythonMethods
    {
        double[,] RoundMatrix(double[,] matrix)
        {
            var xSize = matrix.GetLength(0);
            var ySize = matrix.GetLength(1);
            var newMatrix = new double[xSize, ySize];

            for (int x = 0; x < xSize; x++)
            {
                for (int y = 0; y < ySize; y++)
                {
                    newMatrix[x, y] = Math.Round(matrix[x, y], 2);
                }
            }

            return newMatrix;
        }

        [ClassInitialize]
        public static void Init(TestContext testContext)
        {
            if (!PythonEngine.IsInitialized)
            {
                PythonEngine.Initialize();
                PythonEngine.BeginAllowThreads();
            }
        }

        [TestMethod]
        public async Task FindSimilarities_SameVectors_ReturnsCorrectMatrix()
        {
            var similarityMatrix = await Service.PythonMethods.FindSimilarities("0 1; 0 1");
            var serializedMatrix = JsonConvert.SerializeObject(similarityMatrix);
            Assert.AreEqual("[[1.0,1.0],[1.0,1.0]]", serializedMatrix);
        }

        [TestMethod]
        public async Task FindSimilarities_DifferentVectors_ReturnsCorrectMatrix()
        {
            var similarityMatrix = await Service.PythonMethods.FindSimilarities("0 1; 1 0");
            var serializedMatrix = JsonConvert.SerializeObject(similarityMatrix);
            Assert.AreEqual("[[1.0,0.0],[0.0,1.0]]", serializedMatrix);
        }


        [TestMethod]
        public async Task VectorizeDocumentsTFIDF_ThreeSentences_VectorizesWordsCorrectly()
        {
            var documents = new string[]
            {
                "What's an arm beard cinnamon?",
                "Here's beard cinnamon.",
                "Arm, cinnamon!"
            };

            var matrix = await Service.PythonMethods.VectorizeDocumentsTFIDF(documents);

            matrix = RoundMatrix(matrix);

            var stringifiedMatrix = JsonConvert.SerializeObject(matrix);

            Assert.AreEqual(
                "[[0.53,0.41,0.41,0.32,0.0,0.53],[0.0,0.0,0.55,0.43,0.72,0.0],[0.0,0.79,0.0,0.61,0.0,0.0]]", 
                stringifiedMatrix);
        }
    }
}
