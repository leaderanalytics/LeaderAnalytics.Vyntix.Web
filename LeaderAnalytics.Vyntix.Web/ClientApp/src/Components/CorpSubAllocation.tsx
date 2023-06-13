import React, { useContext, useEffect, useState } from 'react';
import { useParams } from 'react-router';
import { Button } from 'react-bootstrap';
import { useNavigate } from 'react-router-dom'
import { GlobalContext, AppState } from '../AppState';
import { FontAwesomeIcon } from '@fortawesome/react-fontawesome';
import { faBirthdayCake, faDownload } from '@fortawesome/free-solid-svg-icons';
import SelectedPlan from './SelectedPlan';
import { IsNullOrEmpty, HandleCorpSubAllocation } from '../Services/Services';
import { useAsyncEffect } from 'use-async-effect';
import AppInsights from '../Services/AppInsights';

//
// Admin user is redirected here from an Email notice when Approving or Declining a request to use a corporate subscription.
// Called from StaticHTML/SubApprovalEmailTemplate.html
//

function CorpSubAllocation() {
    AppInsights.LogPageView("CorpSubResponse");
    const appState: AppState = useContext(GlobalContext);
    const navigate = useNavigate();
    const [msg, setmsg] = useState("");
    const { a } = useParams() as any;   // admin ID
    const { s } = useParams() as any;   // subscriber ID
    const { o } = useParams() as any;   // is approved
    const adminID = a as string;
    const subID = s as string;
    const isapproved = o as string;

    useAsyncEffect(async () => {

        if (IsNullOrEmpty(adminID) || IsNullOrEmpty(subID) || IsNullOrEmpty(isapproved) || (isapproved !== "1" && isapproved !== "0")) {
            setmsg("Invalid number of parameters.")
        }
        else {
            
            var msg = await HandleCorpSubAllocation(adminID, subID, isapproved === "1")
            setmsg(msg);
        }
        
    },[1]);

    const clickHandler = () => {
        navigate("/");
    }

    return (
        <div className="content-root container-fluid dark-bg center-content flex-column" style={{ alignItems: "stretch" }} >
            <div id="banner">
                <div className="pageBanner rp1">
                    <p className="rh3">Corporate Subscription Allocation</p>
                    
                </div>
            </div>

            <div className="rm-fallback">
                <div className="info-box colorSet1 rp1 rmt5">
                    { msg }
                </div>
                
                <Button onClick={clickHandler} className="iconButton rmt1 rmb1">
                    <div className="rh6">
                        <div>Downloads</div>
                        <FontAwesomeIcon className="rh4" icon={faDownload} />
                    </div>
                </Button>
                
            </div>
        </div>
    )
}
export default CorpSubAllocation;