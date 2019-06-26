dnnCommunityMetrics.controller('adminController', ['$scope', '$uibModal', function ($scope, $uibModal) {

    $scope.viewActivities = function () {
        $uibModal.open({
            templateUrl: '/DesktopModules/Dnn.CommunityMetrics/app/views/activity/activity-list.html?c=' + new Date().getTime(),
            controller: 'activityListController',
            size: 'lg',
            backdrop: 'static'
        });
    };

    $scope.viewUserActivity = function () {
        $uibModal.open({
            templateUrl: '/DesktopModules/Dnn.CommunityMetrics/app/views/user-activity/user-activity-list.html?c=' + new Date().getTime(),
            controller: 'userActivityListController',
            size: 'xl',
            backdrop: 'static'
        });
    };

}]);

