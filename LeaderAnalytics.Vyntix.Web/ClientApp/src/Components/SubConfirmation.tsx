import React,  { useContext } from 'react';
import { GlobalContext, AppState } from '../AppState';
import { useHistory } from 'react-router-dom'
import AppConfig  from '../appconfig';
import { loadStripe } from '@stripe/stripe-js';
import { GetAppState } from '../Services/Services';
import OrderApprovalResponse from '../Model/OrderApprovalResponse';

function SubConfirmation() {
    const history = useHistory();
    const appState: AppState = GetAppState();


    const Checkout = async () => {

        // get the order from session.
        

        if (appState.UserID === null || appState.UserID.length < 2 || appState.SubscriptionPlan === null)
            return;

        // At this point the user is logged in and has made a subscription selection. 
        // Post user identity and subscription selection back to the server for validation.

        const order = {
            UserID: appState.UserID,
            UserEmail: appState.UserEmail,                    // only until we can get it from Azure
            CustomerID: appState.CustomerID,
            SubscriptionID: appState.SubscriptionID,
            PaymentProviderPlanID: appState.SubscriptionPlan?.PaymentProviderPlanID,
            PromoCodes: appState.PromoCodes
        };

        let response = await fetch('/subscription/ApproveSubscriptionOrder', {
            method: 'POST',
            headers: {
                'Content-Type': 'application/json;charset=utf-8'
            },
            body: JSON.stringify(order)
        });

        const approval = await response.json(); // see Model/OrderApprovalResponse

        if (response.status < 300) {
            // Order is OK, redirect to payment processor

            const stripe = await loadStripe(AppConfig.StripeApiKey);
          
            stripe?.redirectToCheckout({ sessionId: approval.sessionID });

            //lineItems: [{ price: appState.SubscriptionPlan?.PaymentProviderPlanID, quantity: 1 }],
            //    successUrl: AppConfig.host + 'SubActivationSuccess',
            //        cancelUrl: AppConfig.host + 'SubActivationFailure',
            //            mode: 'subscription',

        }
        else {

            // Error, redirect to Subscriptions page
            alert(approval.errorMessage);
            history.push("/Subscriptions");
        }
    }

    return (
        <div>
            <div>
                This is SubConfirmation
            </div>

            <div>
                {appState.SubscriptionPlan?.PlanDescription}
            </div>

            <div>
                {appState.PromoCodes}
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