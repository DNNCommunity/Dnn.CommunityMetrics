using System;

namespace Dnn.CommunityMetrics
{
    public class RankedUserActivityDTO
    {

        // initialization
        public RankedUserActivityDTO()
        {
        }

        public int rank { get; set; }

        public int user_id { get; set; }

        public string user_name { get; set; }

        public double total_points { get; set; }
    }
}