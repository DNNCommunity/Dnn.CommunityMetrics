dnnCommunityMetrics.controller('userActivityListController', ['$scope', '$q', '$uibModal', '$uibModalInstance', 'toastr', 'activityService', 'userActivityService', function ($scope, $q, $uibModal, $uibModalInstance, toastr, activityService, userActivityService) {

    $scope.close = function () {
        $uibModalInstance.dismiss('cancel');
    };

    $scope.loading = true;

    $scope.user_id = null;
    $scope.activity_id = null;
    $scope.period_start = null;
    $scope.period_end = null;

    $scope.activities = [];
    $scope.activity = null;
    $scope.user_activities = [];

    $scope.periodStartDatePicker = {
        isOpen: false
    };
    $scope.periodEndDatePicker = {
        isOpen: false
    };

    $scope.getUserActivity = function () {
        var deferred = $q.defer();

        if ($scope.user_id || $scope.activity_id || $scope.period_start || $scope.period_end) {
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
        }
        else {
            toastr.warning("Please provide some filter criteria to limit the search results.", "Need Filter");
        }
    };
    $scope.addUserActivity = function () {
        var modalInstance = $uibModal.open({
            templateUrl: '/DesktopModules/Dnn.CommunityMetrics/app/views/user-activity/user-activity-edit.html?c=' + new Date().getTime(),
            controller: 'userActivityEditController',
            size: 'lg',
            backdrop: 'static',
            resolve: {
                id: function () {
                    return null;
                }
            }
        });

        modalInstance.result.then(
            function () {
                if ($scope.user_activities.length > 0) {
                    $scope.getUserActivity();
                }
            },
            function () {
                if ($scope.user_activities.length > 0) {
                    $scope.getUserActivity();
                }
            }
        );

    };
    $scope.editUserActivity = function (id) {
        console.log(id);
        var modalInstance = $uibModal.open({
            templateUrl: '/DesktopModules/Dnn.CommunityMetrics/app/views/user-activity/user-activity-edit.html?c=' + new Date().getTime(),
            controller: 'userActivityEditController',
            size: 'lg',
            backdrop: 'static',
            resolve: {
                id: function () {
                    return id;
                }
            }
        });

        modalInstance.result.then(
            function () {
                if ($scope.user_activities.length > 0) {
                    $scope.getUserActivity();
                }
            },
            function () {
                if ($scope.user_activities.length > 0) {
                    $scope.getUserActivity();
                }
            }
        );

    };
    $scope.deleteUserActivity = function (user_activity) {
        var modalInstance = $uibModal.open({
            templateUrl: '/DesktopModules/Dnn.CommunityMetrics/app/views/user-activity/user-activity-delete.html?c=' + new Date().getTime(),
            controller: 'userActivityDeleteController',
            size: 'lg',
            backdrop: 'static',
            resolve: {
                user_activity: function () {
                    return user_activity;
                }
            }
        });

        modalInstance.result.then(
            function () {
                if ($scope.user_activities.length > 0) {
                    $scope.getUserActivity();
                }
            },
            function () {
                if ($scope.user_activities.length > 0) {
                    $scope.getUserActivity();
                }
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

    $scope.getCurrentDate = function () {
        return new Date();
    };

    $scope.getDateOneYearAgo = function () {
        var date = new Date();
        date.setDate(date.getDate() - 365);
        return date;
    };

    $scope.rollingYear = function () {
        $scope.period_start = $scope.getDateOneYearAgo();
        $scope.period_end = $scope.getCurrentDate();
        $scope.getUserActivity();
    };

    init = function () {
        var promises = [];
        promises.push($scope.getActivities());
        return $q.all(promises);
    };
    init();
    $scope.rollingYear();

}]);

