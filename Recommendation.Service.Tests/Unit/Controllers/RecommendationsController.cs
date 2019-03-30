using Microsoft.AspNetCore.Mvc;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using System;
using System.Collections.Generic;

namespace Recommendation.Service.Controllers.Tests.Unit
{
    [TestClass]
    public class RecommendationsController
    {
        private Mock<IQueuedRecommendationStorage> _storageMock;
        private Mock<IRecommendationQueue> _queueMock;
        private Mock<IQueueHandler> _handlerMock;
        private Controllers.RecommendationsController _controller;

        [TestInitialize]
        public void BeforeEach()
        {
            _storageMock = new Mock<IQueuedRecommendationStorage>();
            _queueMock = new Mock<IRecommendationQueue>();
            _handlerMock = new Mock<IQueueHandler>();

            _controller = new Controllers.RecommendationsController(
                null,
                _storageMock.Object, 
                _queueMock.Object,
                _handlerMock.Object);
        }

        #region QueueRecommendation
        [TestMethod]
        public void QueueRecommendation_EmptyParameters_ReturnsBadRequestResult()
        {
            var response = _controller.QueueRecommendation(null, null);

            Assert.IsInstanceOfType(response.Result, typeof(BadRequestResult));
        }

        [TestMethod]
        public void QueueRecommendation_NonEmptyParameters_ReturnsQueuedRecommendationId()
        {
            var expectedQueuedRecommendationId = 39;
            _queueMock.Setup(q => q.QueueRecommendation(It.IsAny<string>(), It.IsAny<List<int>>())).Returns(expectedQueuedRecommendationId);

            var response = _controller.QueueRecommendation(Guid.NewGuid().ToString(), new List<int>());

            Assert.AreEqual(expectedQueuedRecommendationId, response.Value);

            _queueMock.Verify(q => q.QueueRecommendation(It.IsAny<string>(), It.IsAny<List<int>>()), Times.Once);
        }
        #endregion

        [TestMethod]
        public void StopRecommendation_ValidRecommendationIdProvided_ReturnsOkResult()
        {
            var response = _controller.StopRecommendation(1);

            Assert.IsInstanceOfType(response, typeof(OkResult));

            _handlerMock.Verify(h => h.StopRecommendation(It.IsAny<int>()), Times.Once);
        }

        [TestMethod]
        public void Status_ValidQueuedRecommendationIdProvided_ReturnsRecommendationStatus()
        {
            _storageMock.Setup(s => s.GetRecommendationStatus(It.IsAny<int>()))
                .Returns(Database.RecommendationStatus.Finished);

            var response = _controller.Status(1);

            Assert.AreEqual(Database.RecommendationStatus.Finished, response.Value);

            _storageMock.Verify(s => s.GetRecommendationStatus(It.IsAny<int>()), Times.Once);
        }

        [TestMethod]
        public void RecommendationId_ValidQueuedRecommendationIdProvided_ReturnsRecommendationId()
        {
            var expectedRecommendationId = 39;

            _storageMock.Setup(s => s.GetRecommendationId(It.IsAny<int>()))
                .Returns(expectedRecommendationId);

            var response = _controller.RecommendationId(1);

            Assert.AreEqual(expectedRecommendationId, response.Value);

            _storageMock.Verify(s => s.GetRecommendationId(It.IsAny<int>()), Times.Once);
        }
    }
}
