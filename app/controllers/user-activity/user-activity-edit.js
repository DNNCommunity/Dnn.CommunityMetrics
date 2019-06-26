dnnCommunityMetrics.controller('userActivityEditController', ['$scope', '$q', '$uibModal', '$uibModalInstance', 'toastr', 'userActivityService', 'activityService', 'id', function ($scope, $q, $uibModal, $uibModalInstance, toastr, userActivityService, activityService, id) {

    $scope.loading = false;

    $scope.close = function () {
        $uibModalInstance.dismiss('cancel');
    };
    $scope.submitted = false;

    $scope.user_activity = {
        id: id
    };

    $scope.activities = [];
    $scope.activity = null;

    $scope.datePicker = {
        isOpen: false
    };

    $scope.getUserActivity = function () {
        var deferred = $q.defer();
        $scope.loading = true;

        userActivityService.get($scope.user_activity.id).then(
            function (response) {
                console.log(response.data);
                $scope.user_activity = response.data;

                $scope.user_activity.date = new Date($scope.user_activity.date);

                activityService.get($scope.user_activity.activity_id).then(
                    function (response) {
                        $scope.activity = response.data;
                    },
                    function () { }
                );

                $scope.loading = false;
            },
            function (response) {
                console.log('getUserActivity failed', response);
                toastr.error("There was a problem loading the user activity", "Error");
                $scope.loading = false;
                deferred.reject();
            }
        );
        return deferred.promise;
    };
    $scope.saveUserActivity = function () {
        $scope.submitted = true;
        $scope.loading = true;

        var isNew = $scope.user_activity.id === null;

        if ($scope.formUserActivity.$valid) {

            userActivityService.save($scope.user_activity).then(
                function (response) {
                    $scope.user_activity = response.data;

                    $scope.loading = false;
                    $scope.submitted = false;

                    if (isNew) {
                        toastr.success("The User Activity was created.", "Success");
                    }
                    else {
                        toastr.success("The User Activity was saved.", "Success");
                    }
                    $uibModalInstance.close($scope.user_activity);
                },
                function (response) {
                    console.log('saveUserActivity failed', response);
                    toastr.error("There was a problem saving the user activity", "Error");
                    $scope.submitted = false;
                    $scope.loading = false;
                }
            );
        }
        else {
            $scope.loading = false;
            $('#formPoints').find('.ng-invalid:visible:first').focus();
        }
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

    $scope.calculatePoints = function () {
        if ($scope.activity !== null && $scope.user_activity.count !== null) {
            $scope.user_activity.points = $scope.activity.factor * $scope.user_activity.count;
        }
        else {
            $scope.user_activity.points = null;
        }
    };

    $scope.init = function () {
        var promises = [];
        promises.push($scope.getActivities());
        if ($scope.user_activity.id) {
            promises.push($scope.getUserActivity());
        }
        return $q.all(promises);
    };
    $scope.init();

}]);

