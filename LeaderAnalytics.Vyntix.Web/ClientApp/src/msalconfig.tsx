//----------------------------------------------------------------------
// This file is copied from src/Config at build time.   See .csproj file
//----------------------------------------------------------------------

/**
* Config object to be passed to MSAL on creation.
* For a full list of msal.js configuration parameters,
* visit https://azuread.github.io/microsoft-authentication-library-for-js/docs/msal/modules/_configuration_.html
**/ 



export const policies = {
    authority: "https://leaderanalytics.b2clogin.com/leaderanalytics.onmicrosoft.com/",
    userFlows: {
        signUpSignIn: "B2C_1_susi2",
        forgotPassword: "B2C_1_password_reset",
        editProfile: "B2C_1_edit_profile"
    }
}

export const msalConfig = {
    auth: {
        clientId: "7f892e9e-97d5-42fb-a553-f9d585d6742b",
        authority: "https://leaderanalytics.b2clogin.com/leaderanalytics.onmicrosoft.com/B2C_1_susi2",
        knownAuthorities: ["https://leaderanalytics.b2clogin.com"],
        redirectUri: "https://localhost:5031",
        postLogoutRedirectUri: "https://localhost:5031"
    }
};

// Scopes you add here will be prompted for consent during login
export const loginRequest = {
    scopes: ['https://LeaderAnalytics.onmicrosoft.com/eb373a05-0053-49c4-aba1-a7630fedfef7/read',
        'https://LeaderAnalytics.onmicrosoft.com/eb373a05-0053-49c4-aba1-a7630fedfef7/Write',
        'https://LeaderAnalytics.onmicrosoft.com/eb373a05-0053-49c4-aba1-a7630fedfef7/access_as_user']
};
