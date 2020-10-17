import { string, number } from "prop-types";
import SubscriptionPlan from "../Model/SubscriptionPlan";
import { GlobalContext, AppState } from '../AppState';
import AppConfig from '../appconfig';
import * as MSAL from 'msal';
import MSALConfig from '../msalconfig';
import ContactRequest from '../Model/ContactRequest';
import AsyncResult from '../Model/AsyncResult';

// Get active subscription plans
export const GetSubscriptionPlans = async (): Promise<SubscriptionPlan[]> => {
    const url = window.location.origin + '/Subscription/GetActiveSubscriptionPlans';
    const response = await fetch(url);
    const json = await response.json();
    var result: SubscriptionPlan[] = new Array<SubscriptionPlan>();

    for (let i = 0; i < json.length; i++) {
        var s: SubscriptionPlan = new SubscriptionPlan();
        s.BillingPeriods = json[i].billingPeriods;
        s.Cost = json[i].cost;
        s.DisplaySequence = json[i].displaySequence;
        s.EndDate = json[i].endDate;
        s.PaymentProviderPlanID = json[i].paymentProviderPlanID;
        s.PlanDescription = json[i].planDescription;
        s.ShortDescription = json[i].shortDescription;
        s.StartDate = json[i].startDate;
        result.push(s);
    }
    return result;
}

export const SignIn = async (appState: AppState): Promise<string> => {

    if (appState.UserID != null && appState.UserID.length > 1) {
        return 'You are already signed in.  Sign out before signing in again.';
    }

    const auth: MSAL.Configuration = new MSALConfig();
    const userAgentApplication = new MSAL.UserAgentApplication(auth as MSAL.Configuration);
    var msg: string = '';

    try {
        const response = await userAgentApplication.loginPopup(AppConfig.loginScopes);

        if (response.idToken !== null) {
            appState.UserName = response.account.name;
            appState.UserID = response.account.accountIdentifier;
            appState.UserEmail = response.idTokenClaims?.emails[0];  // do this until we can get user email from MS Graph.
            appState.Token = response.idToken;
            // Todo - check isNew when creating new user - populate billing email. isNew will be 'true' (string)
            const isNew: string = response.idTokenClaims?.newUser ?? '';

            await GetSubscriptionInfo(appState);
            SaveAppState(appState);
        }
    }
    catch (err) {
        // login dialog throws if user cancels
        msg = 'The login attempt was not successful.';
    }
    return msg;
}

export const SignOut = (appState: AppState) => {

    const auth: MSAL.Configuration = new MSALConfig();
    const userAgentApplication = new MSAL.UserAgentApplication(auth as MSAL.Configuration);
    userAgentApplication.logout();

    appState.UserName = "";
    appState.UserID = "";
    appState.UserEmail = "";
    appState.CustomerID = "";
    appState.SubscriptionID = "";
    appState.Token = null;
    appState.PromoCodes = "";
    appState.SubscriptionPlan = null;
    localStorage.removeItem('appState');
}

export const SaveAppState = (appState: AppState) => {
    appState.TimeStamp = Date.now();
    localStorage.setItem('appState', JSON.stringify(appState));
}

export const GetAppState = (): AppState => {

    var s = localStorage.getItem('appState');
    var appState: AppState = (s === null || s.length === 0) ? new AppState() : JSON.parse(s);

    if (Date.now() - appState.TimeStamp > 3600000 && appState.Token !== null) {
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
    const url = AppConfig.host + 'email/sendemail';
    const msg = 'From Site:LeaderAnalytics.Vyntix.Web' + '\r\nName: ' + request.Name + '\r\nPhone: ' + request.Phone + '\r\nEmail: ' + request.EMail + '\r\nRequirement: ' + request.Requirement + '\r\nComment: ' + request.Message;
    
    let response = await fetch(url, {
        method: 'POST',
        headers: {
            'Content-Type': 'application/json;charset=utf-8'
        },
        body: JSON.stringify({ "To": "leaderanalytics@outlook.com", "Msg": msg, "CaptchaCode": request.Captcha })
    });

    const result: AsyncResult = new AsyncResult();
    result.Success = response.status < 300;

    if (!result.Success)
        result.ErrorMessage = await response.json();
    return result;
}

export const ManageSubscription = async (appState: AppState): Promise<AsyncResult> => {

    const result: AsyncResult = new AsyncResult();

    if ((appState?.CustomerID?.length ?? 0) === 0) {
        result.ErrorMessage = "Invalid CustomerID";
        return result;
    }

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
    SaveAppState(appState);
}

export const IsNullOrEmpty = (s: string) : boolean => { return s === null || s.length === 0 };
