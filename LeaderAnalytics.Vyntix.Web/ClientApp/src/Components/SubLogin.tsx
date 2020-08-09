import React from 'react';
import { useContext } from 'react';
import { useHistory } from 'react-router-dom'
import { GlobalContext, GlobalSettings } from '../GlobalSettings';
import { Redirect } from 'react-router-dom';
import AppConfig from '../appconfig';
import * as MSAL from 'msal';
import MSALConfig from '../msalconfig';

function SubLogin() {
    const globalSettings: GlobalSettings = useContext(GlobalContext);
    const history = useHistory();
    const SignIn = () => {

        // All changes made to this method must also be made in Nav.tsx.  

        var result: boolean = false;

        if (globalSettings.UserID != null && globalSettings.UserID.length > 1) {
            alert('You are already signed in.  Sign out before signing in again.')
            return true;
        }

        const auth: MSAL.Configuration = new MSALConfig();
        const userAgentApplication = new MSAL.UserAgentApplication(auth as MSAL.Configuration);

        userAgentApplication.loginPopup(AppConfig.loginScopes).then(response => {
            globalSettings.UserName = response.account.name;
            globalSettings.UserID = response.account.accountIdentifier;
            globalSettings.UserEmail = response.idTokenClaims?.emails[0];  // do this until we can get user email from MS Graph.
            globalSettings.Token = response.idToken;
            localStorage.setItem('globalSettings', JSON.stringify(globalSettings));
            history.push("/SubConfirmation");

        }).catch(err => {
            alert('The login attempt was not successful.');
            history.push("/Subscriptions");
        });
    }

  
    return (
        <div>
            <div>{globalSettings.SubscriptionPlan?.PlanDescription}</div>

            <div>
                You must log in using your existing acccount or create a new account before you can proceed with your subscription purchase.
            </div>

            <div>
                Click the login button below to log in.  If you do not have an account you can create one.
            </div>

            <div>
                <button onClick={SignIn}>Login</button>
            </div>

            <div>
                <button onClick={() => history.push("/Subscriptions")} >Return to Order</button>
            </div>
        </div>
    )
}
export default SubLogin;