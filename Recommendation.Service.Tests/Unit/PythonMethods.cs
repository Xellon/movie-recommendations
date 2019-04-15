using Microsoft.VisualStudio.TestTools.UnitTesting;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Python.Runtime;

namespace Recommendation.Service.Tests.Unit
{
    [TestClass]
    public class PythonMethods
    {
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
        public async Task FindKeywords()
        {
            var descriptions = new List<string> { "Hello and welcome to Jackass", "Welcome to hype machine" };

            var keywordLists = (await Service.PythonMethods.FindKeywords(descriptions)).Select(keywords => keywords.OrderBy(word => word));

            var serializedKeywordLists = JsonConvert.SerializeObject(keywordLists);

            Assert.AreEqual("[[\"hello\",\"jackass\",\"welcome\"],[\"hype\",\"machine\",\"welcome\"]]", serializedKeywordLists);
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
        public async Task VectorizeDocuments()
        {
            var documents = new string[]
            {
                "arm beard cinnamon",
                "beard cinnamon",
                "arm cinnamon"
            };

            var matrix = await Service.PythonMethods.VectorizeDocuments(documents);

            var stringifiedMatrix = JsonConvert.SerializeObject(matrix);

            Assert.AreEqual("[[1,1,1],[0,1,1],[1,0,1]]", stringifiedMatrix);
        }
    }
}
