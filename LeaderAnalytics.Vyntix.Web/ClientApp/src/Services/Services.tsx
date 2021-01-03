import { string, number } from "prop-types";
import SubscriptionPlan from "../Model/SubscriptionPlan";
import CorpSubscriptionInfo from "../Model/CorpSubscriptionInfo";
import { GlobalContext, AppState } from '../AppState';
import AppConfig from '../appconfig';
import ContactRequest from '../Model/ContactRequest';
import AsyncResult from '../Model/AsyncResult';
import AppInsights from './AppInsights';
import { AuthModule } from './AuthModule';
import { AccountInfo } from "@azure/msal-browser";
import UserRecord from "../Model/UserRecord";



const MSAL = new AuthModule();

// Get active subscription plans
export const GetSubscriptionPlans = async (): Promise<SubscriptionPlan[]> => {
    const url = AppConfig.host + 'Subscription/GetActiveSubscriptionPlans';
    const response = await fetch(url);
    const json = await response.json();
    var result: SubscriptionPlan[] = new Array<SubscriptionPlan>();

    for (let i = 0; i < json.length; i++) {
        const p = DeserializeSubscriptionPlan(json[i]);

        if (p !== null)
            result.push(p);
    }
    return result;
}

export const SignIn = async (appState: AppState): Promise<string> => {

    if (appState.UserID != null && appState.UserID.length > 1) {
        return 'You are already signed in.  Sign out before signing in again.';
    }
    
    var msg: string = '';

    try {
        const response = await MSAL.login('loginPopup');

        if (response.uniqueId.length > 1 && response.account.username.length > 1)
        {
            // Get claims.  See Azure AD B2C -> User flows -> B2C_1_susi -> Application claims
            const claims: any = response.idTokenClaims;

            // Check if user is banned or suspended

            if (claims?.extension_IsBanned ?? false)
                msg = "This login has been permanently banned.";
            else if (claims?.extension_SuspendedUntil !== null && typeof claims.extension_SuspendedUntil !== 'undefined') {
                const suspensionDate = new Date(claims.extension_SuspendedUntil.substring(1));  // strip off leading "z" that we use to prevent Azure from converting to ticks.  Results in local time zone
                const now = new Date();

                if (now < suspensionDate)
                    msg = "This account is suspended until " + suspensionDate.toLocaleDateString() + " " + suspensionDate.toLocaleTimeString();
            }


            if (IsNullOrEmpty(msg)) {

                // Success

                appState.UserName = response.account.name ?? "";
                appState.UserID = response.uniqueId;
                appState.UserEmail = response.account.username;
                appState.ID_Token = response.idToken;
                appState.AccessToken = response.accessToken;
                appState.BillingID = claims.extension_BillingID;
                appState.IsCorpAdmin = (claims.extension_IsCorporateAdmin as boolean) ?? false;
                appState.IsOptIn = (claims.extension_IsOptIn as boolean) ?? false;
                const isNew: boolean = (response.idTokenClaims as any)?.newUser ?? false;  // not an extension
                SaveAppState(appState);
                AppInsights.LogEvent("Login Success", { "email": appState.UserEmail, "IsNew": isNew })
                Log("Login Success Account Name: " + appState.UserName + " Email: " + appState.UserEmail);
            }
        }
    }
    catch (err) {
        // login dialog throws if user cancels
        AppInsights.LogEvent("Login Failure", { "error": err })
        Log("LOGIN FAILURE Error Msg: " + err);
        msg = 'The login attempt was not successful.';
    }

    if (msg.length === 0)
        await GetSubscriptionInfo(appState);

    return msg;
}

export const SignOut = (appState: AppState) => {
    AppInsights.LogEvent("Sign Out", { "email": appState.UserEmail });
    MSAL.logout();
    appState.UserName = "";
    appState.UserID = "";
    appState.UserEmail = "";
    appState.BillingID = "";
    appState.CustomerID = "";
    appState.SubscriptionID = "";
    appState.ID_Token = "";
    appState.AccessToken = "";
    appState.PromoCodes = "";
    appState.SubscriptionPlan = null;
    appState.IsCorpAdmin = false;
    appState.IsOptIn = false;
    appState.CorpSubscriptionID = "";
    appState.CorpAdminEmail = "";
    localStorage.removeItem('appState');
}

export const SaveAppState = (appState: AppState) => {
    appState.TimeStamp = Date.now();
    localStorage.setItem('appState', JSON.stringify(appState));
}

export const GetAppState = (): AppState => {

    var s = localStorage.getItem('appState');
    var appState: AppState = (s === null || s.length === 0) ? new AppState() : JSON.parse(s);

    if (Date.now() - appState.TimeStamp > 3600000 && (appState.ID_Token?.length ?? 0) > 1) {
        SignOut(appState); 
        return GetAppState();
    }
    appState.SubscriptionPlan = SubscriptionPlan.Create(appState.SubscriptionPlan);
    return appState;
}

export const FormatMoney = (num: number) => {
    var formatter = new Intl.NumberFormat('en-US', {
        style: 'currency',
        currency: 'USD',
    });

    return formatter.format(num); 
}

export const SendContactRequest = async (request: ContactRequest): Promise<AsyncResult> => {
    AppInsights.LogEvent("SendContactRequest")
    const url = AppConfig.host + 'email/SendContactRequest';
    const msg = 'From Site:LeaderAnalytics.Vyntix.Web' + '\r\nName: ' + request.Name + '\r\nPhone: ' + request.Phone + '\r\nEmail: ' + request.EMail + '\r\nRequirement: ' + request.Requirement + '\r\nComment: ' + request.Message;
    AppInsights.LogEvent("SendContactRequest", {"Message": msg});

    let response = await fetch(url, {
        method: 'POST',
        headers: {
            'Content-Type': 'application/json;charset=utf-8'
        },
        body: JSON.stringify({ "To": "leaderanalytics@outlook.com", "Msg": msg, "CaptchaCode": request.Captcha })
    });

    const result: AsyncResult = new AsyncResult();
    result.Success = response.status < 300;

    if (!result.Success) {
        result.ErrorMessage = await response.json();
        AppInsights.LogEvent("SendContactRequest failed", { "ErrorMessage": result.ErrorMessage })
    }
    return result;
}

export const ManageSubscription = async (appState: AppState): Promise<AsyncResult> => {

    const result: AsyncResult = new AsyncResult();

    if ((appState?.CustomerID?.length ?? 0) === 0) {
        result.ErrorMessage = "Invalid CustomerID";
        return result;
    }
    AppInsights.LogEvent("ManageSubscription", { "Email": appState.UserEmail });
    const url = AppConfig.host + 'subscription/managesubscription';

    let response = await fetch(url, {
        method: 'POST',
        headers: {
            'Content-Type': 'application/json;charset=utf-8'
        },
        body: JSON.stringify(appState.CustomerID)
    });
    
    result.Success = response.status < 300;
    const json = await response.json();

    if (result.Success) {
        window.location = json;
    }
    else {
        result.ErrorMessage = json;
        AppInsights.LogEvent("ManageSubscription failed", { "Email": appState.UserEmail, "ErrorMessage": result.ErrorMessage });
    }
    return result;

    // After a user navigates to stripe portal, Stripe calls back using url "../lsi/1"
    // This tells us to reload the users subscription info in the event they made some change.
    // See also /Components/Home.tsx
}

export const GetSubscriptionInfo = async (appState: AppState) => {

    if (appState?.UserEmail?.length === 0 ?? true)
        return;

    const url = AppConfig.host + 'subscription/getsubscriptioninfo';

    let response = await fetch(url, {
        method: 'POST',
        headers: {
            'Content-Type': 'application/json;charset=utf-8'
        },
        body: JSON.stringify(appState.UserEmail)
    });
    var json = (await response.json()) as any;
    appState.CustomerID = json.customerID;
    appState.SubscriptionID = json.subscriptionID;
    appState.SubscriptionCount = json.subscriptionCount;
    appState.BillingID = json.billingID;
    SaveAppState(appState);
}

export const GetCorpSubscriptionInfo = async (corpSubscriptionID: string): Promise<CorpSubscriptionInfo> => {

    if (IsNullOrEmpty(corpSubscriptionID))
        throw new Error("corpSubscriptionID can not be null");

    const url = AppConfig.host + 'subscription/GetCorpSubscriptionInfo';

    let response = await fetch(url, {
        method: 'POST',
        headers: {
            'Content-Type': 'application/json;charset=utf-8'
        },
        body: JSON.stringify(corpSubscriptionID)
    });
    var json = (await response.json()) as any;
    var info = new CorpSubscriptionInfo();
    info.AdminEmail = json.adminEmail;
    info.SubscriptionPlan = DeserializeSubscriptionPlan(json.subscriptionPlan);
    return info;
}


export const IsNullOrEmpty = (s: string): boolean => { return s === null || typeof s === 'undefined' || s.length === 0 };

export const Log = async (msg: string) => {

    if (IsNullOrEmpty(msg))
        return;

    const url = AppConfig.host + 'subscription/loginfo';

    let response = await fetch(url, {
        method: 'POST',
        headers: {
            'Content-Type': 'application/json;charset=utf-8'
        },
        body: JSON.stringify(msg)
    });
}

export const ChangePassword = async (appState: AppState): Promise<string> =>
{
    var errorMsg = await MSAL.resetPassword();

    if (IsNullOrEmpty(errorMsg))
        await SignOut(appState);

    return errorMsg;
}

export const EditProfile = async (appState: AppState): Promise<string> => {
    var errorMsg = await MSAL.editProfile();
    return errorMsg;
}

export const HandleCorpSubAllocation = async (adminID: string, subsriberID: string, isApproved: boolean): Promise<string> => {

    const url = AppConfig.host + 'subscription/CorpSubAllocation';

    const response = await fetch(url, {
        method: 'POST',
        headers: {
            'Content-Type': 'application/json;charset=utf-8'
        },
        body: JSON.stringify({ AdminID: adminID, SubscriberID: subsriberID, IsApproved: isApproved })
    });
    const msg = await response.json() as string;
    return msg;
}

const DeserializeSubscriptionPlan = (json: any): SubscriptionPlan | null =>
{
    if (IsNullOrEmpty(json))
        return null;

    var s: SubscriptionPlan = new SubscriptionPlan();
    s.BillingPeriods = json.billingPeriods;
    s.Cost = json.cost;
    s.DisplaySequence = json.displaySequence;
    s.EndDate = json.endDate;
    s.PaymentProviderPlanID = json.paymentProviderPlanID;
    s.PlanDescription = json.planDescription;
    s.ShortDescription = json.shortDescription;
    s.StartDate = json.startDate;
    return s;
}



//export const GetUserRecord = async (userID: string): Promise<UserRecord> => {

//    if (IsNullOrEmpty(userID))
//        throw new Error("userID is required.");

//    const url = AppConfig.host + 'subscription/getsubscriptioninfo';
//    let response = await fetch(url + "&id=" + userID);
//    const json = await response.json();
//    return (json) as UserRecord;
//}
