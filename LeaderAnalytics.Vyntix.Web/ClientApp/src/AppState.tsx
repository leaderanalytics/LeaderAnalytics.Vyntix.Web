import React from 'react';
import { IdToken } from 'msal/lib-commonjs/IdToken';
import SubscriptionPlan from './Model/SubscriptionPlan';

export class AppState {
    public TimeStamp: number = Date.now();
    public UserName: string = "";
    public UserID: string = "";
    public UserEmail: string = "";  // Need this here until I figure out a way to get email from MSGraph.
    public CustomerID: string = ""; 
    public SubscriptionID: string = "";     // Active subscription, if any
    public SubscriptionCount: number = 0;   // Number of subscriptions - active or not.  If > 0, user can navigate to Stripe portal. 
    public Token: IdToken | null = null;
    public SignInCallback: (isSignedIn: boolean) => void = (isSignedIn) => { };
    public RenderTopNav: () => void = () => { };
    public Message: string = "";
    // Order
    public PromoCodes: string = "";
    public SubscriptionPlan: SubscriptionPlan | null = null;
};

export const GlobalContext = React.createContext<AppState>(new AppState());