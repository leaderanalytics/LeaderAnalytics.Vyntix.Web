import React, { useState, useContext } from 'react';
import TopNav from './Components/TopNav';
import Footer from './Components/Footer';
import Home from './Components/Home';
import Contact from './Components/Contact';
import Subscriptions from './Components/Subscriptions';
import SubPlans from './Components/SubPlans';
import SubSignIn from './Components/SubSignIn';
import SubConfirmation from './Components/SubConfirmation';
import SubActivationSuccess from './Components/SubActivationSuccess';
import SubActivationFailure from './Components/SubActivationFailure';
import { GetAppState } from './Services/Services';
import { BrowserRouter as Router, Route, Switch, Link } from 'react-router-dom';
import { AppState, GlobalContext } from './AppState';
import Documentation from './Components/Documentation';
import Downloads from './Components/Downloads';
import ScrollToTop from './Components/ScrollToTop';
import Privacy from './Components/Privacy';
import Terms from './Components/Terms';
import AppInsights from './Services/AppInsights';
import CorpSubAllocation from './Components/CorpSubAllocation';

// This is a callback for stripe that tells us to reload subscription info:
// path="/lsi/:id"
//

function App() {
    AppInsights.Init(); // Must call before initializing AppState.
    AppInsights.LogEvent("Application Start");
    const appState: AppState = GetAppState();
    return (
        <GlobalContext.Provider value={appState}>
            <Router>
                <ScrollToTop />
                <TopNav />
                <Switch>
                    <Route exact path="/" component={Home} />
                    <Route exact path="/lsi/:id" component={Home} />
                    <Route exact path="/contact" component={Contact} />
                    <Route exact path="/documentation" component={Documentation} />
                    <Route exact path="/downloads" component={Downloads} />
                    <Route exact path="/subactivationsuccess" component={SubActivationSuccess} />
                    <Route exact path="/subactivationfailure" component={SubActivationFailure} />
                    <Route exact path="/subconfirmation" component={SubConfirmation} /> 
                    <Route exact path="/subscriptions" component={Subscriptions} />
                    <Route exact path="/subsignin" component={SubSignIn} />
                    <Route exact path="/subplans" component={SubPlans} />
                    <Route exact path="/privacy" component={Privacy} />
                    <Route exact path="/terms" component={Terms} />
                    <Route exact path="/corpsuballocation/:a/:s/:o" component={CorpSubAllocation} />
                    <Route path="*" component={Home} />
                </Switch>
                <Footer/>
            </Router>
        </GlobalContext.Provider>
  );
}

export default App;
