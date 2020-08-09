import { string, number } from "prop-types";
import SubscriptionPlan from "../Model/SubscriptionPlan";
import { GlobalContext, GlobalSettings } from '../GlobalSettings';


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


