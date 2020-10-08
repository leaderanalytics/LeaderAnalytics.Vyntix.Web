import React, { useContext, useEffect, useState } from 'react';
import { NavLink, Link } from 'react-router-dom';
import { GlobalContext, AppState } from '../AppState';
import { useHistory } from 'react-router-dom'
import { Button } from 'react-bootstrap';
import AppConfig  from '../appconfig';
import { loadStripe } from '@stripe/stripe-js';
import { GetAppState, SaveAppState } from '../Services/Services';
import SelectedPlan from './SelectedPlan';
import OrderApprovalResponse from '../Model/OrderApprovalResponse';
import { FontAwesomeIcon } from '@fortawesome/react-fontawesome';
import { faShoppingCart, faArrowCircleLeft, faKey } from '@fortawesome/free-solid-svg-icons';

function SubConfirmation() {
    const history = useHistory();
    const appState: AppState = GetAppState();
    const orderTotal = appState.SubscriptionPlan?.Cost ?? 0;
    const [termsChecked, setTermsChecked] = useState(false);
    const [privacyChecked, setPrivacyChecked] = useState(false);
    const [canCreateSubscription, setCanCreateSubscription] = useState(false);


    const handleSelectionChange = (event: any) => {
        if (event.target.id === "chkTerms")
        {
            setTermsChecked(event.target.checked);
        }
        else if (event.target.id === "chkPrivacy")
        {
            setPrivacyChecked(event.target.checked);
        }
        else
            throw new Error("unknown event origin.");

    
    };


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
            SaveAppState(appState);
            history.push("/SubActivationFailure");
        }
    }
    useEffect(() => setCanCreateSubscription(termsChecked && privacyChecked));
  

    return (
        <div className="content-root container-fluid dark-bg" >
            <div id="banner">
                <div className="pageBanner rp1">
                    <span className="rh5">Confirm your subscription</span>
                </div>
            </div>
            <div className="rmt1 rml13 rmr13 rmb1">
                <SelectedPlan />
                <div className="rh6">

                    <div className="rmt2">
                        Please review your subscription carefully. Read the <Link className="rh6" to="/Documentation" target="_blank">documentation</Link> page for a complete description of the Vyntix service.
                    </div>

                    <div className="rmt2">
                        <input className="mr-2 subscribeCheckbox" type="checkbox" onChange={handleSelectionChange} id="chkTerms"></input>
                        <span>
                            I have read, I understand, and I agree to the <Link to="/Terms" target="_blank" ><span>Vyntix Terms of Use</span></Link>.
                        </span>
                    </div>

                    <div className="rmt2"  >
                        <input className="mr-2 subscribeCheckbox" type="checkbox" onChange={handleSelectionChange} id="chkPrivacy"></input>
                        <span>
                            I have read, I understand, and I agree to the <Link to="/Privacy" target="_blank" ><span>Vyntix Privacy Policy</span></Link>.
                        </span>
                    </div>
                </div>
                <div className="rmt3 center-content">

                    <Button onClick={() => history.push("/Subscriptions")} className="iconButton rmt1 rmb1 rmr2" >
                        <div className="rh6">
                            <FontAwesomeIcon className="rh4 rmr1" icon={faArrowCircleLeft} />
                            <div>Subscriptions</div>

                        </div>
                    </Button>

                    {
                        orderTotal > 0 ?
                            
                            <Button onClick={Checkout} className={`iconButton rmt1 rmb1 ${canCreateSubscription ? "green-border" : "trans-border" }`} disabled={!canCreateSubscription} >
                                <div className="rh6">
                                    <div>Check out</div>
                                    <FontAwesomeIcon className="rh4" icon={faShoppingCart} />
                                </div>
                            </Button>
                            
                            :
                            <Button onClick={Checkout} className="iconButton rmt1 rmb1" disabled={!canCreateSubscription} >
                                <div className="rh6">
                                    <div>Create Subscription</div>
                                    <FontAwesomeIcon className="rh4" icon={faKey} />
                                </div>
                            </Button>
                    }

                </div>
            </div>
        </div>
    )
}
export default SubConfirmation;