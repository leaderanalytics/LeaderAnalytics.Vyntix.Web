import React from 'react';
import { useState, useContext, SyntheticEvent } from 'react';
import { useAsyncEffect } from 'use-async-effect';
import $ from 'jquery';
import { GetSubscriptionPlans } from '../Services/Services';
import SubscriptionPlan from '../Model/SubscriptionPlan';

function Subscriptions() {

    const useFetch = () => {
        var [plans, setPlans] = useState(new Array<SubscriptionPlan>());
        const [loading, setLoading] = useState(true);

        useAsyncEffect(async (isMounted) => {

            if (!isMounted())
                return;

            plans = await GetSubscriptionPlans();

            setPlans(plans)
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
    const toggleChecked = (event: SyntheticEvent) => {
        
        const isChecked: boolean = $(event.target).is(":checked");

        if (!isChecked)
            return;

        $("#subPlans > div > input:checked").prop("checked", false);
        $(event.target).prop("checked", true);
    }

    const handleSubmit = (event: SyntheticEvent) => {
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
        //$.post('/subscription/accountsetup')

    }

    const renderPlans = (plans: SubscriptionPlan[]) => {
        return (
            plans.map((val, i) => (
                <>
                    <div>
                        <input onClick={toggleChecked} type="checkbox" data-providerid={(val).PaymentProviderID}></input>
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