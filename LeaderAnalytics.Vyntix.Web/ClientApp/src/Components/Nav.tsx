import React, { useState, useContext } from 'react';
import { BrowserRouter as Router, Route, Switch, Link } from 'react-router-dom';
import { GlobalContext, AppState } from '../AppState';
import { SignIn, SignOut } from '../Services/Services';

const Nav = () => {
    const appState: AppState = useContext(GlobalContext);

    const LocalSignIn = async () => {
        const isSignedIn = await SignIn(appState);

        if (isSignedIn) {
            appState.SignInCallback?.call(null, isSignedIn);
        }
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
                    <button onClick={LocalSignIn}>Sign in</button>
                    <button onClick={() => SignOut(appState)}>Sign out</button>
                </li>
            </ul>
        </header>
    );
}
export default Nav;