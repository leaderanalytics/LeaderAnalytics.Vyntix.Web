import React, { useContext, useEffect } from 'react';
import { Button } from 'react-bootstrap';
import { useHistory } from 'react-router-dom'
import { GlobalContext, AppState } from '../AppState';
import { FontAwesomeIcon } from '@fortawesome/react-fontawesome';
import { faBirthdayCake, faDownload } from '@fortawesome/free-solid-svg-icons';
import SelectedPlan from './SelectedPlan';
import { IsNullOrEmpty, GetSubscriptionInfo } from '../Services/Services';
import { useAsyncEffect } from 'use-async-effect';
import AppInsights from '../Services/AppInsights';

function SubActivationSuccess() {
    AppInsights.LogPageView("SubActivationSuccess");
    const appState: AppState = useContext(GlobalContext);
    const history = useHistory();



    useAsyncEffect(async () => {
        await GetSubscriptionInfo(appState);

        // After reloading appState, make sure user is signed in and has a valid subscription.  Redirect to home if not.
        if (IsNullOrEmpty(appState.CustomerID) || IsNullOrEmpty(appState.SubscriptionID))
            history.push("/Home");

        appState.RenderTopNav();
    },[1]);

    const clickHandler = () => {
        history.push("/Downloads");
    }

    return (
        <div className="content-root container-fluid dark-bg" >
            <div id="banner">
                <div className="pageBanner rp1">
                    <p className="rh5">Welcome to Vyntix</p>
                    <p className="rh5">Your subscription is active</p>
                </div>
            </div>

            <div className="rm-fallback">
                <div className="info-box colorSet1 rp1">
                    <SelectedPlan/>
                </div>

                <Button onClick={clickHandler} className="iconButton rmt1 rmb1" >
                    <div className="rh6">
                        <div>Downloads</div>
                        <FontAwesomeIcon className="rh4" icon={faDownload} />
                    </div>
                </Button>
            </div>
        </div>
    )
}
export default SubActivationSuccess;