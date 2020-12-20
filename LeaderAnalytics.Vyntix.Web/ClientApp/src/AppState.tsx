import React from 'react';
import SubscriptionPlan from './Model/SubscriptionPlan';

export class AppState {
    public TimeStamp: number = Date.now();
    public UserName: string = "";
    public UserID: string = "";
    public UserEmail: string = "";          
    public BillingID: string = "";        // Azure User ID of the user responsible for payment for a subscription.  Referenced user must have IsCorpAdmin = true.  
    public CustomerID: string = ""; 
    public SubscriptionID: string = "";     // Active subscription, if any
    public SubscriptionCount: number = 0;   // Number of subscriptions - active or not.  If > 0, user can navigate to Stripe portal. 
    public ID_Token: string = "";
    public AccessToken: string = "";
    public SignInCallback: (isSignedIn: boolean) => void = (isSignedIn) => { };
    public RenderTopNav: () => void = () => { };
    public Message: string = "";
    public IsCorpAdmin: boolean = false;
    public IsOptIn: boolean = false;
    // Order
    public PromoCodes: string = "";
    public SubscriptionPlan: SubscriptionPlan | null = null;
    public CorpSubscriptionID = ""; // Azure User ID of a corp admin.  This will become the BillingID if/when the subscription is created.  
    public CorpAdminEmail = ""; // Email address of corp admin.  We need this to compare to domain name of user email after user logs in.  
};

export const GlobalContext = React.createContext<AppState>(new AppState());