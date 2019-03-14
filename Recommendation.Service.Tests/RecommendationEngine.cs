using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Collections.Generic;
using Moq;

namespace Recommendation.Service.Tests
{
    [TestClass]
    public class RecommendationEngine
    {
        [TestMethod]
        public async void FindOutStuff()
        {
            var dbContext = new Mock<Database.DatabaseContext>();
            var engine = new Service.RecommendationEngine(null);
            var stuff = await engine.FindOutStuff(1, new List<int>());
        }
    }
}
