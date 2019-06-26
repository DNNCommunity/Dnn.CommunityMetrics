dnnCommunityMetrics.controller('userActivityDetail2Controller', ['$scope', '$q', '$uibModalInstance', 'toastr', 'userActivityService', 'user_id', 'user_name', 'activity_id', 'activity_name', 'period_start', 'period_end', function ($scope, $q, $uibModalInstance, toastr, userActivityService, user_id, user_name, activity_id, activity_name, period_start, period_end) {

    $scope.user_id = user_id;
    $scope.user_name = user_name;
    $scope.activity_id = activity_id;
    $scope.activity_name = activity_name;
    $scope.period_start = period_start;
    $scope.period_end = period_end;

    $scope.close = function () {
        $uibModalInstance.dismiss('cancel');
    };

    $scope.loading = true;
    $scope.user_activities = [];

    $scope.getUserActivities = function () {
        var deferred = $q.defer();
        $scope.loading = true;
        userActivityService.list($scope.user_id, $scope.activity_id, $scope.period_start, $scope.period_end).then(
            function (response) {
                $scope.user_activities = response.data;
                $scope.loading = false;
                deferred.resolve();
            },
            function (response) {
                console.log('getUserActivities failed', response);
                toastr.error("There was a problem loading the user activities.", "Error");
                $scope.loading = false;
                deferred.reject();
            }
        );
        return deferred.promise;
    };

    init = function () {
        var promises = [];
        promises.push($scope.getUserActivities());
        return $q.all(promises);
    };
    init();
}]);

