import React, { useContext, useState } from 'react';
import { GlobalContext, AppState } from '../AppState';
import { FontAwesomeIcon } from '@fortawesome/react-fontawesome';
import { faKey } from '@fortawesome/free-solid-svg-icons';
import { FormatMoney } from '../Services/Services';

function SelectedPlan() {
    const appState: AppState = useContext(GlobalContext);


    return (
        <div className="info-box colorSet1 rp1">
            <FontAwesomeIcon className="info-box-icon rh4" icon={faKey} />

            <div className="info-box-header rmb1">
                Selected subscripton plan
            </div>


            {
                appState.SubscriptionPlan === null ?
                    <div>
                        No subscription plan has been selected.
                    </div>
                    :
                    <>
                        <div className="info-box-grid">
                            <div className="info-box-title rmr1">Plan name:</div>
                            <div className="info-box-text">{appState.SubscriptionPlan.PlanDescription}</div>
                        </div>
                        <div className="info-box-grid">
                            <div className="info-box-title rmr1">Duration:</div>
                            <div className="info-box-text">{(12 / appState.SubscriptionPlan.BillingPeriods).toString() + ((12 / appState.SubscriptionPlan.BillingPeriods) === 1 ? ' Month' : ' Months')}</div>
                        </div>
                        <div className="info-box-grid">
                            <div className="info-box-title rmr1">Total cost:</div>
                            <div className="info-box-text">{FormatMoney(appState.SubscriptionPlan.Cost)}</div>
                        </div>
                    </>
            }

        </div>
    )
}
export default SelectedPlan;
               