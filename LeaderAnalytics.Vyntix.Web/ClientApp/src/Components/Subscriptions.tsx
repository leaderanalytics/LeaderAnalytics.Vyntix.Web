import React from 'react';
import { useState, useContext, SyntheticEvent } from 'react';
import { useAsyncEffect } from 'use-async-effect';
import $ from 'jquery';
import { GetSubscriptionPlans } from '../Services/Services';
import SubscriptionPlan from '../Model/SubscriptionPlan';
import { GlobalContext, AppState } from '../AppState';
import { useHistory } from 'react-router-dom'
import { SaveAppState } from '../Services/Services';

function Subscriptions() {
    const appState: AppState = useContext(GlobalContext);
    const [promoCodes, setPromoCodes] = useState(appState.PromoCodes);
    const [selectedPlan, setSelectedPlan] = useState(appState.SubscriptionPlan?.PaymentProviderPlanID ?? '');
    const history = useHistory();

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
    
    const handleSelectionChange = (event: any) => setSelectedPlan(event.target.checked ? event.target.dataset.providerid : '');

    const handlePromoCodeChange = (event: any) => setPromoCodes(event.target.value);

    const handleSubmit = async (event: SyntheticEvent) => {
        event.preventDefault();

        if (selectedPlan?.length < 1 ?? true) {
            alert("Please choose a subscription.");
            return;
        }

        // get the selected subscription
        appState.SubscriptionPlan = plans.filter(x => x.PaymentProviderPlanID === selectedPlan)[0];
        appState.PromoCodes = promoCodes;
        SaveAppState(appState);
        
        
        // if the user is not logged in, prompt them to log in.
        if (appState.UserID === null || appState.UserID.length < 2) {
            history.push("/SubSignIn");
        }
        else {
            history.push("/SubConfirmation");
        }
    }

    const renderPlans = (plans: SubscriptionPlan[]) => {
        return (
            plans.map((val, i) => (
                <>
                    <div >
                        <input checked={(val).PaymentProviderPlanID === selectedPlan} onChange={handleSelectionChange} type="checkbox" data-providerid={(val).PaymentProviderPlanID}></input>
                    </div>

                    <div>
                        <span >{(val).PlanDescription}</span>
                    </div>

                    <div>
                        <span>{(val).MonthlyCost}</span>
                    </div>

                    <div>
                        <span>6 Months</span>
                    </div>

                    <div>
                        <span>{(val).Cost}</span>
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
        <div className="container-fluid content-root">
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
        </div>
    )
}
export default Subscriptions;