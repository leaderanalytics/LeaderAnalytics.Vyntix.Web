import React, { useState, useContext } from 'react';
import logo from './logo.svg';
import './App.css';
import Nav from './Components/Nav';
import Home from './Components/Home';
import About from './Components/About';
import Contact from './Components/Contact';
import Subscriptions from './Components/Subscriptions';
import { BrowserRouter as Router, Route, Switch, Link } from 'react-router-dom';
import { GlobalSettings, GlobalContext } from './GlobalSettings';


var s = localStorage.getItem('globalSettings');
var g: GlobalSettings = s === null ? new GlobalSettings() : JSON.parse(s);

function App() {

  return (
          <GlobalContext.Provider value={g}>
          <Router>
            <Nav />
              <Switch>
                  <Route exact path="/" component={Home} />
                  <Route exact path="/about/:name/:otherName" component={About} />
                  <Route exact path="/contact" component={Contact} />
                  <Route exact path="/subscriptions" component={Subscriptions} />
            </Switch>
          </Router>
          </GlobalContext.Provider>
  );
}

export default App;
