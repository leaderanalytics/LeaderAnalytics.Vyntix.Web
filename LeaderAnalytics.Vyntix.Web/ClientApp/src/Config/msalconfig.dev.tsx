//----------------------------------------------------------------------
    // This file is copied from src/Config at build time.   See .csproj file
    //----------------------------------------------------------------------
/**
 * Config object to be passed to MSAL on creation.
 * For a full list of msal.js configuration parameters,
 * visit https://azuread.github.io/microsoft-authentication-library-for-js/docs/msal/modules/_configuration_.html
 * */
import { Configuration, LogLevel } from "@azure/msal-browser";
const MSAL_CONFIG: Configuration = {
    auth: {
        clientId: "7f892e9e-97d5-42fb-a553-f9d585d6742b",
        authority: "https://leaderanalytics.b2clogin.com/leaderanalytics.onmicrosoft.com/B2C_1_susi",
        redirectUri: "https://localhost:5031/",
        knownAuthorities: ["leaderanalytics.b2clogin.com"]
    },
    cache: {
        cacheLocation: "sessionStorage", // This configures where your cache will be stored
        storeAuthStateInCookie: false, // Set this to "true" if you are having issues on IE11 or Edge
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