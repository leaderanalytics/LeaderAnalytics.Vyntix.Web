import React, { useContext } from 'react';
import { useHistory } from 'react-router-dom'
import { Button } from 'react-bootstrap';
import { GlobalContext, AppState } from '../AppState';
import { FontAwesomeIcon } from '@fortawesome/react-fontawesome';
import { faArrowCircleRight, faExclamationTriangle } from '@fortawesome/free-solid-svg-icons';
import { GetAppState, SaveAppState } from '../Services/Services';
import AppInsights from '../Services/AppInsights';


function SubActivationFailure() {
    const appState: AppState = GetAppState();
    const history = useHistory();
    const msg = appState.Message;
    AppInsights.LogEvent("SubActivationFailure", { "ErrorMessage": msg });
    appState.Message = '';
    SaveAppState(appState);

    const clickHandler = () => {
        history.push("/Subscriptions");
    }
    return (
        <div className="content-root container-fluid dark-bg" >
            <div id="banner">
                <div className="pageBanner rp1">
                    <span className="rh5">An error occured while activating your subscription</span>
                </div>
            </div>

            <div className="rm-fallback">
                <div className="info-box colorSet1 rp1">
                    <FontAwesomeIcon className="info-box-icon rh4" icon={faExclamationTriangle}  />

                    <div className="info-box-header rmb1">
                        Error message
                    </div>

                    {
                        msg === '' ?
                            <div>
                                A detailed error message is not available.
                            </div>
                        :
                            <div>
                                {msg}
                            </div>
                    }

                </div>

                <Button onClick={clickHandler} className="iconButton rmt1 rmb1" >
                    <div className="rh6">
                        <div>Continue</div>
                        <FontAwesomeIcon className="rh4" icon={faArrowCircleRight} />
                    </div>
                </Button>
            </div>
        </div>
    )
}
export default SubActivationFailure;