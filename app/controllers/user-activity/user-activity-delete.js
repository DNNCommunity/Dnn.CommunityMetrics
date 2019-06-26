dnnCommunityMetrics.controller('userActivityDeleteController', ['$scope', '$uibModalInstance', 'toastr', 'userActivityService', 'user_activity', function ($scope, $uibModalInstance, toastr, userActivityService, user_activity) {

    $scope.close = function () {
        $uibModalInstance.dismiss('cancel');
    };

    $scope.user_activity = user_activity;

    $scope.deleteUserActivity = function () {
        userActivityService.remove(user_activity.id).then(
            function () {
                toastr.success("The user activity was deleted.", "Success");
                $uibModalInstance.close();
            },
            function (response) {
                console.log('deleteUserActivity failed', response);
                toastr.error("There was a problem deleteing the user activity", "Error");
            }
        );
    };

}]);

