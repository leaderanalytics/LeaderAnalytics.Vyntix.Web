import React from 'react';
import { IdToken } from 'msal/lib-commonjs/IdToken';

export class GlobalSettings {
    public UserName: string = "";
    public UserID: string = "";
    public CustomerID: string = "";
    public SubscriptionID: string = "";
    public Token?: IdToken;
};

var s = new GlobalSettings();
export const GlobalContext = React.createContext<GlobalSettings>(s);