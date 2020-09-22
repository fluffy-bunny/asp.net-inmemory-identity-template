/* eslint-disable no-console */
/* eslint-disable no-unused-vars */
'use strict';

function llLoadSpa(IFRAME_HOST) {
    var IFRAME_ID = 'dspIframe';
    var DEBUG = false;
    var previousScrollTop = 0;

    function log() {
        if (DEBUG) {
            console.log.apply(this, arguments);
        }
    }

    /**
     * Returns browser and version
     *
     */
    function getCurrentBrowser() {
        var ua = navigator.userAgent;
        var versionMatches;
        var browserMatches = ua.match(/(opera|chrome|safari|firefox|msie|trident(?=\/))\/?\s*(\d+)/i) || [];

        if (/trident/i.test(browserMatches[1])) {
            versionMatches = /\brv[ :]+(\d+)/g.exec(ua) || [];
            return { name: 'MSIE', version: versionMatches[1] || '' };
        }
        if (browserMatches[1] === 'Chrome') {
            versionMatches = ua.match(/\b(OPR|Edge)\/(\d+)/);
            if (versionMatches && versionMatches.length > 0 && versionMatches[1] === 'Edge') {
                return { name: 'Edge', version: versionMatches[2] };
            }
            if (versionMatches != null) {
                return { name: 'Opera', version: versionMatches[2] };
            }
        }
        browserMatches = browserMatches[2]
            ? [browserMatches[1], browserMatches[2]]
            : [navigator.appName, navigator.appVersion, '-?'];
        versionMatches = ua.match(/version\/(\d+)/i);
        if (versionMatches !== null) browserMatches.splice(1, 1, versionMatches[1]);

        if (browserMatches[0] && browserMatches[1]) {
            return { name: browserMatches[0], version: browserMatches[1] };
        }
    }

    /**
     * Returns true for supported or unrecognized user agents
     *
     */
    function supportedUserAgent(currentbrowser) {
        var minValidBrowser = {
            Chrome: 20,
            Firefox: 18,
            Safari: 10,
            MSIE: 11,
            Edge: 13,
        };
        if (
            (currentbrowser.name === 'Safari' || currentbrowser.name === 'MSIE') &&
            parseInt(currentbrowser.version, 10) < minValidBrowser[currentbrowser.name]
        ) {
            return false;
        }
        return true;
    }

    /**
     * Displays upgrade message in case of unsupported user agent
     */
    function displayUpgradePage() {
        var el = document.getElementById(IFRAME_ID);
        if (el) {
            var warning = document.createElement('div');
            warning.innerText =
                'Your web browser is out of date and must be updated to view this content. Please update your browser and try again.';
            el.parentNode.insertBefore(warning, el);
        }
    }

    /**
     * Splits hash by '/', encodes each segment, then rejoins
     * the encoded segments with '/'.
     *
     * @param hash
     * @returns {string}
     */
    function sanitizeHash(hash) {
        if (typeof hash !== 'string') {
            return '';
        }
        var parts = hash.split('/');
        for (var i = 0; i < parts.length; i++) {
            // encode the decoded value, to avoid encoding an already encoded string
            parts[i] = encodeURIComponent(decodeURIComponent(parts[i]));
        }
        return parts.join('/');
    }

    /**
     * Returns query string with name and value portions uri encoded.
     * @param str
     * @returns {string}
     */
    function sanitizeQueryString(str) {
        var sections = str.split('&');
        for (var i = 0; i < sections.length; i++) {
            var nameValuePair = sections[i].split('=');
            for (var j = 0; j < nameValuePair.length; j++) {
                // encode the decoded value, to avoid encoding an already encoded string
                nameValuePair[j] = encodeURIComponent(decodeURIComponent(nameValuePair[j]));
            }
            sections[i] = nameValuePair.join('=');
        }
        return sections.join('&');
    }

    /**
     * Sets iframe url using IFRAME_HOST and window.location.hash.
     * Supports use case where user visits a bookmark created after navigating around the iframe.
     */
    function setIframeUrl() {
        if (!supportedUserAgent(getCurrentBrowser())) {
            return displayUpgradePage();
        }

        var url = IFRAME_HOST;
        var queryString = getQueryString();
        if (window.location.hash) {
            url = url + '#' + sanitizeHash(window.location.hash.slice(1));
        } else {
            url = url + '#/'; // so hashrouter will work
        }
        url = url + queryString;
        log('setIframeUrl, url =', url);
        document.getElementById(IFRAME_ID).src = url;
    }

    function getQueryString() {
        var queryString = window.location.href;
        if (queryString.indexOf('?') !== -1) {
            queryString = queryString.substr(queryString.indexOf('?') + 1);
        } else {
            queryString = '';
        }
        if (queryString.indexOf('#') !== -1) {
            // remove the hash portion
            queryString = queryString.slice(0, queryString.indexOf('#'));
        }
        return '?' + sanitizeQueryString(queryString);
    }

    // Updates window location hash
    function updateLocation(hash) {
        if (window.location.hash.substr(1) !== hash && history.replaceState) {
            history.replaceState(
                {
                    hash: hash,
                },
                '',
                '?redirectUrl=' + encodeURIComponent(hash) + '#' + hash
            );
        }
    }

    // Runs logic that must wait until the app in the iframe has finished initializing.
    function onAppInitialized(payload) {
        log('onAppInitialized, payload=', payload);
        sendInit();
    }

    // Sends message to iframe telling it to start NSL handshake
    function sendInit() {
        log('sendInit');
        let ngpUserHash = '';
        let ngpSessionHash = '';
        let viewBagRecordObject = undefined;
        try {
            if (window.viewBagRecord && typeof window.viewBagRecord === 'string') {
                viewBagRecordObject = JSON.parse(window.viewBagRecord);
            }
        } catch (e) {
            // parse exception, do nothing
        }
        if (typeof viewBagRecordObject === 'object') {
            ngpUserHash = viewBagRecordObject.UserHash;
            ngpSessionHash = viewBagRecordObject.SessionHash;
        }

        document.getElementById(IFRAME_ID).contentWindow.postMessage(
            {
                type: 'INIT',
                payload: JSON.stringify({ ngpUserHash: ngpUserHash, ngpSessionHash: ngpSessionHash }),
            },
            IFRAME_HOST
        );
    }

    // Sends message with logout command to iframe
    /*
    function sendLogoutMessage() {
      document.getElementById(IFRAME_ID).contentWindow.postMessage({
        type: 'LOGOUT',
        payload: 'parent telling iframe to log out'
      }, IFRAME_HOST);
    }*/

    // Sends message containing new location hash to iframe (workaround to blocker on cross origin iframe access)
    function sendLocationHash(locationHash, queryString) {
        // n.b. when the user hits the back button, sendLocationHash will be triggered.  Sending postMessage to the contentWindow then
        // prevents the user from clicking the forward button

        document.getElementById(IFRAME_ID).contentWindow.postMessage(
            {
                type: 'LOCATION_HASH_CHANGE',
                payload: { locationHash: locationHash, queryString: queryString },
            },
            IFRAME_HOST
        );
    }

    // Adjust DSPIframe's height according to its content
    function adjustIframeMinHeight(iframeMinHeight) {
        document.getElementById(IFRAME_ID).style.minHeight = iframeMinHeight + 'px';
        if (window.notifySPAContainer) {
            //log('need to notify window.notifySPAContainer');
            window.notifySPAContainer({ action: 'resize', sender: 'reactone', payload: { height: iframeMinHeight } });
        } else {
            log('window.notifySPAContainer does not exist');
        }
    }

    function toggleBodyOverflow(isModalOpened) {
        if (isModalOpened) {
            if (/iP(hone|od|ad)/.test(navigator.platform)) {
                document.body.style.position = 'absolute';
                document.body.style.width = '100%';
                document.body.style.top = '0px';
            }
            document.body.style.overflow = 'hidden';
        } else {
            document.body.style.overflow = '';
            document.body.style.position = '';
            document.body.style.width = '';
        }
    }

    function getIframeMinHeight() {
        document.getElementById(IFRAME_ID).contentWindow.postMessage(
            {
                type: 'GET_IFRAME_MIN_HEIGHT',
                payload: {
                    minHeight: document.getElementById(IFRAME_ID).style.minHeight,
                },
            },
            IFRAME_HOST
        );
    }

    function getOffSetTop(el) {
        var elOffsetTop = 0;
        while (el) {
            elOffsetTop += el.offsetTop - el.scrollTop + el.clientTop;
            el = el.offsetParent;
        }
        return elOffsetTop;
    }

    /** Returns the IOS version */
    function iosVersion() {
        if (/iP(hone|od|ad)/.test(navigator.platform)) {
            var v = navigator.appVersion.match(/OS (\d+)_(\d+)_?(\d+)?/);
            return [parseInt(v[1], 10), parseInt(v[2], 10), parseInt(v[3] || 0, 10)].join('.');
        }
    }

    function sendWindowSpec(payload) {
        var header = document.querySelector('header');
        var footer = document.querySelector('footer');
        if (iosVersion() <= '11.4.0') {
            previousScrollTop = window.pageYOffset >= document.body.scrollHeight ? previousScrollTop : window.pageYOffset;
        } else {
            previousScrollTop = window.pageYOffset;
        }
        document.getElementById(IFRAME_ID).contentWindow.postMessage(
            {
                type: 'GET_WINDOW_SPEC',
                payload: {
                    headerHeight: header ? header.getBoundingClientRect().height : 0,
                    footerHeight: footer ? footer.getBoundingClientRect().height : 0,
                    windowHeight: window.innerHeight,
                    windowWidth: window.innerWidth,
                    scrollTop: previousScrollTop,
                    scrollHeight: document.body.scrollHeight,
                    offsetTop: document.getElementById(IFRAME_ID).offsetTop,
                    actionToTake: payload.actionToTake,
                },
            },
            IFRAME_HOST
        );
    }

    // Handler for receiving messages from iframe
    function receiveMessage(event) {
        log('receiveMessage, event =', event);
        if (event.origin !== IFRAME_HOST) {
            log('receiveMessage, ignored event =', event);
            return;
        }

        if (!event.data.type) {
            throw new Error('event data missing type information');
        }
        switch (event.data.type) {
            case 'LOGGED_OUT':
                // when SPA encounters NSL logged out state, it should reauthenticate
                log('receiveMessage, payload=', event.data.payload);
                window.location.href =
                    '/extspa/home/reauthenticate?returnurl=' + encodeURIComponent(window.location.pathname + event.data.payload);
                break;
            case 'LOCATION_CHANGED':
                updateLocation(event.data.payload);
                break;
            case 'APP_LOADED':
                onAppInitialized(event.data.payload);
                break;
            case 'GET_PARENT_WINDOW_SPEC':
                sendWindowSpec(event.data.payload);
                break;
            case 'RESIZED':
                adjustIframeMinHeight(event.data.payload);
                break;
            case 'MODAL_OPENED':
                toggleBodyOverflow(event.data.payload);
                break;
            case 'SCROLL_PARENT':
                if (typeof event.data.payload !== undefined && event.data.payload !== window.pageYOffset) {
                    window.scrollTo(0, event.data.payload);
                }
                break;
            case 'KEEP_NGP_ALIVE':
                if (window.notifySPAContainer) {
                    window.notifySPAContainer({ action: 'keepalive', sender: 'reactone' });
                } else {
                    log('window.notifySPAContainer does not exist');
                }
                break;
            case 'OPEN_CREDIT_REPORT':
                if (
                    event.data.payload.reportFullUrl &&
                    (event.data.payload.reportFullUrl.indexOf('https://uat-api.consumer.equifax.com') === 0 ||
                        event.data.payload.reportFullUrl.indexOf('https://api.consumer.equifax.com') === 0)
                ) {
                    window.location.href = event.data.payload.reportFullUrl;
                }
                break;
            case 'CHANGE_WINDOW_LOCATION':
                window.location.href = event.data.payload;
                break;
            case 'RECORDSPALOAD':
                log('parent received RECORDSPALOAD, spa=', event.data.spa);
                break;
            case 'REQUEST_MIN_HEIGHT':
                getIframeMinHeight();
                break;
            default:
                break;
        }
    }

    window.addEventListener('hashchange', function () {
        // make sure iframe hash and window hash are in sync
        log('parent window hashchange, new hash =', window.location.hash);
        sendLocationHash(window.location.hash.substr(1), getQueryString()); // dont send the leading # symbol
    });
    window.addEventListener('message', receiveMessage, false);
    window.addEventListener('load', setIframeUrl);
    window.onresize = sendWindowSpec;
}

/* eslint-enable no-unused-vars */
/* eslint-enable no-console */