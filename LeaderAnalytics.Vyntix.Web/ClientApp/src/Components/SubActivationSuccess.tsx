import React, { useContext, useEffect, useState } from 'react';
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
    const [status, setstatus] = useState("");
    const [isActive, setIsActive] = useState(false);


    useAsyncEffect(async () => {
        await GetSubscriptionInfo(appState);
        
        // If SubscriptionID is null, user has been directed here after requesting a Corporate subscription. Corp subscriptions are not created immediately - a request is emailed to the corp admin first.

        if (IsNullOrEmpty(appState.CustomerID) || IsNullOrEmpty(appState.SubscriptionID)) {
            setstatus("Your subscription request has been created successfully.  You will receive an email notification when your administrator approves your request.");
            setIsActive(false);
        }
        else {
            setstatus("Your subscription is active.");
            setIsActive(true);
        }

        appState.RenderTopNav();
    },[1]);

    const clickHandler = () => {
        history.push("/Downloads");
    }

    return (
        <div className="content-root container-fluid dark-bg center-content flex-column" style={{ alignItems: "stretch" }} >
            <div id="banner">
                <div className="pageBanner rp1">
                    <p className="rh3">Welcome to Vyntix</p>
                    
                </div>
            </div>

            <div className="rm-fallback">
                <p className="rh5 rmt5 center-content">{status}</p>
                <div className="info-box colorSet1 rp1 rmt5">
                    <SelectedPlan/>
                </div>
                {
                    isActive ?
                        <Button onClick={clickHandler} className="iconButton rmt1 rmb1">
                            <div className="rh6">
                                <div>Downloads</div>
                                <FontAwesomeIcon className="rh4" icon={faDownload} />
                            </div>
                        </Button>
                    :
                        <div></div>
                }
            </div>
        </div>
    )
}
export default SubActivationSuccess;