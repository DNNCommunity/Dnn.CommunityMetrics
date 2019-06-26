dnnCommunityMetrics.factory('userActivityService', ['$http', '$filter', function userActivityService($http, $filter) {

    var base_path = "/api/Dnn.CommunityMetrics/userActivity";

    // interface
    var service = {
        list: list,
        aggregate: aggregate,
        report: report,
        get: get,
        insert: insert,
        update: update,
        remove: remove,
        save: save
    };
    return service;

    // implementation    

    // list
    function list(user_id = null, activity_id = null, period_start = null, period_end = null, skip = null, take = null) {

        if (period_start) {
            period_start = $filter('date')(period_start, 'MM/dd/yyyy');
        }
        if (period_end) {
            period_end = $filter('date')(period_end, 'MM/dd/yyyy');
        }

        var request = $http({
            method: "get",
            url: base_path + '?user_id=' + user_id + '&activity_id=' + activity_id + '&period_start=' + period_start + '&period_end=' + period_end + '&skip=' + skip + '&take=' + take
        });
        return request;
    }

    // aggregate
    function aggregate(user_id = null, period_start = null, period_end = null, skip = null, take = null) {

        if (period_start) {
            period_start = $filter('date')(period_start, 'MM/dd/yyyy');
        }
        if (period_end) {
            period_end = $filter('date')(period_end, 'MM/dd/yyyy');
        }

        var request = $http({
            method: "get",
            url: base_path + '?user_id=' + user_id + '&period_start=' + period_start + '&period_end=' + period_end + '&skip=' + skip + '&take=' + take
        });
        return request;
    }

    // report
    function report(user_search, period_start = null, period_end = null, skip = null, take = null) {

        if (period_start) {
            period_start = $filter('date')(period_start, 'MM/dd/yyyy');
        }
        if (period_end) {
            period_end = $filter('date')(period_end, 'MM/dd/yyyy');
        }

        var request = $http({
            method: "get",
            url: base_path + '?user_search=' + user_search + '&period_start=' + period_start + '&period_end=' + period_end + '&skip=' + skip + '&take=' + take
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
