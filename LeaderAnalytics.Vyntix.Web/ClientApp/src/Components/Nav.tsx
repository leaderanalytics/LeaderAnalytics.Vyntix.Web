import React, { useState, useContext } from 'react';
import { BrowserRouter as Router, Route, Switch, Link } from 'react-router-dom';
import * as MSAL from 'msal';
import MSALConfig from '../msalconfig';
import { GlobalContext, GlobalSettings } from '../GlobalSettings';

const Nav = () => {

    const globalSettings: GlobalSettings = useContext(GlobalContext);
    var userAgentApplication: MSAL.UserAgentApplication;
    

    const signIn = () => {
        if (userAgentApplication === null || userAgentApplication === undefined) {
            var auth: MSAL.Configuration = new MSALConfig();
            userAgentApplication = new MSAL.UserAgentApplication(auth as MSAL.Configuration)
        }

        var loginRequest = {
            scopes: ["https://LeaderAnalytics.onmicrosoft.com/9ea79dd6-d8c9-48da-8f54-6394a953f003/read"] // optional Array<string>
        };

        userAgentApplication.loginPopup(loginRequest).then(response => {
            
            globalSettings.UserName = response.account.name;
            globalSettings.UserID = response.account.accountIdentifier;
            globalSettings.Token = response.idToken;

        }).catch(err => {
            alert('The login attempt was not successful.')
        });
    }

    const signOut = () => {
        alert('sign out');
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
                    <button onClick={() =>  signIn()}>Sign in</button>
                    <button onClick={() => signOut()}>Sign out</button>
                </li>
            </ul>
        </header>


    );


}
export default Nav;