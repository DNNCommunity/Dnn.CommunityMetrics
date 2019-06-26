dnnCommunityMetrics.controller('activitySettingEditController', ['$scope', '$q', '$uibModalInstance', 'toastr', 'activitySettingService', 'setting', function ($scope, $q, $uibModalInstance, toastr, activitySettingService, setting) {

    $scope.loading = false;

    $scope.close = function () {
        $uibModalInstance.dismiss('cancel');
    };
    $scope.submitted = false;

    $scope.activitySetting = setting;

    $scope.saveActivitySetting = function () {
        $scope.submitted = true;
        $scope.loading = true;

        if ($scope.formActivitySetting.$valid) {

            activitySettingService.save($scope.activitySetting).then(
                function (response) {
                    $scope.activitySetting = response.data;

                    $scope.loading = false;
                    $scope.submitted = false;

                    toastr.success("The setting '" + $scope.activitySetting.name + "' was saved.", "Success");
                    $uibModalInstance.close($scope.activitySetting);
                },
                function (response) {
                    console.log('saveActivitySetting failed', response);
                    toastr.error("There was a problem saving the activity setting", "Error");
                    $scope.submitted = false;
                    $scope.loading = false;
                }
            );
        }
        else {
            $scope.loading = false;
            $('#formActivitySetting').find('.ng-invalid:visible:first').focus();
        }
    };

}]);

