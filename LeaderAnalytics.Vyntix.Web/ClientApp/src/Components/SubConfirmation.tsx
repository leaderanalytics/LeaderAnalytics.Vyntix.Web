import React,  { useContext } from 'react';
import { GlobalContext, GlobalSettings } from '../GlobalSettings';
import { useHistory } from 'react-router-dom'
import AppConfig  from '../appconfig';
import { loadStripe } from '@stripe/stripe-js';



function SubConfirmation() {
    const globalSettings: GlobalSettings = useContext(GlobalContext);
    const history = useHistory();

    const Checkout = async () => {
        

        // At this point the user is logged in and has made a subscription selection. 
        // Post user identity and subscription selection back to the server for validation.

        const order = {
            UserID: globalSettings.UserID,
            UserEmail: globalSettings.UserEmail,                    // only until we can get it from Azure
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


        if (response.status < 300) {
            // Order is OK, redirect to payment processor

            const stripe = await loadStripe(AppConfig.StripeApiKey);

            stripe?.redirectToCheckout({
                customerEmail: "sam.wheat@outlook.com",
                lineItems: [{ price: globalSettings.SubscriptionPlan?.PaymentProviderPlanID, quantity: 1 }],
                successUrl: AppConfig.host + 'SubActivationSuccess',
                cancelUrl: AppConfig.host + 'SubActivationFailure',
                mode: 'subscription'
            });
        }
        else {

            // Error, redirect to Subscriptions page
            const errorMsg = await response.text();
            alert(errorMsg);
            history.push("/Subscriptions");
        }
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