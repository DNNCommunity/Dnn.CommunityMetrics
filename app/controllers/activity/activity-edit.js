dnnCommunityMetrics.controller('activityEditController', ['$scope', '$q', '$uibModal', '$uibModalInstance', 'toastr', 'activityService', 'activityTypeService', 'activitySettingService', 'id', function ($scope, $q, $uibModal, $uibModalInstance, toastr, activityService, activityTypeService, activitySettingService, id) {

    $scope.loading = false;

    $scope.close = function () {
        $uibModalInstance.dismiss('cancel');
    };
    $scope.submitted = false;

    $scope.activityTypes = [];

    $scope.activity = {
        id: id
    };

    $scope.getActivityTypes = function () {
        var deferred = $q.defer();
        $scope.loading = true;

        activityTypeService.list().then(
            function (response) {
                $scope.activityTypes = response.data;
                $scope.loading = false;
            },
            function (response) {
                console.log('getActivityTypes failed', response);
                toastr.error("There was a problem loading the activity types", "Error");
                $scope.loading = false;
                deferred.reject();
            }
        );
        return deferred.promise;
    };

    $scope.getActivitySettings = function () {
        var deferred = $q.defer();
        $scope.loading = true;

        activitySettingService.list($scope.activity.type_name, $scope.activity.id).then(
            function (response) {
                $scope.settings = response.data;
                $scope.loading = false;
            },
            function (response) {
                console.log('getActivitySettings failed', response);
                toastr.error("There was a problem loading the activity settings", "Error");
                $scope.loading = false;
                deferred.reject();
            }
        );
        return deferred.promise;
    };
    $scope.typeChanged = function () {
        $scope.getActivitySettings();
    };

    $scope.getActivity = function () {
        var deferred = $q.defer();
        $scope.loading = true;

        activityService.get($scope.activity.id).then(
            function (response) {
                $scope.activity = response.data;

                $scope.getActivitySettings();

                $scope.loading = false;
            },
            function (response) {
                console.log('getActivity failed', response);
                toastr.error("There was a problem loading the activity", "Error");
                $scope.loading = false;
                deferred.reject();
            }
        );
        return deferred.promise;
    };
    $scope.saveActivity = function () {
        $scope.submitted = true;
        $scope.loading = true;

        var isNew = $scope.activity.id === null;

        if ($scope.formActivity.$valid) {

            activityService.save($scope.activity).then(
                function (response) {
                    $scope.activity = response.data;

                    $scope.loading = false;
                    $scope.submitted = false;

                    if (isNew) {
                        toastr.success("The Activity '" + $scope.activity.name + "' was created.", "Success");
                    }
                    else {
                        toastr.success("The Activity '" + $scope.activity.name + "' was saved.", "Success");
                        $uibModalInstance.close($scope.activity);
                    }                    
                },
                function (response) {
                    console.log('saveActivity failed', response);
                    toastr.error("There was a problem saving the activity", "Error");
                    $scope.submitted = false;
                    $scope.loading = false;
                }
            );
        }
        else {
            $scope.loading = false;
            $('#formActivity').find('.ng-invalid:visible:first').focus();
        }
    };

    $scope.editSetting = function (setting) {

        var clone = angular.copy(setting);
        clone.activity_id = $scope.activity.id;

        var modalInstance = $uibModal.open({
            templateUrl: '/DesktopModules/Dnn.CommunityMetrics/app/views/activity-setting/activity-setting-edit.html?c=' + new Date().getTime(),
            controller: 'activitySettingEditController',
            size: 'lg',
            backdrop: 'static',
            resolve: {
                setting: function () {
                    return clone;
                }
            }
        });

        modalInstance.result.then(
            function () {
                $scope.getActivitySettings();
            },
            function () {
                $scope.getActivitySettings();
            }
        );
    };

    $scope.init = function () {
        var promises = [];
        promises.push($scope.getActivityTypes());
        if ($scope.activity.id) {
            promises.push($scope.getActivity());
        }
        return $q.all(promises);
    };
    $scope.init();

}]);

