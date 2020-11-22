//----------------------------------------------------------------------
    // This file is copied from src/Config at build time.   See .csproj file
    //----------------------------------------------------------------------
/**
 * Config object to be passed to MSAL on creation.
 * For a full list of msal.js configuration parameters,
 * visit https://azuread.github.io/microsoft-authentication-library-for-js/docs/msal/modules/_configuration_.html
 * */

import { Configuration, LogLevel } from "@azure/msal-browser";

export const POLICIES = {
    authority: "https://leaderanalytics.b2clogin.com/leaderanalytics.onmicrosoft.com/",
    userFlows: {
        signUpSignIn: "B2C_1_susi",
        forgotPassword: "B2C_1_password_reset",
        editProfile: "B2C_1_edit_profile"
    }
}

const MSAL_CONFIG: Configuration = {
    auth: {
        clientId: "f16f44ba-20c7-4fc0-a940-f70def6146eb",
        authority: "https://leaderanalytics.b2clogin.com/leaderanalytics.onmicrosoft.com/B2C_1_susi",
        redirectUri: "https://vyntix.com/",
        knownAuthorities: ["leaderanalytics.b2clogin.com"]
    },
    cache: {
        cacheLocation: "localStorage", // LocalStorage allows single signon accross tabs.
        storeAuthStateInCookie: false // Set this to "true" to save cache in cookies to address trusted zones limitations in IE (see: https://github.com/AzureAD/microsoft-authentication-library-for-js/wiki/Known-issues-on-IE-and-Edge-Browser)
    },

    system: {
        loggerOptions: {
            loggerCallback: (level: LogLevel, message: string, containsPii: boolean) => {
                if (containsPii) {
                    return;
                }
                switch (level) {
                    case LogLevel.Error:
                        console.error(message);
                        return;
                    case LogLevel.Info:
                        console.info(message);
                        return;
                    case LogLevel.Verbose:
                        console.debug(message);
                        return;
                    case LogLevel.Warning:
                        console.warn(message);
                        return;
                }
            }
        }
    }
};
export default MSAL_CONFIG;