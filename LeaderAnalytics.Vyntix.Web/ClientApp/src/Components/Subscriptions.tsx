import React from 'react';
import { useState, useContext, SyntheticEvent } from 'react';
import { Image, Button } from 'react-bootstrap';
import { useAsyncEffect } from 'use-async-effect';
import $ from 'jquery';
import { GetSubscriptionPlans } from '../Services/Services';
import SubscriptionPlan from '../Model/SubscriptionPlan';
import { GlobalContext, AppState } from '../AppState';
import { useHistory } from 'react-router-dom'
import { SaveAppState } from '../Services/Services';
import business from '../Assets/business-use.png';
import nonbusiness from '../Assets/non-business-use.png';
import businesstxt from '../Assets/business-use-txt.png';
import nonbusinesstxt from '../Assets/non-business-use-txt.png';
import freetrial from '../Assets/free-trial.png';
import { FontAwesomeIcon } from '@fortawesome/react-fontawesome';
import { faArrowCircleRight } from '@fortawesome/free-solid-svg-icons';

function Subscriptions() {
    const appState: AppState = useContext(GlobalContext);
    const history = useHistory();
    const handleSubmit = async (event: any) => {
        event.preventDefault();
        const elem = document.activeElement as any;
        const planChoice = elem.value;

        if (planChoice === "0")                               
            history.push("/SubPlans"); // Business subscription
        else if (planChoice === "1") {
            // Non-business subscription
            var plan: SubscriptionPlan = new SubscriptionPlan();
            plan.PaymentProviderPlanID = "NONBUSINESS";
            appState.SubscriptionPlan = plan;

            // if the user is not logged in, prompt them to log in.
            if (appState.UserID === null || appState.UserID.length < 2) {
                history.push("/SubSignIn");
            }
            else {
                history.push("/SubConfirmation");
            }
        }
    }

    return (
        <div className="container-fluid content-root light-bg rpt2">
            <div id="banner">
                <div id="bannerHeadline" className="rp1 rh6">
                    <span className="rh5">Choose a subscription plan for your usage scenario</span>
                </div>
            </div>
            <form onSubmit={handleSubmit}>

                <div id="subPlanTypes">
                    <div className="subPlanType">
                        <Image src={business} fluid />
                        <Image src={businesstxt} fluid />
                        <div className="subPlanTypeDescription rp1 rh6">
                            <p>
                                This subscription is intended for companies and individuals who use the service in the operation of
                                a business.
                            </p>
                            <Image src={freetrial} fluid />
                            <p>
                                FREE 30 day trial, no credit card required.
                            </p>
                            <p>
                                Leader Analytics does not sell data and none of our subscription plans include the
                                cost of data from any vendor. 
                            </p>
                            <p>
                                Our tiered pricing is an outstanding value for companies of all sizes.  We offer
                                extraordinary value to small businesses including individual consultants, 
                                freelancers, advisors, and bloggers.  
                            </p>
                      
                        </div>
                        <Button type="submit" value={0} className="continueButton">
                            <div className="rh5">
                                <div>Choose a Business use subscription</div>
                                <FontAwesomeIcon className="rh2" icon={faArrowCircleRight} />
                            </div>
                        </Button>
                    </div>
                    
                    <div className="subPlanType">
                        <Image src={nonbusiness} fluid />
                        <Image src={nonbusinesstxt} fluid />
                        <div className="subPlanTypeDescription rp1 rh6">
                            
                            <p>
                                This subscription may only be used by individual researchers, individual investors, students, and instructors. 
                                It may not be used for any business activity. 
                            </p>
                            <p>
                                A business activity is any activity that directly or indirectly results in payment or 
                                benefit of any kind being received.
                            </p>

                            <div className="nonAcceptableUse rp1">
                                <p>
                                    Examples of business activities where this subscription may not be used include:
                                </p>

                                <ul>
                                    <li>
                                        Bloggers and authors who receive authorship fees, subscription fees, advertising revenue, royalties,
                                        or promotional exposure for a business in which they have an interest.
                                    </li>
                                    <li>Consultants, advisors, and wealth managers who receive payment or benefit of any kind.</li>
                                    <li>Software developers who incorporate the subscription into their product which is sold or licensed.</li>
                                </ul>
                            </div>

                            <div className="acceptableUse rp1">
                                <p>
                                    Activities where this subscription may be used are limited to:
                                </p>

                                <ul>
                                    <li>Individual investors who manage funds for themselves and their immediate families only.</li>
                                    <li>Students and instructors who use the subscription in an academic environment.</li>
                                </ul>
                            </div>
                            <p>
                                Leader Analytics does not sell data and none of our subscription plans include the
                                cost of data from any vendor.
                            </p>
                        </div>
                        <button type="submit" value={1}>Non-business use subscription</button>
                    </div>
                </div>
            </form>
        </div>
    )
}
export default Subscriptions;