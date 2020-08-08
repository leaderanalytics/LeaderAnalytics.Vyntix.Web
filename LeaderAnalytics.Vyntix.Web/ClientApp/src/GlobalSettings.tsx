import React from 'react';
import { IdToken } from 'msal/lib-commonjs/IdToken';

export class GlobalSettings {
    public UserName: string = "";
    public UserID: string = "";
    public UserEmail: string = "";  // Need this here until I figure out a way to get email from MSGraph.
    public CustomerID: string = ""; 
    public SubscriptionID: string = "";
    public Token: IdToken | null = null;
};

var s = new GlobalSettings();
export const GlobalContext = React.createContext<GlobalSettings>(s);