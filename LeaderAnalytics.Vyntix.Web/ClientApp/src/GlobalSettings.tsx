import React from 'react';
import { IdToken } from 'msal/lib-commonjs/IdToken';
import SubscriptionPlan from './Model/SubscriptionPlan';

export class GlobalSettings {
    public TimeStamp: number = Date.now();
    public UserName: string = "";
    public UserID: string = "";
    public UserEmail: string = "";  // Need this here until I figure out a way to get email from MSGraph.
    public CustomerID: string = ""; 
    public SubscriptionID: string = "";  // Existing paid subscription, if any
    public Token: IdToken | null = null;

    // Order
    public PromoCodes: string = "";
    public SubscriptionPlan: SubscriptionPlan | null = null;
};

var s = new GlobalSettings();
export const GlobalContext = React.createContext<GlobalSettings>(s);