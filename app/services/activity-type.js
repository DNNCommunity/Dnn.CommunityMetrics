dnnCommunityMetrics.factory('activityTypeService', ['$http', function activityTypeService($http) {

    var base_path = "/api/Dnn.CommunityMetrics/activityType";

    // interface
    var service = {
        list: list,
        get: get
    };
    return service;

    // implementation    

    // list 
    function list() {
        var request = $http({
            method: "get",
            url: base_path
        });
        return request;
    }

    // get
    function get(typeName) {
        var request = $http({
            method: "get",
            url: base_path + "?typeName=" + typeName
        });
        return request;
    }

}]);
