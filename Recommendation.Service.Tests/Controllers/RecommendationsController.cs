using Microsoft.AspNetCore.Mvc;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using System.Collections.Generic;

namespace Recommendation.Service.Controllers.Tests
{
    [TestClass]
    public class RecommendationsController
    {
        #region QueueRecommendation
        [TestMethod]
        public void QueueRecommendation_EmptyParameters_ReturnsBadRequestResult()
        {
            var queueMock = new Mock<IRecommendationQueue>();
            var controller = new Controllers.RecommendationsController(queueMock.Object);

            var response = controller.QueueRecommendation(null, null);

            Assert.IsInstanceOfType(response.Result, typeof(BadRequestResult));
        }

        [TestMethod]
        public void QueueRecommendation_NonEmptyParameters_ReturnsQueuedRecommendationId()
        {
            var expectedQueuedRecommendationId = 39;
            var queueMock = new Mock<IRecommendationQueue>();
            queueMock.Setup(q => q.QueueRecommendation(It.IsAny<int>(), It.IsAny<List<int>>())).Returns(expectedQueuedRecommendationId);
            var controller = new Controllers.RecommendationsController(queueMock.Object);

            var response = controller.QueueRecommendation(42, new List<int>());

            Assert.AreEqual(expectedQueuedRecommendationId, response.Value);

            queueMock.Verify(q => q.QueueRecommendation(It.IsAny<int>(), It.IsAny<List<int>>()), Times.Once);
        }
        #endregion

        [TestMethod]
        public void StopRecommendation_ValidRecommendationIdProvided_ReturnsOkResult()
        {
            var queueMock = new Mock<IRecommendationQueue>();
            var controller = new Controllers.RecommendationsController(queueMock.Object);

            var response = controller.StopRecommendation(1);

            Assert.IsInstanceOfType(response, typeof(OkResult));

            queueMock.Verify(q => q.StopRecommendation(It.IsAny<int>()), Times.Once);
        }

        [TestMethod]
        public void Status_ValidQueuedRecommendationIdProvided_ReturnsRecommendationStatus()
        {
            var queueMock = new Mock<IRecommendationQueue>();
            queueMock.Setup(q => q.GetRecommendationStatus(It.IsAny<int>())).Returns(Database.RecommendationStatus.Finished);
            var controller = new Controllers.RecommendationsController(queueMock.Object);

            var response = controller.Status(1);

            Assert.AreEqual(Database.RecommendationStatus.Finished, response.Value);

            queueMock.Verify(q => q.GetRecommendationStatus(It.IsAny<int>()), Times.Once);
        }

        [TestMethod]
        public void RecommendationId_ValidQueuedRecommendationIdProvided_ReturnsRecommendationId()
        {
            var expectedRecommendationId = 39;
            var queueMock = new Mock<IRecommendationQueue>();
            queueMock.Setup(q => q.GetRecommendationId(It.IsAny<int>())).Returns(expectedRecommendationId);
            var controller = new Controllers.RecommendationsController(queueMock.Object);

            var response = controller.RecommendationId(1);

            Assert.AreEqual(expectedRecommendationId, response.Value);

            queueMock.Verify(q => q.GetRecommendationId(It.IsAny<int>()), Times.Once);
        }
    }
}
