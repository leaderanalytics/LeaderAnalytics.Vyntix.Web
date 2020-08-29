import React, { useState } from 'react';
import { useContext, useEffect } from 'react';
import { useHistory } from 'react-router-dom'
import { GlobalContext, AppState } from '../AppState';
import { SignIn } from '../Services/Services';

function SubSignIn() {
    const appState: AppState = useContext(GlobalContext);
    var [isSignedIn, setisSignedIn] = useState(appState.UserID != null && appState.UserID.length > 1);
    const history = useHistory();

    // The SignIn method in the Nav component calls the method that is assigned to appState.SignInCallback.
    // We want the user to navigate to SubConfirmation when they log in using the SignIn button on the Nav component.
    // OR the SignIn button we show on this page:
    appState.SignInCallback = (isSignedIn) => SignInCallback(isSignedIn);

    // The fact that useEffect returns a method tells React it is to be called on unload.
    // When user navigates away from this component we set the sign in call back to null:
    useEffect(() => () => {
        appState.SignInCallback = (x) => { }; // this gets called when this component unloads.
    }, []);

    
    const LocalSignIn = async () => {
        isSignedIn = await SignIn(appState);
        setisSignedIn(isSignedIn);
        SignInCallback(isSignedIn);
        appState.RenderTopNav();
    }

    const SignInCallback = (isSignedIn: boolean) => {

        if(isSignedIn)
            history.push("/SubConfirmation");
        else
            history.push("/Subscriptions");
    }
  
    return (
        <div className="container-fluid content-root">
            <div>{appState.SubscriptionPlan?.PlanDescription}</div>

            <div>
                You must log in using your existing acccount or create a new account before you can proceed with your subscription purchase.
            </div>

            <div>
                Click the login button below to log in.  If you do not have an account you can create one.
            </div>

            <div>
                <button onClick={LocalSignIn}>Login</button>
            </div>

            <div>
                <button onClick={() => history.push("/Subscriptions")} >Return to Order</button>
            </div>
        </div>
    )
}
export default SubSignIn;