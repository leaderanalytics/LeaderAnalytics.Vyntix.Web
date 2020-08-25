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

    const handleSubmit = async (event: any) => {
        event.preventDefault();
        const elem = document.activeElement as any;
        const planChoice = elem.value;
    }

    return (
        <div className="container-fluid content-root">
            <form  onSubmit={handleSubmit}>

                <div id="subPlanTypes">
                    <div>
                        <button type="submit" value={0}>Non-business use subscription</button>
                    </div>

                    <div>
                        <button type="submit" value={1}>Business use subscription</button>
                    </div>
                </div>
            </form>
        </div>
    )
}
export default Subscriptions;