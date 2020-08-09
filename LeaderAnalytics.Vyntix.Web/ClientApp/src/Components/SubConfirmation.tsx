import React,  { useContext } from 'react';
import { GlobalContext, GlobalSettings } from '../GlobalSettings';
import { useHistory } from 'react-router-dom'

function SubConfirmation() {
    const globalSettings: GlobalSettings = useContext(GlobalContext);
    const history = useHistory();

    const Checkout = async () => {
        alert('This is checkout');

        // At this point the user is logged in and has made a subscription selection. 
        // Post user identity and subscription selection back to the server for validation.

        const order = {
            UserID: globalSettings.UserID,
            CustomerID: globalSettings.CustomerID,
            SubscriptionID: globalSettings.SubscriptionID,
            PaymentProviderPlanID: globalSettings.SubscriptionPlan?.PaymentProviderPlanID,
            PromoCodes: globalSettings.PromoCodes
        };

        let response = await fetch('/subscription/ApproveSubscriptionOrder', {
            method: 'POST',
            headers: {
                'Content-Type': 'application/json;charset=utf-8'
            },
            body: JSON.stringify(order)
        });
    }

    return (
        <div>
            <div>
                This is SubConfirmation
            </div>

            <div>
                {globalSettings.SubscriptionPlan?.PlanDescription}
            </div>

            <div>
                {globalSettings.PromoCodes}
            </div>

            <div>
                <button onClick={Checkout} >Procced to checkout</button>
            </div>


            <div>
                <button onClick={() => history.push("/Subscriptions")} >Return to Order</button>
            </div>
        </div>
    )
}
export default SubConfirmation;