import React, { useState, useContext } from 'react';
import { BrowserRouter as Router, Route, Switch, Link } from 'react-router-dom';
import { GlobalContext, GlobalSettings } from '../GlobalSettings';
import { SignIn, SignOut } from '../Services/Services';

const Nav = () => {
    const globalSettings: GlobalSettings = useContext(GlobalContext);

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
                    <button onClick={() => SignIn(globalSettings)}>Sign in</button>
                    <button onClick={() => SignOut(globalSettings)}>Sign out</button>
                </li>
            </ul>
        </header>
    );
}
export default Nav;