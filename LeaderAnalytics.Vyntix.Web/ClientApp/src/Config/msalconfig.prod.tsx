//----------------------------------------------------------------------
// This file is copied from src/Config at build time.   See .csproj file
//----------------------------------------------------------------------

export const msalConfig = {
    auth: {
        clientId: "f16f44ba-20c7-4fc0-a940-f70def6146eb",
        authority: "https://leaderanalytics.b2clogin.com/leaderanalytics.onmicrosoft.com/B2C_1_susi2",
        knownAuthorities: ["https://leaderanalytics.b2clogin.com"],
        redirectUri: "https://vyntix.com/",
        postLogoutRedirectUri: "https://vyntix.com/"
    }
};

// Scopes you add here will be prompted for consent during login
export const loginRequest = {
    scopes: ['https://LeaderAnalytics.onmicrosoft.com/api/read',
        'https://LeaderAnalytics.onmicrosoft.com/api/write',
        'https://LeaderAnalytics.onmicrosoft.com/api/access_as_user']
};
