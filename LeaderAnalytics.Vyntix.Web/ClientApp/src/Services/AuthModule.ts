import { PublicClientApplication, AuthorizationUrlRequest, SilentRequest, AuthenticationResult, Configuration, LogLevel, AccountInfo, InteractionRequiredAuthError, EndSessionRequest, RedirectRequest, PopupRequest } from "@azure/msal-browser";
import { SsoSilentRequest } from "@azure/msal-browser/dist/src/request/SsoSilentRequest";
import MSAL_CONFIG from '../msalconfig';

export class AccountInfoClass implements AccountInfo
{
    homeAccountId: string = "";
    environment: string = "";
    tenantId: string = "";
    username: string = "";
    localAccountId: string = "";
    name?: string | undefined;
}
export class PopupRequestClass implements PopupRequest
{
    scopes: string[] = [];
    claims?: string | undefined;
    authority?: string | undefined;
    correlationId?: string | undefined;
    resourceRequestMethod?: string | undefined;
    resourceRequestUri?: string | undefined;
    authenticationScheme?: import("@azure/msal-browser").AuthenticationScheme | undefined;
    redirectUri?: string | undefined;
    extraScopesToConsent?: string[] | undefined;
    responseMode?: import("@azure/msal-common").ResponseMode | undefined;
    codeChallenge?: string | undefined;
    codeChallengeMethod?: string | undefined;
    state?: string | undefined;
    prompt?: string | undefined;
    account?: AccountInfo | undefined;
    loginHint?: string | undefined;
    domainHint?: string | undefined;
    sid?: string | undefined;
    extraQueryParameters?: import("@azure/msal-common").StringDict | undefined;
    nonce?: string | undefined;
}

export class SsoSilentRequestClass implements SsoSilentRequest
{

}

export class SilentRequestClass implements SilentRequest
{
    scopes: string[] = [];
    claims?: string | undefined;
    authority?: string | undefined;
    correlationId?: string | undefined;
    resourceRequestMethod?: string | undefined;
    resourceRequestUri?: string | undefined;
    account: AccountInfo = new AccountInfoClass();
    forceRefresh?: boolean | undefined;
    redirectUri?: string | undefined;
    extraQueryParameters?: import("@azure/msal-common").StringDict | undefined;
}

export class AuthenticationResultClass implements AuthenticationResult
{
    uniqueId: string = "";
    tenantId: string = "";
    scopes: string[] = [];
    account: AccountInfo = new AccountInfoClass();
    idToken: string = "";
    idTokenClaims: object = {};
    accessToken: string = "";
    fromCache: boolean = false;
    expiresOn: Date = new Date();
    tokenType: string = "";
    extExpiresOn?: Date | undefined;
    state?: string | undefined;
    familyId?: string | undefined;
}

export class AuthModule {

    private myMSALObj: PublicClientApplication;                              // https://azuread.github.io/microsoft-authentication-library-for-js/ref/msal-browser/classes/_src_app_publicclientapplication_.publicclientapplication.html
    private account: AccountInfoClass | null ;                              // https://azuread.github.io/microsoft-authentication-library-for-js/ref/msal-common/modules/_src_account_accountinfo_.html
    private loginRedirectRequest: RedirectRequest | null = null;            // https://azuread.github.io/microsoft-authentication-library-for-js/ref/msal-browser/modules/_src_request_redirectrequest_.html
    private loginRequest: PopupRequestClass;                                // https://azuread.github.io/microsoft-authentication-library-for-js/ref/msal-browser/modules/_src_request_popuprequest_.html
    private profileRedirectRequest: RedirectRequest | null = null;
    private profileRequest: PopupRequest | null = null;
    private mailRedirectRequest: RedirectRequest | null = null;
    private mailRequest: PopupRequest | null = null;
    private silentProfileRequest: SilentRequestClass;                     // https://azuread.github.io/microsoft-authentication-library-for-js/ref/msal-browser/modules/_src_request_silentrequest_.html
    private silentMailRequest: SilentRequestClass;
    private silentLoginRequest: SsoSilentRequestClass ;

    constructor() {
        this.myMSALObj = new PublicClientApplication(MSAL_CONFIG);
        this.account = null;
        this.setRequestObjects();
        this.loginRequest = new PopupRequestClass();
        this.silentLoginRequest = new SsoSilentRequestClass();
        this.silentMailRequest = new SilentRequestClass();
        this.silentProfileRequest = new SilentRequestClass();
    }

    /**
     * Initialize request objects used by this AuthModule.
     */
    private setRequestObjects(): void {
        this.loginRequest = {
            scopes: []
        };

        this.loginRedirectRequest = {
            ...this.loginRequest,
            redirectStartPage: window.location.href
        };

        this.profileRequest = {
            scopes: ["User.Read"]
        };

        this.profileRedirectRequest = {
            ...this.profileRequest,
            redirectStartPage: window.location.href
        };

        // Add here scopes for access token to be used at MS Graph API endpoints.
        this.mailRequest = {
            scopes: ["Mail.Read"]
        };

        this.mailRedirectRequest = {
            ...this.mailRequest,
            redirectStartPage: window.location.href
        };

        this.silentProfileRequest = {
            scopes: ["openid", "profile", "User.Read"],
            account: new AccountInfoClass(),
            forceRefresh: false
        };

        this.silentMailRequest = {
            scopes: ["openid", "profile", "Mail.Read"],
            account: new AccountInfoClass(),
            forceRefresh: false
        };

        this.silentLoginRequest = {
            loginHint: "IDLAB@msidlab0.ccsctp.net"
        }
    }

    /**
     * Calls getAllAccounts and determines the correct account to sign into, currently defaults to first account found in cache.
     * TODO: Add account chooser code
     * 
     * https://github.com/AzureAD/microsoft-authentication-library-for-js/blob/dev/lib/msal-common/docs/Accounts.md
     */
    private getAccount(): AccountInfo  {
        // need to call getAccount here?
        const currentAccounts = this.myMSALObj.getAllAccounts();

        if (currentAccounts.length > 1) {
            // Add choose account code here
            console.log("Multiple accounts detected, need to add choose account code.");
            return currentAccounts[0];
        }
        return currentAccounts[0];
    }

    /**
     * Calls loginPopup or loginRedirect based on given signInType.
     * @param signInType 
     */
    async login(signInType: string): Promise<AuthenticationResult>  {
        var response: AuthenticationResult = new AuthenticationResultClass();

        if (signInType === "loginPopup") {
            response = await this.myMSALObj.loginPopup(this.loginRequest);
            this.account = response.account;
        }
        return response;
    }

    /**
     * Logs out of current account.
     */
    logout(): void {
        const logOutRequest: EndSessionRequest = {
            account: this.account as AccountInfo
        };

        this.myMSALObj.logout(logOutRequest);
    }

    
}
