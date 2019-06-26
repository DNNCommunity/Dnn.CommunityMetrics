dnnCommunityMetrics.controller('activityListController', ['$scope', '$q', '$uibModal', '$uibModalInstance', 'toastr', 'activityService', function ($scope, $q, $uibModal, $uibModalInstance, toastr, activityService) {

    $scope.close = function () {
        $uibModalInstance.dismiss('cancel');
    };

    $scope.loading = true;
    $scope.activities = [];

    $scope.getActivities = function () {
        var deferred = $q.defer();
        $scope.loading = true;
        activityService.list().then(
            function (response) {
                $scope.activities = response.data;
                $scope.loading = false;
                deferred.resolve();
            },
            function (response) {
                console.log('getActivities failed', response);
                toastr.error("There was a problem loading activities", "Error");
                $scope.loading = false;
                deferred.reject();
            }
        );
        return deferred.promise;
    };
    $scope.addActivity = function () {
        var modalInstance = $uibModal.open({
            templateUrl: '/DesktopModules/Dnn.CommunityMetrics/app/views/activity/activity-edit.html?c=' + new Date().getTime(),
            controller: 'activityEditController',
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
                $scope.getActivities();
            },
            function () {
                $scope.getActivities();
            }
        );

    };
    $scope.editActivity = function (id) {
        var modalInstance = $uibModal.open({
            templateUrl: '/DesktopModules/Dnn.CommunityMetrics/app/views/activity/activity-edit.html?c=' + new Date().getTime(),
            controller: 'activityEditController',
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
                $scope.getActivities();
            },
            function () {
                $scope.getActivities();
            }
        );

    };
    $scope.deleteActivity = function (activity) {
        var modalInstance = $uibModal.open({
            templateUrl: '/DesktopModules/Dnn.CommunityMetrics/app/views/activity/activity-delete.html?c=' + new Date().getTime(),
            controller: 'activityDeleteController',
            size: 'lg',
            backdrop: 'static',
            resolve: {
                activity: function () {
                    return activity;
                }
            }
        });

        modalInstance.result.then(
            function () {
                $scope.getActivities();
            },
            function () {
                $scope.getActivities();
            }
        );
    };

    init = function () {
        var promises = [];
        promises.push($scope.getActivities());
        return $q.all(promises);
    };
    init();
}]);

