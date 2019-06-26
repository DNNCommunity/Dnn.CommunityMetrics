dnnCommunityMetrics.factory('activitySettingService', ['$http', function activitySettingService($http) {

    var base_path = "/api/Dnn.CommunityMetrics/activitySetting";

    // interface
    var service = {
        list: list,
        get: get,
        insert: insert,
        update: update,
        remove: remove,
        save: save
    };
    return service;

    // implementation    

    // list
    function list(type_name, activity_id) {
        var request = $http({
            method: "get",
            url: base_path + "?typeName=" + type_name + "&activity_id=" + activity_id
        });
        return request;
    }

    // get 
    function get(id) {
        var request = $http({
            method: "get",
            url: base_path + '/' + id
        });
        return request;
    }

    // insert
    function insert(item) {
        var request = $http({
            method: "post",
            url: base_path,
            data: item
        });
        return request;
    }

    // update 
    function update(item) {
        var request = $http({
            method: "put",
            url: base_path,
            data: item
        });
        return request;
    }

    // delete
    function remove(id) {
        var request = $http({
            method: "delete",
            url: base_path + '/' + id
        });
        return request;
    }

    // save
    function save(item) {
        if (item.id > 0) {
            return update(item);
        }
        else {
            return insert(item);
        }
    }

}]);
