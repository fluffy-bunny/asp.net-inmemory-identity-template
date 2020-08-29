'use strict';

var fetchActivityMonitor = function () {
    var _this = this,
        _arguments = arguments;

    var library = {};
    var w = window;
    w._activityDetected= false;

    w._oldOpen = XMLHttpRequest.prototype.open;
    var onStateChange = function onStateChange(event) {
        if (event.currentTarget.readyState === 4) {
            library._onHttpMonitor(event.currentTarget.responseURL, event.currentTarget.status);
        }
    };
    XMLHttpRequest.prototype.open = function () {
        // when an XHR object is opened, add a listener for its readystatechange events
        _this.addEventListener('readystatechange', onStateChange);
        // run the real `open`
        w._oldOpen.apply(_this, _arguments);
    };
    if (!window.fetch.polyfill) {
        w._oldFetch = w.fetch;

        window.fetch = function (input, init) {
            return w._oldFetch(input, init).then(function (response) {
                library._onHttpMonitor(response.url, response.status);
                return response;
            });
        };
    }
    library.start = function (callback) {
        library._callback = callback;
        library.timer = setInterval(function () {
            library._onTimer();
        }, 5000);
    };
    library.stop = function () {
        library._callback = null;
        if (self.timer) {
            clearInterval(_this.timer);
        }
    };

    library._onHttpMonitor = function (url, status) {
        w._activityDetected = true;
    };
    library._onTimer = function () {
        if (w._activityDetected) {
            w._activityDetected = false;
            if (library._callback) {
                console.log("fetch monitor _callback");
                library._callback(); 
            }
        }
    };

    return library;
}();
