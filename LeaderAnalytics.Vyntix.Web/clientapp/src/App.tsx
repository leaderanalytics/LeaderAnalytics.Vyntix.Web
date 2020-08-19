import React, { useState, useContext } from 'react';
import TopNav from './Components/TopNav';
import Home from './Components/Home';
import Contact from './Components/Contact';
import Subscriptions from './Components/Subscriptions';
import SubSignIn from './Components/SubSignIn';
import SubConfirmation from './Components/SubConfirmation';
import SubActivationSuccess from './Components/SubActivationSuccess';
import SubActivationFailure from './Components/SubActivationFailure';
import { GetAppState } from './Services/Services';
import { BrowserRouter as Router, Route, Switch, Link } from 'react-router-dom';
import { AppState, GlobalContext } from './AppState';
import Documentation from './Components/Documentation';
import Downloads from './Components/Downloads';

function App() {

    return (
        <GlobalContext.Provider value={GetAppState()}>
            <Router>
                <TopNav />
                <div>   /* Do not use container here.  We don't want 15 px padding.  */
                    <Switch>
                        <Route exact path="/" component={Home} />
                        <Route exact path="/contact" component={Contact} />
                        <Route exact path="/documentation" component={Documentation} />
                        <Route exact path="/downloads" component={Downloads} />
                        <Route exact path="/subactivationsuccess" component={SubActivationSuccess} />
                        <Route exact path="/subactivationfailure" component={SubActivationFailure} />
                        <Route exact path="/subconfirmation" component={SubConfirmation} />
                        <Route exact path="/subscriptions" component={Subscriptions} />
                        <Route exact path="/subsignin" component={SubSignIn} />
                    </Switch>
                </div>
            </Router>
        </GlobalContext.Provider>
  );
}

export default App;
