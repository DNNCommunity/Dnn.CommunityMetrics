dnnCommunityMetrics.controller('userActivityDetailController', ['$scope', '$q', '$uibModal', '$uibModalInstance', 'toastr', 'userActivityService', 'user_id', 'user_name', 'period_start', 'period_end', function ($scope, $q, $uibModal, $uibModalInstance, toastr, userActivityService, user_id, user_name, period_start, period_end) {

    $scope.user_id = user_id;
    $scope.user_name = user_name;
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
        userActivityService.aggregate(user_id, period_start, period_end).then(
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

    $scope.viewUserActivityDetail = function (user_activity) {
        var modalInstance = $uibModal.open({
            templateUrl: '/DesktopModules/Dnn.CommunityMetrics/app/views/user-activity/user-activity-detail2.html?c=' + new Date().getTime(),
            controller: 'userActivityDetail2Controller',
            size: 'lg',
            backdrop: 'static',
            resolve: {
                user_id: function () {
                    return $scope.user_id;
                },
                user_name: function () {
                    return user_name;
                },
                activity_id: function () {
                    return user_activity.activity_id;
                },
                activity_name: function () {
                    return user_activity.activity_name;
                },
                period_start: function () {
                    return $scope.period_start;
                },
                period_end: function () {
                    return $scope.period_end;
                }
            }
        });

        modalInstance.result.then(
            function () {
                $scope.getUserActivity();

            },
            function () {
                $scope.getUserActivity();
            }
        );

    };

    init = function () {
        var promises = [];
        promises.push($scope.getUserActivities());
        return $q.all(promises);
    };
    init();
}]);

