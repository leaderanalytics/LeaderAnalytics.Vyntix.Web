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
        authority: "https://leaderanalytics.onmicrosoft.com",
        knownAuthorities: ["https://leaderanalytics.b2clogin.com"],
        redirectUri: "https://localhost:5031",
        postLogoutRedirectUri: "https://localhost:5031"
    }
};

// Scopes you add here will be prompted for consent during login
export const loginRequest = {
    scopes: ['https://LeaderAnalytics.onmicrosoft.com/be098f0e-6e50-40c4-9a36-212762d209fe/read',
        'https://LeaderAnalytics.onmicrosoft.com/be098f0e-6e50-40c4-9a36-212762d209fe/access_as_user']
};
