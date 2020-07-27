import * as MSAL from 'msal';
export default class MSALConfig {
    /**
     * Config object to be passed to MSAL on creation.
     * For a full list of msal.js configuration parameters, 
     * visit https://azuread.github.io/microsoft-authentication-library-for-js/docs/msal/modules/_configuration_.html
     * */

    public auth = {
        clientId: "7f892e9e-97d5-42fb-a553-f9d585d6742b",
        authority: "https://leaderanalytics.b2clogin.com/leaderanalytics.onmicrosoft.com/B2C_1_susi",
        validateAuthority: false
    };
    public cache = {
        CacheLocation: "localStorage", // LocalStorage allows single signon accross tabs.
        storeAuthStateInCookie: false // Set this to "true" to save cache in cookies to address trusted zones limitations in IE (see: https://github.com/AzureAD/microsoft-authentication-library-for-js/wiki/Known-issues-on-IE-and-Edge-Browser)
    };

    public system = {
        loggerOptions: {
            loggerCallback: (level: MSAL.LogLevel, message: string, containsPii: boolean): void => {
                if (containsPii) {
                    return;
                }
                switch (level) {
                    case MSAL.LogLevel.Error:
                        console.error(message);
                        return;
                    case MSAL.LogLevel.Info:
                        console.info(message);
                        return;
                    case MSAL.LogLevel.Verbose:
                        console.debug(message);
                        return;
                    case MSAL.LogLevel.Warning:
                        console.warn(message);
                        return;
                }
            },
            piiLoggingEnabled: false
        },
        windowHashTimeout: 60000,
        iframeHashTimeout: 6000,
        loadFrameTimeout: 0
    };
}