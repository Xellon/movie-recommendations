using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Recommendation.Service
{
    public interface IQueueHandler
    {
        void StopRecommendation(int queuedRecommendationId);
    }

    public class QueueHandler : IQueueHandler
    {
        struct RecommendationTask
        {
            public Task<int> Task { get; set; }
            public int QueuedRecommendationId { get; set; }
        }

        private readonly int ConcurrentRecommendationsLimit = 4;
        private readonly Database.DatabaseContext _context;
        private readonly IRecommendationQueue _queue;
        private readonly IQueuedRecommendationStorage _storage;
        private readonly IRecommendationEngine _recommendationEngine;
        private readonly Timer _timer;
        private List<RecommendationTask> _runningTasks;

        public QueueHandler(Database.DatabaseContext databaseContext, IRecommendationQueue queue, IQueuedRecommendationStorage storage)
        {
            _queue = queue;
            _storage = storage;
            _context = databaseContext;
            _recommendationEngine = new RecommendationEngine(databaseContext);
            _runningTasks = new List<RecommendationTask>(ConcurrentRecommendationsLimit);
            _timer = new Timer(Tick, null, 1000, Timeout.Infinite);
        }

        private void Tick(object state)
        {
            // Stop a task if it's marked as such
            // ...

            var finishedTasks = _runningTasks.Where(t => !t.Task.IsCompleted);
            var runningTasks = _runningTasks.Where(t => t.Task.IsCompleted);

            // Continue tasks that are still running
            _runningTasks = new List<RecommendationTask>(runningTasks);

            // Fill task list if needed
            for (int i = runningTasks.Count(); i < ConcurrentRecommendationsLimit; i++)
            {
                var queuedRecommendation = _queue.GetUnstartedRecommendation();

                if (queuedRecommendation is null)
                    break;

                var task = _recommendationEngine.FindOutStuff(queuedRecommendation.RecommendationParameters);
                _runningTasks.Add(new RecommendationTask
                {
                    Task = task,
                    QueuedRecommendationId = queuedRecommendation.Id
                });
            }

            // Update queued recommendations that are finished
            foreach (var task in finishedTasks)
            {
                _storage.SetRecommendationStatus(task.QueuedRecommendationId, Database.RecommendationStatus.Finished);
                _storage.SetRecommendationId(task.QueuedRecommendationId, task.Task.Result);
            }
        }

        public void StopRecommendation(int queuedRecommendationId)
        {
            _storage.SetRecommendationStatus(queuedRecommendationId, Database.RecommendationStatus.Stopped);
        }
    }
}
