import { string, number } from "prop-types";
import SubscriptionPlan from "../Model/SubscriptionPlan";
import { GlobalContext, GlobalSettings } from '../GlobalSettings';
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

export const SignIn = async (globalSettings: GlobalSettings): Promise<boolean> => {

    if (globalSettings.UserID != null && globalSettings.UserID.length > 1) {
        alert('You are already signed in.  Sign out before signing in again.')
        return true;
    }

    const auth: MSAL.Configuration = new MSALConfig();
    const userAgentApplication = new MSAL.UserAgentApplication(auth as MSAL.Configuration);
    var success: boolean = false;

    try {
        const response = await userAgentApplication.loginPopup(AppConfig.loginScopes);

        if (response.idToken !== null) {
            globalSettings.TimeStamp = Date.now();                          // Keep it from expiring.
            globalSettings.UserName = response.account.name;
            globalSettings.UserID = response.account.accountIdentifier;
            globalSettings.UserEmail = response.idTokenClaims?.emails[0];  // do this until we can get user email from MS Graph.
            globalSettings.Token = response.idToken;
            localStorage.setItem('globalSettings', JSON.stringify(globalSettings));
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

export const SignOut = (globalSettings: GlobalSettings) => {

    const auth: MSAL.Configuration = new MSALConfig();
    const userAgentApplication = new MSAL.UserAgentApplication(auth as MSAL.Configuration);
    userAgentApplication.logout();
    globalSettings.TimeStamp = Date.now();
    globalSettings.UserName = "";
    globalSettings.UserID = "";
    globalSettings.UserEmail = "";
    globalSettings.CustomerID = "";
    globalSettings.SubscriptionID = "";
    globalSettings.Token = null;
    globalSettings.PromoCodes = "";
    globalSettings.SubscriptionPlan = null;
    localStorage.setItem('globalSettings', JSON.stringify(globalSettings));
}


