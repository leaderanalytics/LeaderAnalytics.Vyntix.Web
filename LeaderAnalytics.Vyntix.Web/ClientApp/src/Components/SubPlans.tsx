import React from 'react';
import { useState, useContext, SyntheticEvent } from 'react';
import { NavLink, Link } from 'react-router-dom';
import { useAsyncEffect } from 'use-async-effect';
import { GetSubscriptionPlans } from '../Services/Services';
import SubscriptionPlan from '../Model/SubscriptionPlan';
import { GlobalContext, AppState } from '../AppState';
import { useHistory } from 'react-router-dom'
import { SaveAppState, FormatMoney } from '../Services/Services';
import { Image, Button, Nav, Form } from 'react-bootstrap';
import { FontAwesomeIcon } from '@fortawesome/react-fontawesome';
import { faArrowCircleRight, faStar, faCheck, faTools } from '@fortawesome/free-solid-svg-icons';
import fourGuys from '../Assets/fourguys.jpg';
import blueNeural from '../Assets/blue-neural1.png';
import Dialog from './Dialog';
import DialogType from '../Model/DialogType';
import DialogProps from '../Model/DialogProps';
import AppInsights from '../Services/AppInsights';

function SubPlans() {
    AppInsights.LogPageView("SubPlans");
    const appState: AppState = useContext(GlobalContext);
    const [promoCodes, setPromoCodes] = useState(appState.PromoCodes);
    const [selectedPlan, setSelectedPlan] = useState(appState.SubscriptionPlan);
    const history = useHistory();
    const [plans, setPlans] = useState(new Array<SubscriptionPlan>());
    const [dialogProps, setDialogProps] = useState(new DialogProps("", DialogType.None, () => { }));

    const useFetch = () => {
        const [loading, setLoading] = useState(true);

        useAsyncEffect(async (isMounted) => {

            if (isMounted() && plans.length === 0) {
                const p = await GetSubscriptionPlans();
                setPlans(p);

                if (selectedPlan === null) {
                    setSelectedPlan(p[0]);
                }
            }
            setLoading(false);

        }, [1]);

        return { loading };
    }
    

    const handleDropdownSelectionChange = (event: any) => {
        const p: SubscriptionPlan = plans.filter(x => x.PaymentProviderPlanID === event.target.value)[0];
        setSelectedPlan(p);
    }
    

    const handlePromoCodeChange = (event: any) => setPromoCodes(event.target.value);

    const handleSubmit = async (event: SyntheticEvent) => {
        event.preventDefault();

        if (selectedPlan === null) {
            setDialogProps(new DialogProps("Please choose a subscription plan.", DialogType.Error, () => { setDialogProps(new DialogProps("", DialogType.None, () => { })); }));
            return;
        }

        // get the selected subscription
        appState.SubscriptionPlan = selectedPlan;
        appState.PromoCodes = promoCodes;
        SaveAppState(appState);

        AppInsights.LogEvent("Select Plan", { "PlanName": selectedPlan.PlanDescription });

        // if the user is not logged in, prompt them to log in.
        if (appState.UserID === null || appState.UserID.length < 2) {
            history.push("/SubSignIn");
        }
        else {
            history.push("/SubConfirmation");
        }
    }

    
       

    // -------------------------------------
    

    const { loading } = (useFetch())

    if (loading || plans === null || plans.length == 0)
        return (<div>loading...</div>);


    return (
        <div className="container-fluid content-root dark-bg">
            <Dialog dialogProps={dialogProps} />
            <div className="pageBanner rp1">
                <span className="rh5">Business-use subscription plans</span>
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
                    Vyntix is still under development.  All business use subscriptions are FREE until the service is officially released.
                    Please choose the subscription plan that applies to you from the list below and click Continue.  You will be prompted to create an account however you will not be asked for a 
                    credit card.  You can you renew your subscription at no cost as often as you wish while Vyntix is under development.  Free business use subscriptions will expire
                    approximately thirty days after Vyntix is released. See the <Link className="rh6" to="/Documentation" >Documentation</Link> page for estimated pricing.
                </p>
                <FontAwesomeIcon icon={faTools} className="rh5 rm1" />
            </div>
            

            <form onSubmit={handleSubmit}>

                <div id="promoCodes" className="rmt1 rp1" >
                    <label>Enter promo codes, if any, here.  Separate multiple codes with a comma:</label>
                    <input type="text" value={promoCodes} onChange={handlePromoCodeChange}></input>
                </div>

             

                <div>
                    <div className="rh6 rmt3">
                        Vyntix subscriptions are value priced.  This pricing model allows independent business owners and very small businesses to use a Vyntix subscription at a price they can afford.
                        To determine the cost of your subscription, choose an amount from the list below that reflects your company's total gross income for the last year for which you have completed a tax return.
                    </div>
                    <div className="pricing-grid-collapse rmt3">
                        <div className="planInfoBox">
                            <div className="pageBanner section-header rp1">
                                <span className="rh5">Total gross income</span>
                            </div>
                            <div className="rp1 d-flex justify-content-end">
                                <Form.Control as="select" onChange={handleDropdownSelectionChange} className="planSelector">
                                    {plans.filter(x => x.DisplaySequence > 0).sort(x => x.DisplaySequence).map(x => <option selected={(x.PaymentProviderPlanID === appState.SubscriptionPlan?.PaymentProviderPlanID ?? false)} key={x.PaymentProviderPlanID} value={x.PaymentProviderPlanID} className="rh5">{x.ShortDescription}</option>)}
                                </Form.Control>
                            </div>
                        </div>

                        <div className="planInfoBox">
                            <div className="pageBanner section-header rp1">
                                <span className="rh5">Cost per month</span>
                            </div>
                            <div className="rp1 d-flex justify-content-end">
                                <span className="rh4">{FormatMoney(selectedPlan?.MonthlyCost ?? 0)}</span>
                            </div>

                        </div>

                        <div className="planInfoBox">
                            <div className="pageBanner section-header rp1">
                                <span className="rh5">Duration</span>
                            </div>
                            <div className="rp1 d-flex justify-content-end">
                                <span className="rh4">{12 / (selectedPlan?.BillingPeriods ?? 2)} Months</span>
                            </div>
                        </div>


                        <div className="planInfoBox">
                            <div className="pageBanner section-header rp1">
                                <span className="rh5">Total cost</span>
                            </div>
                            <div className="rp1 d-flex justify-content-end">
                                <span className="rh4">{FormatMoney(selectedPlan?.Cost ?? 0)}</span>
                            </div>
                        </div>
                    </div>
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