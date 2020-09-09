import { string, number } from "prop-types";
import SubscriptionPlan from "../Model/SubscriptionPlan";
import { GlobalContext, AppState } from '../AppState';
import AppConfig from '../appconfig';
import * as MSAL from 'msal';
import MSALConfig from '../msalconfig';


// Dummmy / test
export const GetPerson = async (): Promise<any> => {
    const url = 'https://api.randomuser.me/';
    const response = await fetch(url);
    const json = await response.json();
    const result = json.results[0];
    return result;
}

// Dummmy / test
export const GetIdentity = async () : Promise<string> => {
    const url = window.location.origin + '/Subscription/Identity';
    const response = await fetch(url);
    const result = await response.text();
    return result;
}

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
        s.StartDate = json[i].startDate;
        result.push(s);
    }
    
    return result;
}

export const SignIn = async (appState: AppState): Promise<boolean> => {

    if (appState.UserID != null && appState.UserID.length > 1) {
        alert('You are already signed in.  Sign out before signing in again.')
        return true;
    }

    const auth: MSAL.Configuration = new MSALConfig();
    const userAgentApplication = new MSAL.UserAgentApplication(auth as MSAL.Configuration);
    var success: boolean = false;

    try {
        const response = await userAgentApplication.loginPopup(AppConfig.loginScopes);

        if (response.idToken !== null) {
            appState.UserName = response.account.name;
            appState.UserID = response.account.accountIdentifier;
            appState.UserEmail = response.idTokenClaims?.emails[0];  // do this until we can get user email from MS Graph.
            appState.Token = response.idToken;
            SaveAppState(appState);
            success = true;
        }
    }
    catch (err) {

        // login dialog throws if user cancels
    }

    if (!success) {
        alert('The login attempt was not successful.');
    }
    return success;
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
    appState.SubscriptionPlans = new Array<SubscriptionPlan>();
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
    return appState;
}


export const FormatMoney = (num: number) => {
    var formatter = new Intl.NumberFormat('en-US', {
        style: 'currency',
        currency: 'USD',
    });

    return formatter.format(num); 
}



    
