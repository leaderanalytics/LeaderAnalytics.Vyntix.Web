import React, { useContext, useEffect, useState } from 'react';
import { NavLink, Link } from 'react-router-dom';
import { GlobalContext, AppState } from '../AppState';
import { useHistory } from 'react-router-dom'
import {Image, Button } from 'react-bootstrap';
import AppConfig  from '../appconfig';
import { loadStripe } from '@stripe/stripe-js';
import { IsNullOrEmpty, GetAppState } from '../Services/Services';
import SelectedPlan from './SelectedPlan';
import OrderApprovalResponse from '../Model/OrderApprovalResponse';
import { FontAwesomeIcon } from '@fortawesome/react-fontawesome';
import { faShoppingCart, faArrowCircleLeft, faKey } from '@fortawesome/free-solid-svg-icons';
import Dialog from './Dialog';
import DialogType from '../Model/DialogType';
import DialogProps from '../Model/DialogProps';


function SubConfirmation() {
    const history = useHistory();
    const appState: AppState = GetAppState();
    const orderTotal = appState.SubscriptionPlan?.Cost ?? 0;
    const [termsChecked, setTermsChecked] = useState(false);
    const [privacyChecked, setPrivacyChecked] = useState(false);
    const [canCreateSubscription, setCanCreateSubscription] = useState(false);
    const [hideTrialPeriodMsg, setHideTrialPeriodMsg] = useState(true);
    const CAPTCHA_URL = AppConfig.host + "email/captchaImage";
    const [captcha, setCaptcha] = useState("");
    const [captchaImgUrl, setCaptchaUrl] = useState(CAPTCHA_URL + '?d=' + new Date().getTime().toString());
    const [dialogProps, setDialogProps] = useState(new DialogProps("", DialogType.None, () => { }));


    // Make sure user is signed in and has a valid subscription.  Redirect to home if not.
    if (IsNullOrEmpty(appState.CustomerID) || appState.SubscriptionPlan == null)
        history.push("/Home");

    const handleSelectionChange = (event: any) => {
        if (event.target.id === "chkTerms")
            setTermsChecked(event.target.checked);
        else if (event.target.id === "chkPrivacy")
            setPrivacyChecked(event.target.checked);
        else
            throw new Error("unknown event origin.");
    };

    const order = {
        UserID: appState.UserID,
        UserEmail: appState.UserEmail,
        CustomerID: appState.CustomerID,
        SubscriptionID: appState.SubscriptionID,
        PaymentProviderPlanID: appState.SubscriptionPlan?.PaymentProviderPlanID,
        PromoCodes: appState.PromoCodes,
        Captcha:""
    };

    useEffect(() => {
        // Check if customer will get directed to payment processor upon confirming their subscription
        // This will happen if the sub is not eligible for a trial period and is not free.

        fetch('/subscription/IsPrepaymentRequired', {
            method: 'POST',
            headers: {
                'Content-Type': 'application/json;charset=utf-8'
            },
            body: JSON.stringify(order)
        }).then(response => {
            response.json().then(j => {
                setHideTrialPeriodMsg(j === null || j === false);
            });
        });
    },[1]);

    const Checkout = async () => {

        if (appState.UserID === null || appState.UserID.length < 2 || appState.UserEmail === null || appState.SubscriptionPlan === null || (captcha?.length ?? 0) === 0)
            return;

        // At this point the user is logged in and has made a subscription selection. 
        // Post user identity and subscription selection back to the server for validation.

        order.Captcha = captcha;

        let response = await fetch('/subscription/CreateSubscription', {
            method: 'POST',
            headers: {
                'Content-Type': 'application/json;charset=utf-8'
            },
            body: JSON.stringify(order)
        });

        const approval = await response.json(); // see Model/OrderApprovalResponse

        if (response.status < 300) {
            // Order approval is OK

            if (approval.sessionID !== null) {
                // If approval.SessionID is not null, we need to redirect to 
                // payment processor so customer can make payment immediately:
                const stripe = await loadStripe(AppConfig.StripeApiKey);
                stripe?.redirectToCheckout({ sessionId: approval.sessionID });
            }
            else {
                // We are done.  Customer has created a subscription with a trial period
                // or a free subscription.
                history.push("/SubActivationSuccess");
            }
        }
        else {

            // Approval error
            setDialogProps(new DialogProps(approval.errorMessage, DialogType.Error, () => { setDialogProps(new DialogProps("", DialogType.None, () => { })); history.push("/"); }));
        }
    }
    useEffect(() => setCanCreateSubscription(termsChecked && privacyChecked && (captcha?.length ?? 0) > 0));
  

    return (
        <div className="content-root container-fluid dark-bg" >
            <Dialog dialogProps={dialogProps} />
            <div id="banner">
                <div className="pageBanner rp1">
                    <span className="rh5">Confirm your subscription</span>
                </div>
            </div>
            <div className="rmt1 rm-fallback rmb1">
                <SelectedPlan />
                <div className="rh6">

                    <div className="rmt2" hidden={hideTrialPeriodMsg}>
                        This subscription is not eligible for a trial period because you have received a trial period for subscriptions you have purchased previously.  Only one trial period per subscriber is granted.
                    </div>

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

                    <div className="rmt2"  >
                        <div>
                            <Image id="captchaImage" src={captchaImgUrl} />
                        </div>
                        <div className="form-inline rmt1">
                            <div className="form-group">
                                <label className="control-label">Enter the numbers shown above:&nbsp; </label>
                                <input type="text" style={{ width: "90px" }} className="form-control" onChange={e => setCaptcha(e.target.value)}></input>
                            </div>
                        </div>
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
                            <Button onClick={Checkout} className={`iconButton rmt1 rmb1 ${canCreateSubscription ? "green-border" : "trans-border"}`} disabled={!canCreateSubscription} >
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