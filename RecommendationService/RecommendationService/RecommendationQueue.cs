using System.Collections.Generic;
using System.Threading.Tasks;

namespace RecommendationService
{
    public class RecommendationQueue
    {
        private int _lastId = 0;
        private List<QueuedRecommendation> _recommendations;

        private int GenerateNewId() => ++_lastId;

        public int Add(Task recommendationTask)
        {
            var id = GenerateNewId();
            return id;
        }



        public void Remove(int id)
        {

        }

    }
}
