using System.Collections;
using System.Collections.Generic;

namespace Dnn.CommunityMetrics
{
    public interface IActivity
    {
        MetricTypeEnum MetricType { get; }
        List<ActivitySettingDTO> GetSettings();
        List<UserActivityDTO> GetUserActivity(ActivityDTO objActivity);
    }
}
