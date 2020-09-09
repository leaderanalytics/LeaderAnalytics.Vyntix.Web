import React from 'react';
import { useState, useContext, SyntheticEvent } from 'react';
import { NavLink, Link } from 'react-router-dom';
import { useAsyncEffect } from 'use-async-effect';
import { GetSubscriptionPlans } from '../Services/Services';
import SubscriptionPlan from '../Model/SubscriptionPlan';
import { GlobalContext, AppState } from '../AppState';
import { useHistory } from 'react-router-dom'
import { SaveAppState, FormatMoney } from '../Services/Services';
import { Image, Button, Nav } from 'react-bootstrap';
import { FontAwesomeIcon } from '@fortawesome/react-fontawesome';
import { faArrowCircleRight, faStar, faCheck, faTools } from '@fortawesome/free-solid-svg-icons';
import fourGuys from '../Assets/fourguys1.jpg';
import blueNeural from '../Assets/blue-neural1.png';
import Dialog from './Dialog';


function SubPlans() {
    const appState: AppState = useContext(GlobalContext);
    const [promoCodes, setPromoCodes] = useState(appState.PromoCodes);
    const [selectedPlan, setSelectedPlan] = useState(appState.SubscriptionPlan?.PaymentProviderPlanID ?? '');
    const [message, setMessage] = useState('');
    const history = useHistory();

    

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
            setMessage("Please choose a subscription plan.");
            return;
        }

        // get the selected subscription
        appState.SubscriptionPlan = appState.SubscriptionPlans.filter(x => x.PaymentProviderPlanID === selectedPlan)[0];
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
    



    return (
        <div className="container-fluid content-root dark-bg">
            <Dialog message={message} callback={() => setMessage('')} />
            <div className="pageBanner rp1">
                <span className="rh5">Subscription plans</span>
            </div>

            <div id="subPlansTopContainer">
                <div id="subPlansLeft">
                    <Image src={fourGuys} id="fourGuysImg" />
                </div>

                <div id="subPlansRight">

                    <div className="disclosureCard colorSet1 rp1 rmb1 rm1">
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

            <div id="freeSubContainer" className=" rh6 rp1 rmt1">
                <FontAwesomeIcon icon={faTools} className="rh5 rm1" />
                <p>
                    Vyntix is still under development.  All business use subscriptons are FREE until the service is officially released.
                    Please choose the subscription plan that applies to you from the list below and click Continue.  You will be prompted to create an account however you will not be asked for a 
                    credit card.  You can you renew your subscription at no cost as often as you wish while Vyntix is under develpment.  Free business use subscriptions will expire
                    approximately thirty days after Vyntix is released. See the <Link className="rh6" to="/Documentation" >Documentation</Link> page for estimated pricing.
                </p>
                <FontAwesomeIcon icon={faTools} className="rh5 rm1" />
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
                    {renderPlans(appState.SubscriptionPlans)}
                </div>
                <Button type="submit" value={0} className="iconButton rmt1 rmb1" >
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