import React from 'react';
import { useState, useContext, SyntheticEvent } from 'react';
import { useAsyncEffect } from 'use-async-effect';
import { GetSubscriptionPlans } from '../Services/Services';
import SubscriptionPlan from '../Model/SubscriptionPlan';
import { GlobalContext, AppState } from '../AppState';
import { useHistory } from 'react-router-dom'
import { SaveAppState, FormatMoney } from '../Services/Services';
import { Image, Button } from 'react-bootstrap';
import { FontAwesomeIcon } from '@fortawesome/react-fontawesome';
import { faArrowCircleRight, faStar, faCheck } from '@fortawesome/free-solid-svg-icons';
import fourGuys from '../Assets/fourguys.jpg';
import blueNeural from '../Assets/blue-neural.png';

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

            <div className="pageBanner rp1">
                <span className="rh5">Subscription plans</span>
            </div>

            <div id="subPlansTopContainer">
                <div id="subPlansLeft">
                    <Image src={fourGuys} id="fourGuysImg" />
                </div>

                <div id="subPlansRight">

                    <div className="disclosureCard rp1 rmb1 rm1">
                        <div className="disclosureCardHeader">
                            <FontAwesomeIcon className="rh4" icon={faStar} />
                            <p className="rh5">
                                Pricing and quality guarantee
                            </p>
                        </div>
                        <p className="rh6">
                            <FontAwesomeIcon icon={faCheck} />
                            Our pricing is completely transparent.  There are no upsells, undocumented limitations, or hidden charges.
                        </p>
                        <p className="rh6">
                            <FontAwesomeIcon icon={faCheck} />
                            Our software contains no spyware, adware, or any other kind of malware.
                        </p>
                    </div>
                </div>
            </div>

            

            <form onSubmit={handleSubmit}>

                <div id="promoCodes" className="rmt1 rmb1 rp1" >
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
                <Button type="submit" value={0} className="continueButton">
                    <div className="rh6">
                        <div>Continue</div>
                        <FontAwesomeIcon className="rh4" icon={faArrowCircleRight} />
                    </div>
                </Button>
            </form>
        </div>
    )
}
export default SubPlans;