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
import { BrowserRouter as Router, Route, Routes, Link } from 'react-router-dom';
import { AppState, GlobalContext } from './AppState';
import Documentation from './Components/Documentation';
import Downloads from './Components/Downloads';
import ScrollToTop from './Components/ScrollToTop';
import Privacy from './Components/Privacy';
import Terms from './Components/Terms';
import AppInsights from './Services/AppInsights';
import CorpSubAllocation from './Components/CorpSubAllocation';
import { MsalProvider } from "@azure/msal-react";
import { IPublicClientApplication } from "@azure/msal-browser";
// This is a callback for stripe that tells us to reload subscription info:
// path="/lsi/:id"
//

type AppProps = {
    pca: IPublicClientApplication
};


function App({ pca }: AppProps) {
    AppInsights.Init(); // Must call before initializing AppState.
    AppInsights.LogEvent("Application Start");
    const appState: AppState = GetAppState();
    return (
        <GlobalContext.Provider value={appState}>
            <Router>
                <MsalProvider instance={ pca }>
                <ScrollToTop />
                <TopNav />
                    <Routes>
                        <Route path="/" element={<Home/>} />
                        <Route path="/lsi/:id" element={<Home/>} />
                        <Route path="/contact" element={<Contact/>} />
                        <Route path="/documentation" element={<Documentation/>} />
                        <Route path="/downloads" element={<Downloads/>} />
                        <Route path="/subactivationsuccess" element={<SubActivationSuccess/>} />
                        <Route path="/subactivationfailure" element={<SubActivationFailure/>} />
                        <Route path="/subconfirmation" element={<SubConfirmation/>} /> 
                        <Route path="/subscriptions" element={<Subscriptions/>} />
                        <Route path="/subsignin" element={<SubSignIn/>} />
                        <Route path="/subplans" element={<SubPlans/>} />
                        <Route path="/privacy" element={<Privacy/>} />
                        <Route path="/terms" element={<Terms/>} />
                        <Route path="/corpsuballocation/:a/:s/:o" element={<CorpSubAllocation/>} />
                        <Route path="*" element={<Home/>} />
                </Routes>
                <Footer />
                </MsalProvider>
            </Router>
        </GlobalContext.Provider>
  );
}

export default App;
