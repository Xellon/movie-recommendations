using Microsoft.AspNetCore.Mvc;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using System.Collections.Generic;

namespace Recommendation.Service.Controllers.Tests
{
    [TestClass]
    public class RecommendationsController
    {
        [TestMethod]
        public void QueueRecommendation_EmptyParameters_ReturnsBadRequestResult()
        {
            var contextMoq = new Mock<Database.DatabaseContext>();

            var controller = new Controllers.RecommendationsController(contextMoq.Object);

            var response = controller.QueueRecommendation(null, null);

            Assert.IsInstanceOfType(response.Result, typeof(BadRequestResult));
        }

        [TestMethod]
        public void QueueRecommendation_EmptyParameters_ReturnsBadRequestResult2()
        {
            var contextMoq = new Mock<Database.DatabaseContext>();

            var controller = new Controllers.RecommendationsController(contextMoq.Object);

            var response = controller.QueueRecommendation(42, new List<int>());

            Assert.IsInstanceOfType(response.Result, typeof(OkResult));
        }
    }
}
