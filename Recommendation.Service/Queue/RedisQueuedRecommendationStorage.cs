using System;
using Newtonsoft.Json;
using Recommendation.Database;
using StackExchange.Redis;

namespace Recommendation.Service
{
    public class RedisQueuedRecommendationStorage : IQueuedRecommendationStorage
    {
        private readonly ConnectionMultiplexer _connection;

        private IDatabase DataBase => _connection.GetDatabase();

        public RedisQueuedRecommendationStorage(string connectionString)
        {
            _connection = ConnectionMultiplexer.Connect(connectionString);
        }

        public void FlushDB()
        {
            _connection.GetDatabase().Execute("FLUSHDB");
        }

        private int GetNewRecommendationId()
        {
            return (int)DataBase.StringIncrement("recommendation:id");
        }

        private int GetLatestRecommendationId()
        {
            return (int)DataBase.StringGet("recommendation:id");
        }

        public int GetQueuedCount()
        {
            var result = DataBase.ScriptEvaluate(@"
                local count = 0
                local matches = redis.pcall('KEYS', 'recommendation:*')

                for _,key in ipairs(matches) do
                    if key ~= 'recommendation:id' then 
                        local val = redis.pcall('HGET', key, 'status')
                        if val == '0' then
                            count = count + 1
                        end
                    end
                end

                return count
            ");

            try
            {
                return int.Parse(result.ToString());
            }
            catch
            {
                return 0;
            }
        }

        public int Add(RecommendationParameters parameters)
        {
            var id = GetNewRecommendationId();
            var hash = new HashEntry[] {
                new HashEntry("recommendationParameters", JsonConvert.SerializeObject(parameters)),
                new HashEntry("status", (int)RecommendationStatus.Queued),
                new HashEntry("startTime", DateTime.Now.ToString()),
            };

            DataBase.HashSet($"recommendation:{id}", hash);
            return id;
        }

        public QueuedRecommendation GetOldestUnstartedRecommendation()
        {
            var oldestDate = DateTime.Now;
            var oldestIndex = 0;

            for (int i = 1; i <= GetLatestRecommendationId(); i++)
            {
                if (!DataBase.KeyExists($"recommendation:{i}"))
                    continue;

                var status = GetRecommendationStatus(i);
                var date = GetRecommendationStartTime(i);

                if (status == RecommendationStatus.Queued && date < oldestDate)
                {
                    oldestDate = date;
                    oldestIndex = i;
                }
            }

            return new QueuedRecommendation()
            {
                Id = oldestIndex,
                RecommendationId = GetRecommendationId(oldestIndex),
                RecommendationParameters = GetRecommendationParameters(oldestIndex),
                StartTime = GetRecommendationStartTime(oldestIndex),
                Status = GetRecommendationStatus(oldestIndex)
            };
        }

        public int GetRecommendationId(int queuedRecommendationId)
        {
            var value = DataBase.HashGet($"recommendation:{queuedRecommendationId}", "recommendationId");

            if (value.HasValue && value.TryParse(out int id))
                return id;

            return 0;
        }

        public RecommendationStatus GetRecommendationStatus(int queuedRecommendationId)
        {
            var value = DataBase.HashGet($"recommendation:{queuedRecommendationId}", "status");

            if(value.HasValue && value.TryParse(out int status))
                return (RecommendationStatus)status;

            return RecommendationStatus.DoesNotExist;
        }

        public DateTime GetRecommendationStartTime(int queuedRecommendationId)
        {
            var value = DataBase.HashGet($"recommendation:{queuedRecommendationId}", "startTime");

            if (value.HasValue)
                return DateTime.Parse(value.ToString());

            return new DateTime(0);
        }

        public RecommendationParameters GetRecommendationParameters(int queuedRecommendationId)
        {
            var value = DataBase.HashGet($"recommendation:{queuedRecommendationId}", "recommendationParameters");

            if (value.IsNullOrEmpty)
                return null;

            return JsonConvert.DeserializeObject<RecommendationParameters>(value.ToString());
        }

        public bool Remove(int queuedRecommendationId)
        {
            return DataBase.KeyDelete($"recommendation:{queuedRecommendationId}");
        }

        public void SetRecommendationId(int queuedRecommendationId, int recommendationId)
        {
            DataBase.HashSet($"recommendation:{queuedRecommendationId}", "recommendationId", recommendationId);
        }

        public void SetRecommendationStatus(int queuedRecommendationId, RecommendationStatus newStatus)
        {
            DataBase.HashSet($"recommendation:{queuedRecommendationId}", "status", (int)newStatus);
        }
    }
}
