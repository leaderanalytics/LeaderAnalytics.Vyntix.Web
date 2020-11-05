import React, { useState, useContext, useEffect } from 'react';
import { BrowserRouter as Router, Route, Switch, Link, NavLink } from 'react-router-dom';
import { useAsyncEffect } from 'use-async-effect';
import { GlobalContext, AppState } from '../AppState';
import { SignIn, SignOut, ManageSubscription } from '../Services/Services';
import { Button, Navbar, NavDropdown, Nav, Form, FormControl, Container, Image, DropdownButton, ButtonGroup, Dropdown } from 'react-bootstrap';
import { FontAwesomeIcon } from '@fortawesome/react-fontawesome'
import { faBars, faHome, faBahai, faKey, faBook, faDownload, faEnvelope, faSignInAlt, faSignOutAlt } from '@fortawesome/free-solid-svg-icons'
import logo from '../Assets/VyntixLogo.png';
import AsyncResult from '../Model/AsyncResult';
import Dialog from './Dialog';
import DialogType from '../Model/DialogType';
import DialogProps from '../Model/DialogProps';

const TopNav = () => {
    const appState: AppState = useContext(GlobalContext);
    const [isSignedIn, setisSignedIn] = useState(appState.UserID !== null && appState.UserID.length > 1);   // UserID is maintained by Azure
    const [hasActiveSub, setHasActiveSub] = useState(appState.SubscriptionID !== null && appState.SubscriptionID.length > 1); // True if user has an active subscription.
    const [hasAnySub, setHasAnySub] = useState(appState.SubscriptionCount > 0); // True if user has any subscription - active or not.
    const [dialogProps, setDialogProps] = useState(new DialogProps("", DialogType.None, () => { }));

    appState.RenderTopNav = () => {
        setisSignedIn(appState.UserID !== null && appState.UserID.length > 1);
        setHasAnySub(appState.SubscriptionCount > 0);
        setHasActiveSub(appState.SubscriptionID !== null && appState.SubscriptionID.length > 1);
    };

    const LocalSignIn = async (event: any) => {
        event.preventDefault();
        const errorMsg = await SignIn(appState);
        const tmpIsSignedIn: boolean = errorMsg.length === 0;

        setisSignedIn(tmpIsSignedIn);
        appState.RenderTopNav();

        if (tmpIsSignedIn) {
            appState.SignInCallback?.call(null, tmpIsSignedIn);
        }
        else
            setDialogProps(new DialogProps(errorMsg, DialogType.Error, () => { setDialogProps(new DialogProps("", DialogType.None, () => { })); }));
    }

    const LocalManageSubscription = async (event: any) => {
        event.preventDefault();
        const result: AsyncResult = await ManageSubscription(appState);

        if (!result.Success) 
            setDialogProps(new DialogProps(result.ErrorMessage, DialogType.Error, () => { setDialogProps(new DialogProps("", DialogType.None, () => { })); }));
    }

    return (
        <Navbar variant="dark" expand="lg" collapseOnSelect fixed="top" className="nav-container dark-bg">
            <Dialog dialogProps={dialogProps} />
            <Navbar.Brand href="/">
                <Image src={logo} className="logo-large" />
            </Navbar.Brand>
            <Navbar.Toggle aria-controls="basic-navbar-nav" >
                <FontAwesomeIcon icon={faBars} className="nav-toggle" />
            </Navbar.Toggle>
            <Navbar.Collapse id="basic-navbar-nav">
                <Nav className="nav-fill w-100">
                    <Nav.Link className="rh6" as={NavLink} to="/" href="/" exact eventKey="1" ><FontAwesomeIcon icon={faHome} className="nav-toggle nav-icon" />Home</Nav.Link>
                    <Nav.Link className="rh6" as={NavLink} to="/Subscriptions" href="/Subscriptions" eventKey="2"><FontAwesomeIcon icon={faKey} className="nav-toggle nav-icon" />Subscribe</Nav.Link>
                    <Nav.Link className="rh6" as={NavLink} to="/Documentation" href="/Documentation" eventKey="3"><FontAwesomeIcon icon={faBook} className="nav-toggle nav-icon" />Documentation</Nav.Link>
                    <Nav.Link className="rh6" as={NavLink} to="/Downloads" href="/Downloads" eventKey="4"><FontAwesomeIcon icon={faDownload} className="nav-toggle nav-icon" />Downloads</Nav.Link>
                    <Nav.Link className="rh6" as={NavLink} to="/Contact" href="/Contact" eventKey="5"><FontAwesomeIcon icon={faEnvelope} className="nav-toggle nav-icon" />Contact Us</Nav.Link>

                    {isSignedIn ?
                        <ButtonGroup>
                            <DropdownButton as={ButtonGroup} title="Profile" id="profileButton" alignRight className={`${hasActiveSub ? "green-border" : "trans-border"}`}>
                                <Dropdown.Item eventKey="1" className="rh6" onClick={() => SignOut(appState)} >Sign Out</Dropdown.Item>
                                <Dropdown.Item eventKey="2" className="rh6" disabled={!hasAnySub} onClick={LocalManageSubscription}>Manage Subscription</Dropdown.Item>
                            </DropdownButton>
                        </ButtonGroup>
                        :
                        <Nav.Link className="rh6" to="/" as={NavLink} onClick={LocalSignIn} eventKey="7" exact activeClassName="zzz"><FontAwesomeIcon icon={faSignInAlt} className="nav-toggle nav-icon" />Sign in</Nav.Link>
                    }
                </Nav>
            </Navbar.Collapse>
        </Navbar>
    );
}
export default TopNav;