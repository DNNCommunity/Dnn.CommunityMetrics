dnnCommunityMetrics.controller('dashboardController', ['$scope', '$q', 'toastr', '$uibModal', 'userActivityService', 'activityService', function ($scope, $q, toastr, $uibModal, userActivityService, activityService) {

    $scope.period_start = new Date();
    $scope.period_end = new Date();
    $scope.skip = 0;
    $scope.take = 50;
    $scope.user_search = '';

    $scope.user_id = user_id;

    $scope.periodStartDatePicker = {
        isOpen: false
    };

    $scope.periodEndDatePicker = {
        isOpen: false
    };

    $scope.getUserActivity = function () {
        var deferred = $q.defer();
        $scope.loading = true;

        userActivityService.report($scope.user_search, $scope.period_start, $scope.period_end, $scope.skip, $scope.take).then(
            function (response) {
                $scope.user_activities = response.data;
                $scope.loading = false;
                deferred.resolve();
            },
            function (response) {
                console.log('getUserActivity failed', response);
                toastr.error("There was a problem loading the Dashboard", "Error");
                $scope.loading = false;
                deferred.reject();
            }
        );
        return deferred.promise;
    };

    $scope.thisYear = function () {
        var now = new Date();
        var year = now.getFullYear();

        $scope.period_start = new Date(year, 0, 1);
        $scope.period_end = new Date(year, 11, 31);

        $scope.getUserActivity();
    };

    $scope.lastYear = function () {
        var now = new Date();
        var year = now.getFullYear() - 1;

        $scope.period_start = new Date(year, 0, 1);
        $scope.period_end = new Date(year, 11, 31);

        $scope.getUserActivity();
    }

    $scope.thisMonth = function () {
        var now = new Date();
        var year = now.getFullYear();
        var month = now.getMonth();

        $scope.period_start = new Date(year, month, 1);

        $scope.period_end = new Date(year, month + 1, 1);
        $scope.period_end.setDate($scope.period_end.getDate() - 1);

        $scope.getUserActivity();
    };

    $scope.lastMonth = function () {
        var now = new Date();
        var year = now.getFullYear();
        var month = now.getMonth();

        $scope.period_start = new Date(year, month - 1, 1);

        $scope.period_end = new Date(year, month, 1);
        $scope.period_end.setDate($scope.period_end.getDate() - 1);

        $scope.getUserActivity();
    };

    $scope.allTime = function () {
        $scope.period_start = null;
        $scope.period_end = null;

        $scope.getUserActivity();
    };

    $scope.viewUserActivityDetail = function (user_activity) {
        var modalInstance = $uibModal.open({
            templateUrl: '/DesktopModules/Dnn.CommunityMetrics/app/views/user-activity/user-activity-detail.html?c=' + new Date().getTime(),
            controller: 'userActivityDetailController',
            size: 'lg',
            backdrop: 'static',
            resolve: {
                user_id: function () {
                    return user_activity.user_id;
                },
                user_name: function () {
                    return user_activity.user_name;
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

    $scope.getActivities = function () {
        var deferred = $q.defer();
        $scope.loading = true;

        activityService.list().then(
            function (response) {
                $scope.activities = response.data;
                $scope.loading = false;
            },
            function (response) {
                console.log('getActivities failed', response);
                toastr.error("There was a problem loading the activities", "Error");
                $scope.loading = false;
                deferred.reject();
            }
        );
        return deferred.promise;
    };

    init = function () {
        var promises = [];
        promises.push($scope.getActivities());
        return $q.all(promises);
    };

    init();
    $scope.thisYear();

}]);

