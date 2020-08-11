import React, { useState, useContext } from 'react';
import logo from './logo.svg';
import './App.css';
import Nav from './Components/Nav';
import Home from './Components/Home';
import About from './Components/About';
import Contact from './Components/Contact';
import Subscriptions from './Components/Subscriptions';
import SubSignIn from './Components/SubSignIn';
import SubConfirmation from './Components/SubConfirmation';
import SubActivationSuccess from './Components/SubActivationSuccess';
import SubActivationFailure from './Components/SubActivationFailure';
import { GetAppState } from './Services/Services';
import { BrowserRouter as Router, Route, Switch, Link } from 'react-router-dom';
import { AppState, GlobalContext } from './AppState';


function App() {

    return (
        <GlobalContext.Provider value={GetAppState()}>
          <Router>
            <Nav />
              <Switch>
                  <Route exact path="/" component={Home} />
                  <Route exact path="/about/:name/:otherName" component={About} />
                  <Route exact path="/contact" component={Contact} />
                  <Route exact path="/subscriptions" component={Subscriptions} />
                  <Route exact path="/subsignin" component={SubSignIn} />
                  <Route exact path="/subconfirmation" component={SubConfirmation} />
                  <Route exact path="/subactivationsuccess" component={SubActivationSuccess} />
                  <Route exact path="/subactivationfailure" component={SubActivationFailure} />
            </Switch>
          </Router>
          </GlobalContext.Provider>
  );
}

export default App;
