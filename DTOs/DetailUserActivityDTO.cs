using System;

namespace Dnn.CommunityMetrics
{
    public class DetailUserActivityDTO
    {

        // initialization
        public DetailUserActivityDTO()
        {
        }

        public int activity_id { get; set; }

        public string activity_name { get; set; }

        public int count { get; set; }
        public double total_points { get; set; }

    }
}