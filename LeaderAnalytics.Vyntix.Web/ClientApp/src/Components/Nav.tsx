import React, { useState, useContext } from 'react';
import { BrowserRouter as Router, Route, Switch, Link } from 'react-router-dom';
import * as MSAL from 'msal';
import MSALConfig from '../msalconfig';
import AppConfig from '../appconfig';
import { GlobalContext, GlobalSettings } from '../GlobalSettings';

const Nav = () => {
    const globalSettings: GlobalSettings = useContext(GlobalContext);

    const SignIn = () => {

        // All changes made to this method must also be made in Subscriptions.tsx.

        var result: boolean = false;

        if (globalSettings.UserID != null && globalSettings.UserID.length > 1) {
            alert('You are already signed in.  Sign out before signing in again.')
            return true;
        }
        
        const auth: MSAL.Configuration = new MSALConfig();
        const userAgentApplication = new MSAL.UserAgentApplication(auth as MSAL.Configuration);

        userAgentApplication.loginPopup(AppConfig.loginScopes).then(response => {
            globalSettings.UserName = response.account.name;
            globalSettings.UserID = response.account.accountIdentifier;
            globalSettings.UserEmail = response.idTokenClaims?.emails[0];  // do this until we can get user email from MS Graph.
            globalSettings.Token = response.idToken;
            localStorage.setItem('globalSettings', JSON.stringify(globalSettings));
            result = true;

        }).catch(err => {
            alert('The login attempt was not successful.')
        });

        return result;
    }

    const SignOut = () => {
        
        const auth: MSAL.Configuration = new MSALConfig();
        const userAgentApplication = new MSAL.UserAgentApplication(auth as MSAL.Configuration);
        userAgentApplication.logout();
        globalSettings.UserName = "";
        globalSettings.UserID = "";
        globalSettings.Token = null;
        localStorage.setItem('globalSettings', JSON.stringify(globalSettings));
    }

    return (
        <header>
            <ul>
                <li>
                    <Link to="/">Home</Link>
                </li>

                <li>
                    <Link to="/About/me/her">About</Link>
                </li>

                <li>
                    <Link to="/Contact?number=123&name=joe">Contact</Link>
                </li>

                <li>
                    <Link to="/Subscriptions">Subscribe</Link>
                </li>


                <li>
                    <button onClick={() =>  SignIn()}>Sign in</button>
                    <button onClick={() => SignOut()}>Sign out</button>
                </li>
            </ul>
        </header>
    );


}
export default Nav;