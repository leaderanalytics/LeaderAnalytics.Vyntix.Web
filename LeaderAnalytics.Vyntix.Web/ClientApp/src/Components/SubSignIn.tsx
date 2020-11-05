import React, { useState } from 'react';
import { useContext, useEffect } from 'react';
import { useHistory } from 'react-router-dom'
import { Button } from 'react-bootstrap';
import { GlobalContext, AppState } from '../AppState';
import { SignIn } from '../Services/Services';
import SelectedPlan  from './SelectedPlan';
import { FontAwesomeIcon } from '@fortawesome/react-fontawesome';
import { faKey, faSignInAlt, faArrowCircleLeft } from '@fortawesome/free-solid-svg-icons';
import Dialog from './Dialog';
import DialogProps from '../Model/DialogProps';
import DialogType from '../Model/DialogType';
import AppInsights from '../Services/AppInsights';

function SubSignIn() {
    AppInsights.LogPageView("SubSignIn");
    const appState: AppState = useContext(GlobalContext);
    var [isSignedIn, setisSignedIn] = useState(appState.UserID != null && appState.UserID.length > 1);
    const [dialogProps, setDialogProps] = useState(new DialogProps("", DialogType.None, () => { }));
    const [errorMsg, setErrorMsg] = useState('');
    const history = useHistory();

    // Make sure user has selected a subscription.  Redirect to home if not.
    if (appState.SubscriptionPlan == null)
        history.push("/Home");

    // The SignIn method in the Nav component calls the method that is assigned to appState.SignInCallback.
    // We want the user to navigate to SubConfirmation when they log in using the SignIn button on the Nav component.
    // OR the SignIn button we show on this page:
    appState.SignInCallback = (isSignedIn) => SignInCallback(isSignedIn);

    // The fact that useEffect returns a method tells React it is to be called on unload.
    // When user navigates away from this component we set the sign in call back to null:
    useEffect(() => () => {
        appState.SignInCallback = (x) => { }; // this gets called when this component unloads.
    }, []);

    
    const LocalSignIn = async (event: any) => {
        event.preventDefault();
        var tmp: string = await SignIn(appState);
        isSignedIn = tmp.length === 0;
        setErrorMsg(tmp);
        setisSignedIn(isSignedIn);
        SignInCallback(isSignedIn);
        appState.RenderTopNav();

        if (tmp.length > 0)
            setDialogProps(new DialogProps(tmp, DialogType.Error, () => { setDialogProps(new DialogProps("", DialogType.None, () => { })); }));
    }

    const SignInCallback = (isSignedIn: boolean) => {

        if(isSignedIn)
            history.push("/SubConfirmation");
    }
  
    return (
        <div className="container-fluid content-root dark-bg">
            <Dialog dialogProps={dialogProps} />
            <div id="banner">
                <div className="pageBanner rp1">
                    <span className="rh5">Sign in before creating a subscription</span>
                </div>
            </div>
            <div className="rm-fallback">
                <div className="rmt1 rmb1">
                    <SelectedPlan />
                </div>

                <div className="rh6">
                    You must sign in using your existing acccount or create a new account before you can create your subscription.
                </div>

                <div className="rh6">
                    Click the button below to sign in.  If you do not have an account you can create one.
                </div>

                <div>
                    <Button onClick={() => history.push("/Subscriptions")} className="iconButton rmt1 rmb1 rmr2" >
                        <div className="rh6">
                            <FontAwesomeIcon className="rh4 rmr1" icon={faArrowCircleLeft} />
                            <div>Back to Subscriptions</div>
                        </div>
                    </Button>

                    <Button onClick={LocalSignIn} className="iconButton rmt1 rmb1" >
                        <div className="rh6">
                            <div>Sign in</div>
                            <FontAwesomeIcon className="rh4" icon={faSignInAlt} />
                        </div>
                    </Button>
                </div>
            </div>
        </div>
    )
}
export default SubSignIn;