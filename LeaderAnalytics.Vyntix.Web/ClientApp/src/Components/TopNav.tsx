import React, { useState, useContext } from 'react';
import { BrowserRouter as Router, Route, Switch, Link, NavLink } from 'react-router-dom';
import { GlobalContext, AppState } from '../AppState';
import { SignIn, SignOut } from '../Services/Services';
import { Button, Navbar, NavDropdown, Nav, Form, FormControl, Container, Image } from 'react-bootstrap';
import { FontAwesomeIcon } from '@fortawesome/react-fontawesome'
import { faBars, faHome, faBahai, faKey, faBook, faDownload, faEnvelope, faSignInAlt, faSignOutAlt } from '@fortawesome/free-solid-svg-icons'
import logo from '../Assets/VyntixLogo2.png';

const TopNav = () => {
    const appState: AppState = useContext(GlobalContext);
    var [isSignedIn, setisSignedIn] = useState(false);

    const LocalSignIn = async () => {
        isSignedIn = await SignIn(appState);

        setisSignedIn(isSignedIn);

        if (isSignedIn) {
            appState.SignInCallback?.call(null, isSignedIn);
        }
    }


    return (
        <div className="container-fluid">
            <Navbar variant="dark" expand="lg">
                <Navbar.Brand href="/Home">
                    <Image src={logo} className="logo-large" />
                </Navbar.Brand>
                <Navbar.Toggle aria-controls="basic-navbar-nav" >
                    <FontAwesomeIcon icon={faBars} className="nav-toggle" />
                </Navbar.Toggle>
                <Navbar.Collapse id="basic-navbar-nav">
                    <Nav className="nav-fill w-100 ">
                        <Nav.Link className="rh6" as={NavLink} to="/"><FontAwesomeIcon icon={faHome} className="nav-toggle nav-icon" /><span>Home</span></Nav.Link>
                        <Nav.Link className="rh6" as={NavLink} to="/Subscriptions"><FontAwesomeIcon icon={faKey} className="nav-toggle nav-icon" />Subscribe</Nav.Link>
                        <Nav.Link className="rh6" as={NavLink} to="/Documentation"><FontAwesomeIcon icon={faBook} className="nav-toggle nav-icon" />Documentation</Nav.Link>
                        <Nav.Link className="rh6" as={NavLink} to="/Downloads"><FontAwesomeIcon icon={faDownload} className="nav-toggle nav-icon" />Downloads</Nav.Link>
                        <Nav.Link className="rh6" as={NavLink} to="/Contact"><FontAwesomeIcon icon={faEnvelope} className="nav-toggle nav-icon" />Contact Us</Nav.Link>
                        
                        { isSignedIn ?
                            <Nav.Link className="rh6" onClick={() => SignOut(appState)}><FontAwesomeIcon icon={faSignOutAlt} className="nav-toggle nav-icon" />Sign out</Nav.Link>
                        :
                            <Nav.Link className="rh6" onClick={LocalSignIn}><FontAwesomeIcon icon={faSignInAlt} className="nav-toggle nav-icon" />Sign in</Nav.Link>
                        }
                    </Nav>
                </Navbar.Collapse>
            </Navbar>
        </div>
        
    );
}
export default TopNav;