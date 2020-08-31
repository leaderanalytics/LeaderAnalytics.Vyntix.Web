import React from 'react';
import { useState, useContext, SyntheticEvent } from 'react';
import { useAsyncEffect } from 'use-async-effect';
import $ from 'jquery';
import { GetSubscriptionPlans } from '../Services/Services';
import SubscriptionPlan from '../Model/SubscriptionPlan';
import { GlobalContext, AppState } from '../AppState';
import { useHistory } from 'react-router-dom'
import { SaveAppState, FormatMoney } from '../Services/Services';
import { Button } from 'react-bootstrap';

function SubPlans() {
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

    const handleRowSelectionChange = (event: any) => {
        var target = event.target;
        var plan = target.dataset.providerid;

        if (plan === undefined) {
            while (target.parentNode !== null && target.parentNode !== undefined && plan === undefined) {
                target = target.parentNode;
                plan = target.dataset.providerid;
            }
        }

        setSelectedPlan(plan === selectedPlan ? '' : plan);
    }
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
            plans.filter(x => x.DisplaySequence > 0).sort(x => x.DisplaySequence).map((val, i) => (
                <div  className={(val).PaymentProviderPlanID === selectedPlan ? "gridrow selectedPlan" : "gridrow"} data-providerid={(val).PaymentProviderPlanID} onClick={handleRowSelectionChange}>
                    <div className="cell-sub">
                        <input checked={(val).PaymentProviderPlanID === selectedPlan} onChange={handleSelectionChange} type="checkbox" data-providerid={(val).PaymentProviderPlanID} className="subscribeCheckbox"></input>
                    </div>

                    <div className="cell-desc">
                        <span>{(val).PlanDescription}</span>
                    </div>

                    <div className="cell-cost">
                        <span>{FormatMoney((val).MonthlyCost)}</span>
                    </div>

                    <div className="cell-dur">
                        <span>{(12 / val.BillingPeriods).toString() + ((12 / val.BillingPeriods) === 1 ? ' Month' : ' Months') }</span>
                    </div>

                    <div className="cell-total">
                        <span>{FormatMoney((val).Cost)}</span>
                    </div>
                </div>
            ))
        )
    }


    // -------------------------------------
    const { plans, loading } = (useFetch())

    if (loading || plans === null || plans.length == 0)
        return (<div>loading...</div>);

    return (
        <div className="container-fluid content-root dark-bg">



            <form onSubmit={handleSubmit}>

                <div id="promoCodes" >
                    <label>Enter promo codes, if any, here.  Seperate multiple codes with a comma:</label>
                    <input type="text" value={promoCodes} onChange={handlePromoCodeChange}></input>
                </div>

                <div id="subPlans" className="sub-plan-grid rh6">
                    <div className="sub-plan-grid-header gridheaderrow" >
                        <div className="cell-sub">Select</div>
                        <div className="cell-desc">Plan description</div>
                        <div className="cell-cost">Monthly cost</div>
                        <div className="cell-dur">Duration</div>
                        <div className="cell-total">Total cost</div>
                     </div>   
                    {renderPlans(plans)}
                </div>
                <button type="submit" className="btn btn-primary">Continue</button>
            </form>
        </div>
    )
}
export default SubPlans;