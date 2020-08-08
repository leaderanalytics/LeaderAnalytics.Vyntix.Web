import React from 'react';
import { useState, useContext, SyntheticEvent } from 'react';
import { useAsyncEffect } from 'use-async-effect';
import $ from 'jquery';
import { GetSubscriptionPlans } from '../Services/Services';
import SubscriptionPlan from '../Model/SubscriptionPlan';
import AppConfig from '../appconfig';
import * as MSAL from 'msal';
import MSALConfig from '../msalconfig';

import { GlobalContext, GlobalSettings } from '../GlobalSettings';

function Subscriptions() {
    const globalSettings: GlobalSettings = useContext(GlobalContext);
    const [promoCodes, setPromoCodes] = useState("");    

    const useFetch = () => {
        
        var [plans, setPlans] = useState(new Array<SubscriptionPlan>());
        const [loading, setLoading] = useState(true);
        

        useAsyncEffect(async (isMounted) => {

            if (!isMounted())
                return;

            plans = await GetSubscriptionPlans();
            setPlans(plans);
            setLoading(false);

        }, []);

        return { plans, loading };
    }

    const getSelectionCount = () : number => {
        return $("#subPlans > div > input:checked").length;
    }

    const getSelectedSubscription = () : string => {
        return ($("#subPlans > div > input:checked").attr("data-providerid") as string);
    }

    // make sure only one plan is checked.
    const handleSelectionChange = (event: SyntheticEvent) => {
        
        const isChecked: boolean = $(event.target).is(":checked");

        if (!isChecked)
            return;

        $("#subPlans > div > input:checked").prop("checked", false);
        $(event.target).prop("checked", true);
    }

    const handlePromoCodeChange = (event: any) => {
        setPromoCodes(event.target.value);
    }


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
            result = true;

        }).catch(err => {
            alert('The login attempt was not successful.')
        });

        return result;
    }

    const handleSubmit = async (event: SyntheticEvent) => {
        event.preventDefault();
        const c: number = getSelectionCount();

        if (c === 0) {
            alert("Please choose a subscription.");
            return;
        }
        else if (c > 1) {
            alert("Please choose only one subscription.");
            return;
        }

        // get the selected subscription
        const product: string = getSelectedSubscription();

        // if the user is not logged in, prompt them to log in.
        if (globalSettings.UserID === null || globalSettings.UserID.length < 2) {
            alert('You must create an account or sign in with your existing account ID before you can proceed.');
            const isLoggedIn: boolean = SignIn();

            if (!isLoggedIn)
                return;
        }

        // At this point the user is logged in and has made a subscription selection. 
        // Post user identity and subscription selection back to the server for validation.

        const order = {
            UserID: globalSettings.UserID,
            CustomerID: globalSettings.CustomerID,
            SubscriptionID: globalSettings.SubscriptionID,
            PlanPaymentProviderID: product,
            PromoCodes: promoCodes
        }

        let response = await fetch('/subscription/ApproveSubscriptionOrder', {
            method: 'POST',
            headers: {
                'Content-Type': 'application/json;charset=utf-8'
            },
            body: JSON.stringify(order)
        });
    }

    const renderPlans = (plans: SubscriptionPlan[]) => {
        return (
            plans.map((val, i) => (
                <>
                    <div>
                        <input key={i} onClick={handleSelectionChange} type="checkbox" data-providerid={(val).PlanPaymentProviderID}></input>
                    </div>

                    <div>
                        <span key={i}>{(val).PlanDescription}</span>
                    </div>

                    <div>
                        <span key={i}>{(val).MonthlyCost}</span>
                    </div>

                    <div>
                        <span key={i}>6 Months</span>
                    </div>

                    <div>
                        <span key={i}>{(val).Cost}</span>
                    </div>
                </>
            ))
        )
    }


    // -------------------------------------
    const { plans, loading } = (useFetch())

    if (loading || plans === null || plans.length == 0)
        return (<div>loading...</div>);

    return (
        <form  onSubmit={handleSubmit}>

            <div id="promoCodes" >
                <label>Enter promo codes, if any, here.  Seperate multiple codes with a comma:</label>
                <input type="text" value={promoCodes} onChange={handlePromoCodeChange}></input>
            </div>

            <div id="subPlans" className="sub-plan-grid">
                <div>
                    <span>Subscribe</span>
                </div>

                <div>
                    <span>Plan description</span>
                </div>

                <div>
                    <span>Monthly cost</span>
                </div>

                <div>
                    <span>Subscription duration</span>
                </div>

                <div>
                    <span>Total subscription cost</span>
                </div>

                { renderPlans(plans) }

            </div>
            <button type="submit">Continue</button>
        </form>
    )
}
export default Subscriptions;