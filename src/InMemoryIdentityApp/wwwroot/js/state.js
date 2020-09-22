var State = (function () {

    var supported = !!window.history;
    var enabled = true;
    var popCallbacks = [];
    var pushCallbacks = [];

    var initialURL = location.href;


    function triggerPopStates(state) {
        popCallbacks.forEach(function (c) {
            c(state);
        });
    }

    function triggerPushStates(state, title, href) {
        pushCallbacks.forEach(function (c) {
            c(state, title, href);
        });
    }

    if (supported) {

        // Initially replace a state.  This allows a popState event to fire when going back to initial state
        history.replaceState({}, document.title, window.location.href);
        window.onpopstate = function (e) {
            var state = e.state;
            if (state) {
                triggerPopStates(state);
            }
        }
      

    }
    return {
        enabled: enabled,
        disable: function () {
            enabled = false;
        },
        enable: function () {
            enabled = true;
        },
        pushState: function (state, title, href) {
            if (!supported || !enabled) {
                return;
            }
            history.pushState(state, title, href);
            triggerPushStates(state, title, href);
        },
        onPopState: function (f) {
            if (!supported || !enabled) {
                return;
            }
            popCallbacks.push(f);
        },
        onPushState: function (f) {
            if (!supported || !enabled) {
                return;
            }
            pushCallbacks.push(f);
        }
    };
})();