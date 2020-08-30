import React, { useState, useContext, useEffect } from 'react';
import { BrowserRouter as Router, Route, Switch, Link, NavLink } from 'react-router-dom';
import { useAsyncEffect } from 'use-async-effect';
import { GlobalContext, AppState } from '../AppState';
import { SignIn, SignOut } from '../Services/Services';
import { Button, Navbar, NavDropdown, Nav, Form, FormControl, Container, Image } from 'react-bootstrap';
import { FontAwesomeIcon } from '@fortawesome/react-fontawesome'
import { faBars, faHome, faBahai, faKey, faBook, faDownload, faEnvelope, faSignInAlt, faSignOutAlt } from '@fortawesome/free-solid-svg-icons'
import logo from '../Assets/VyntixLogo.png';


const TopNav = () => {
    const appState: AppState = useContext(GlobalContext);
    const [isSignedIn, setisSignedIn] = useState(appState.UserID !== null && appState.UserID.length > 1);
    appState.RenderTopNav = () => {

        setisSignedIn(appState.UserID !== null && appState.UserID.length > 1)


    };

    

    const LocalSignIn = async () => {
        const x = await SignIn(appState);

        setisSignedIn(x);

        if (isSignedIn) {
            appState.SignInCallback?.call(null, isSignedIn);
        }
    }

    return (
        <Navbar variant="dark" expand="lg" collapseOnSelect fixed="top" className="nav-container dark-bg">
            <Navbar.Brand href="/">
                <Image src={logo} className="logo-large" />
            </Navbar.Brand>
            <Navbar.Toggle aria-controls="basic-navbar-nav" >
                <FontAwesomeIcon icon={faBars} className="nav-toggle" />
            </Navbar.Toggle>
            <Navbar.Collapse id="basic-navbar-nav">
                <Nav className="nav-fill w-100 dark-bg">
                    <Nav.Link className="rh6" as={NavLink} to="/" href="/" exact eventKey="1" ><FontAwesomeIcon icon={faHome} className="nav-toggle nav-icon" />Home</Nav.Link>
                    <Nav.Link className="rh6" as={NavLink} to="/Subscriptions" href="/Subscriptions" eventKey="2"><FontAwesomeIcon icon={faKey} className="nav-toggle nav-icon" />Subscribe</Nav.Link>
                    <Nav.Link className="rh6" as={NavLink} to="/Documentation" href="/Documentation" eventKey="3"><FontAwesomeIcon icon={faBook} className="nav-toggle nav-icon" />Documentation</Nav.Link>
                    <Nav.Link className="rh6" as={NavLink} to="/Downloads" href="/Downloads" eventKey="4"><FontAwesomeIcon icon={faDownload} className="nav-toggle nav-icon" />Downloads</Nav.Link>
                    <Nav.Link className="rh6" as={NavLink} to="/Contact" href="/Contact" eventKey="5"><FontAwesomeIcon icon={faEnvelope} className="nav-toggle nav-icon" />Contact Us</Nav.Link>

                    {isSignedIn ?
                        <Nav.Link className="rh6" to="/" as={NavLink} onClick={() => SignOut(appState)} eventKey="6" exact activeClassName="zzz"><FontAwesomeIcon icon={faSignOutAlt} className="nav-toggle nav-icon" />Sign out</Nav.Link>
                        :
                        <Nav.Link className="rh6" to="/" as={NavLink} onClick={LocalSignIn} eventKey="7" exact activeClassName="zzz"><FontAwesomeIcon icon={faSignInAlt} className="nav-toggle nav-icon" />Sign in</Nav.Link>
                    }
                </Nav>
            </Navbar.Collapse>
        </Navbar>
    );
}
export default TopNav;