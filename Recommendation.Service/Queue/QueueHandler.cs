using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
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
        private readonly IRecommendationQueue _queue;
        private readonly IQueuedRecommendationStorage _storage;
        private readonly IRecommendationEngine _recommendationEngine;
        private List<RecommendationTask> _runningTasks;
        private readonly ILogger _logger;

        private static QueueHandler _handlerInstance = null;

        public QueueHandler(DbContextOptions<Database.DatabaseContext> dbContextOptions, IConfiguration configuration, IRecommendationQueue queue, IQueuedRecommendationStorage storage, ILogger logger = null)
        {
            _queue = queue;
            _storage = storage;
            _logger = logger;

            var engineOptions = configuration.Get<PythonEngineOptions>();

            _recommendationEngine = new PythonRecommendationEngine(dbContextOptions, engineOptions);
            _runningTasks = new List<RecommendationTask>(ConcurrentRecommendationsLimit);

            Task.Run(() =>
            {
                while (true)
                {
                    Tick();
                    Thread.Sleep(500);
                }
            });
        }

        /// <summary>
        /// Either returns an already created instance or creates a new one. This is needed because mvc controllers are recreated on each request
        /// </summary>
        public static QueueHandler GetOrCreate(DbContextOptions<Database.DatabaseContext> dbContextOptions, IConfiguration configuration, IRecommendationQueue queue, IQueuedRecommendationStorage storage, ILogger logger = null)
        {
            if(_handlerInstance is null)
                _handlerInstance = new QueueHandler(dbContextOptions, configuration, queue, storage, logger);

            return _handlerInstance;
        }

        private void Tick()
        {
            // Stop a task if it's marked as such
            // ...

            var finishedTasks = _runningTasks.Where(t => t.Task.IsCompleted || t.Task.Status == TaskStatus.Faulted);
            var runningTasks = _runningTasks.Where(t => !t.Task.IsCompleted && t.Task.Status != TaskStatus.Faulted);

            // Continue tasks that are still running
            _runningTasks = new List<RecommendationTask>(runningTasks);

            // Fill task list if needed
            FillTaskList();

            UpdateFinishedTasks(finishedTasks);
        }

        private void UpdateFinishedTasks(IEnumerable<RecommendationTask> finishedTasks)
        {
            // Update queued recommendations that are finished
            foreach (var task in finishedTasks)
            {
                if (!(task.Task.Exception is null))
                {
                    _storage.SetRecommendationStatus(task.QueuedRecommendationId, Database.RecommendationStatus.Error);
                    _storage.SetRecommendationId(task.QueuedRecommendationId, 0);

                    _logger.LogInformation(task.Task.Exception, task.Task.Exception.Message);
                    _logger.LogError(task.Task.Exception.Message);

                    continue;
                }

                _storage.SetRecommendationStatus(task.QueuedRecommendationId, Database.RecommendationStatus.Finished);
                _storage.SetRecommendationId(task.QueuedRecommendationId, task.Task.Result);
            }
        }

        private void FillTaskList()
        {
            // Fill task list if needed
            for (int i = 0; i < Math.Min(ConcurrentRecommendationsLimit - _runningTasks.Count(), _queue.QueuedCount); i++)
            {
                var queuedRecommendation = _queue.GetUnstartedRecommendation();

                if (queuedRecommendation is null || _storage.GetRecommendationStatus(queuedRecommendation.Id) != Database.RecommendationStatus.Queued)
                    break;

                _storage.SetRecommendationStatus(queuedRecommendation.Id, Database.RecommendationStatus.InProgress);

                var task = _recommendationEngine.GenerateRecommendation(queuedRecommendation.RecommendationParameters);
                _runningTasks.Add(new RecommendationTask
                {
                    Task = task,
                    QueuedRecommendationId = queuedRecommendation.Id
                });
            }
        }

        public void StopRecommendation(int queuedRecommendationId)
        {
            _storage.SetRecommendationStatus(queuedRecommendationId, Database.RecommendationStatus.Stopped);
        }
    }
}
