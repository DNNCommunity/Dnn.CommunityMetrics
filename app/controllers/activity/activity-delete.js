dnnCommunityMetrics.controller('activityDeleteController', ['$scope', '$uibModalInstance', 'toastr', 'activityService', 'activity', function ($scope, $uibModalInstance, toastr, activityService, activity) {

    $scope.close = function () {
        $uibModalInstance.dismiss('cancel');
    };

    $scope.activity = activity;

    $scope.deleteActivity = function () {
        activityService.remove(activity.id).then(
            function () {
                toastr.success("The activity '" + activity.name + "' was deleted.", "Success");
                $uibModalInstance.close();
            },
            function (response) {
                console.log('deleteActivity failed', response);
                toastr.error("There was a problem deleteing the activity","Error");
            }
        );
    };

}]);

