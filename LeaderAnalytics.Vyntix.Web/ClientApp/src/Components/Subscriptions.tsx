﻿import React from 'react';
import { useState, useContext, SyntheticEvent } from 'react';
import { Image, Button } from 'react-bootstrap';
import { useAsyncEffect } from 'use-async-effect';
import $ from 'jquery';
import { GetSubscriptionPlans } from '../Services/Services';
import SubscriptionPlan from '../Model/SubscriptionPlan';
import { GlobalContext, AppState } from '../AppState';
import { useNavigate } from 'react-router-dom'
import { SaveAppState } from '../Services/Services';
import business from '../Assets/business-use.jpg';
import nonbusiness from '../Assets/non-business-use.jpg';
import businesstxt from '../Assets/business-use-txt.png';
import nonbusinesstxt from '../Assets/non-business-use-txt.png';
import freetrial from '../Assets/free-trial.png';
import { FontAwesomeIcon } from '@fortawesome/react-fontawesome';
import { faArrowCircleRight } from '@fortawesome/free-solid-svg-icons';
import AppInsights from '../Services/AppInsights';

function Subscriptions() {
    AppInsights.LogPageView("Subscriptions");
    const appState: AppState = useContext(GlobalContext);
    const navigate = useNavigate();

    const handleSubmit = async (event: any) => {
        event.preventDefault();
        const elem = document.activeElement as any;
        const planChoice = elem.value;
        
        if (planChoice === "0") {
            AppInsights.LogEvent("Select Business Subscription");
            navigate("/SubPlans"); // Business subscription
        }
        else if (planChoice === "1") {
            AppInsights.LogEvent("Select Non-Business Subscription");
            // Non-business subscription
            appState.SubscriptionPlan = (await GetSubscriptionPlans()).filter(x => x.PlanDescription.indexOf('Free non-business') > -1)[0];

            if (appState.SubscriptionPlan === null)
                throw new Error("Non-business plan was not found.");

            SaveAppState(appState);

            // if the user is not logged in, prompt them to log in.
            if (appState.UserID === null || appState.UserID.length < 2) {
                navigate("/SubSignIn");
            }
            else {
                navigate("/SubConfirmation");
            }
        }
    }

    

    return (
        <div className="container-fluid content-root dark-bg rpt2">
            <div id="banner">
                <div className="pageBanner rp1">
                    <span className="rh5">Choose a subscription plan for your usage scenario</span>
                </div>
            </div>
            <form onSubmit={handleSubmit}>

                <div id="subPlanTypes">
                    <div className="subPlanType">
                        <Image src={business} fluid  />
                        <Image src={businesstxt} className="subTypeText" />
                        <div className="subPlanTypeDescription rp1 rh6">
                            <p>
                                This subscription is intended for companies and individuals who use the service in the operation of
                                a business.
                            </p>
                            <div id="freeTrialImg">
                                <Image src={freetrial} fluid />
                            </div>
                            
                            <p>
                                Our tiered pricing is an outstanding value for companies of all sizes.  We offer
                                extraordinary value to small businesses as well as individual consultants, 
                                freelancers, advisors, and bloggers.  
                            </p>

                            <p>
                                Start managing your forecasts today.  Click the button below to get started with 
                                a FREE 30 day trial or a subscription sized for your business.
                            </p>
                      
                        </div>
                        <Button type="submit" value={0} className="iconButton">
                            <div className="rh6">
                                <div>Choose a business use subscription</div>
                                <FontAwesomeIcon className="rh4" icon={faArrowCircleRight} />
                            </div>
                        </Button>
                    </div>
                    
                    <div className="subPlanType">
                        <Image src={nonbusiness} fluid />
                        <Image src={nonbusinesstxt} className="subTypeText" />
                        <div className="subPlanTypeDescription rp1 rh6">
                            
                            <p>
                                This subscription may only be used by individual researchers, individual investors, students, and instructors. 
                                It may not be used for any business activity. 
                            
                                A business activity is any activity that is performed with an expectation of payment or benefit of any kind being received.
                            </p>

                            <p id="nonBusinessFreeText" className="rp1">
                                Non-business use subscriptions are FREE and include all of the functionality of business use subscriptions.
                            </p>

                            <div className="nonAcceptableUse rp1 rh7">
                                <p>
                                    Examples of business activities where this subscription may not be used include:
                                </p>

                                <ul>
                                    <li>
                                        Bloggers, authors, consultants, advisors, and wealth managers who work with the 
                                        expectation of payment or benefit of any kind.
                                        Benefit incluces authorship fees, subscription fees, advertising revenue, royalties, profit sharing, capital gains,
                                        or promotional exposure for a business in which there is an interest.
                                    </li>
                                    
                                    <li>Any activity within any group or organization that works with the expectation of payment, benefit, or profit.</li>
                                </ul>
                            </div>

                            <div className="acceptableUse rp1 rh7">
                                <p>
                                    Activities where this subscription may be used are limited to:
                                </p>

                                <ul>
                                    <li>Individual investors who manage funds for themselves and their immediate family only.</li>
                                    <li>Students and instructors who use the subscription to fulfill the requirements of their academic coursework.</li>
                                </ul>
                            </div>
                      
                        </div>
                        <Button type="submit" value={1} className="iconButton">
                            <div className="rh6">
                                <div>Choose a non-business use subscription</div>
                                <FontAwesomeIcon className="rh4" icon={faArrowCircleRight} />
                            </div>
                        </Button>
                    </div>
                </div>
            </form>
        </div>
    )
}
export default Subscriptions;