using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Collections.Generic;

namespace Recommendation.Service.Tests.Integration
{
    [TestClass]
    public class RedisQueuedRecommendationStorage
    {
        Service.RedisQueuedRecommendationStorage _storage = new Service.RedisQueuedRecommendationStorage("localhost");
        readonly RecommendationParameters _testParameters = new RecommendationParameters
        {
            UserId = "asdf",
            RequestedTagIds = new List<int> { 1, 2 }
        };

        [ClassInitialize]
        public static void ClassInitialize(TestContext context)
        {
            var storage = new Service.RedisQueuedRecommendationStorage("localhost");
            storage.FlushDB();
        }

        [TestCleanup]
        public void CleanUp()
        {
            _storage.FlushDB();
        }

        [TestMethod]
        public void Add_RegularParameters_ReturnsValidRecommendationId()
        {
            var recommendationId = _storage.Add(_testParameters);

            Assert.AreEqual(1, recommendationId);

            recommendationId = _storage.Add(_testParameters);

            Assert.AreEqual(2, recommendationId);
        }

        [TestMethod]
        public void GetQueuedCount_3RecommendationsInQueue_Returns3()
        {
            _storage.Add(_testParameters);
            _storage.Add(_testParameters);
            _storage.Add(_testParameters);

            var queuedCount = _storage.GetQueuedCount();

            Assert.AreEqual(3, queuedCount);

            _storage.SetRecommendationStatus(1, Database.RecommendationStatus.InProgress);

            queuedCount = _storage.GetQueuedCount();

            Assert.AreEqual(2, queuedCount);
        }

        [TestMethod]
        public void GetOldestUnstartedRecommendation_3QueuedRecommendations_ReturnsFirstOne()
        {
            _storage.Add(new RecommendationParameters
            {
                UserId = "asdf1",
                RequestedTagIds = new List<int> { 1 }
            });
            _storage.Add(new RecommendationParameters
            {
                UserId = "asdf2",
                RequestedTagIds = new List<int> { 2 }
            });
            _storage.Add(new RecommendationParameters
            {
                UserId = "asdf3",
                RequestedTagIds = new List<int> { 3 }
            });

            var recommendation = _storage.GetOldestUnstartedRecommendation();

            Assert.AreEqual(1, recommendation.Id);
            Assert.AreEqual(Database.RecommendationStatus.Queued, recommendation.Status);
            Assert.AreEqual("asdf1", recommendation.RecommendationParameters.UserId);

            _storage.SetRecommendationStatus(1, Database.RecommendationStatus.InProgress);
            recommendation = _storage.GetOldestUnstartedRecommendation();
            Assert.AreEqual(2, recommendation.Id);
        }


        [TestMethod]
        public void Remove_SingleRecommendationRemoved_RemovedSuccessfully()
        {
            _storage.Add(new RecommendationParameters
            {
                UserId = "asdf1",
                RequestedTagIds = new List<int> { 1 }
            });

            var recommendation = _storage.GetOldestUnstartedRecommendation();

            Assert.AreEqual(1, recommendation.Id);
            Assert.AreEqual(Database.RecommendationStatus.Queued, recommendation.Status);
            Assert.AreEqual("asdf1", recommendation.RecommendationParameters.UserId);

            Assert.IsTrue(_storage.Remove(1));

            recommendation = _storage.GetOldestUnstartedRecommendation();
            Assert.AreEqual(0, recommendation.Id);
        }
    }
}
