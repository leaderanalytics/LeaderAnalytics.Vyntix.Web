import React from 'react';
import { useContext } from 'react';
import { useHistory } from 'react-router-dom'
import { GlobalContext, GlobalSettings } from '../GlobalSettings';
import { SignIn } from '../Services/Services';

function SubLogin() {
    const globalSettings: GlobalSettings = useContext(GlobalContext);
    const history = useHistory();


    const LocalSignIn = async () => {
        const isSignedIn = await SignIn(globalSettings);

        if (isSignedIn)
            history.push("/SubConfirmation");
        else
            history.push("/Subscriptions");
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
                <button onClick={LocalSignIn}>Login</button>
            </div>

            <div>
                <button onClick={() => history.push("/Subscriptions")} >Return to Order</button>
            </div>
        </div>
    )
}
export default SubLogin;