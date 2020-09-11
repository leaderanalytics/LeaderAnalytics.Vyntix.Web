﻿import React, { useContext } from 'react';
import { NavLink, Link } from 'react-router-dom';
import { GlobalContext, AppState } from '../AppState';
import { Button } from 'react-bootstrap';
import { useHistory } from 'react-router-dom'
import AppConfig  from '../appconfig';
import { loadStripe } from '@stripe/stripe-js';
import { GetAppState } from '../Services/Services';
import SelectedPlan from './SelectedPlan';
import OrderApprovalResponse from '../Model/OrderApprovalResponse';
import { FontAwesomeIcon } from '@fortawesome/react-fontawesome';
import { faShoppingCart, faArrowCircleLeft, faKey } from '@fortawesome/free-solid-svg-icons';

function SubConfirmation() {
    const history = useHistory();
    const appState: AppState = GetAppState();
    const orderTotal = appState.SubscriptionPlan?.Cost;

    const Checkout = async () => {

        if (appState.UserID === null || appState.UserID.length < 2 || appState.SubscriptionPlan === null)
            return;

        // At this point the user is logged in and has made a subscription selection. 
        // Post user identity and subscription selection back to the server for validation.
        
        const order = {
            UserID: appState.UserID,
            UserEmail: appState.UserEmail,                    
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

        // test
        approval.errorMessage = "this is a test";
        (response as any).status = 301;
        // test

        if (response.status < 300) {

            // Order approval is OK

            if (orderTotal > 0) {
                // Redirect to payment processor
                const stripe = await loadStripe(AppConfig.StripeApiKey);
                stripe?.redirectToCheckout({ sessionId: approval.sessionID });
            }
            else {
                // Create a no cost subscription
                let createResponse = await fetch('/subscription/CreateNoCostSubscription', {
                    method: 'POST',
                    headers: {
                        'Content-Type': 'application/json;charset=utf-8'
                    },
                    body: JSON.stringify(order)
                });
                history.push("/SubActivationSuccess");
            }
        }
        else {

            // Approval error
            appState.Message = approval.errorMessage;
            history.push("/SubActivationSuccess");
        }
    }

    return (
        <div className="content-root container-fluid dark-bg" >
            <div id="banner">
                <div className="pageBanner rp1">
                    <span className="rh5">Confirm your subscription</span>
                </div>
            </div>
            <div className="rmt1 rml2 rmr2 rmb1">
                <SelectedPlan />
            </div>

            <div>
                Please review your subscription carefully. Read the <Link className="rh6" to="/Documentation" target="_blank" >Documentation</Link> page for a complete description of the Vyntix service.
            </div>

            <div>

                <Button onClick={() => history.push("/Subscriptions")} className="iconButton rmt1 rmb1 rmr2" >
                    <div className="rh6">
                        <FontAwesomeIcon className="rh4 rmr1" icon={faArrowCircleLeft} />
                        <div>Back to Subscriptions</div>

                    </div>
                </Button>

                {
                    orderTotal > 0 ?

                        <Button onClick={Checkout} className="iconButton rmt1 rmb1" >
                            <div className="rh6">
                                <div>Check out</div>
                                <FontAwesomeIcon className="rh4" icon={faShoppingCart} />
                            </div>
                        </Button>
                    :
                        <Button onClick={Checkout} className="iconButton rmt1 rmb1" >
                            <div className="rh6">
                                <div>Create Subscription</div>
                                <FontAwesomeIcon className="rh4" icon={faKey} />
                            </div>
                        </Button>
                }

            </div>
        </div>
    )
}
export default SubConfirmation;