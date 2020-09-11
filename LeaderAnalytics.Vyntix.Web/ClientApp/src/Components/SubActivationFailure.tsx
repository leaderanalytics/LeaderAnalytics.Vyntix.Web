import React, { useContext } from 'react';
import { useHistory } from 'react-router-dom'
import { Button } from 'react-bootstrap';
import { GlobalContext, AppState } from '../AppState';
import { FontAwesomeIcon } from '@fortawesome/react-fontawesome';
import { faArrowCircleRight, faExclamationTriangle } from '@fortawesome/free-solid-svg-icons';



function SubActivationFailure() {
    const appState: AppState = useContext(GlobalContext);
    const history = useHistory();

    const clickHandler = () => {
        appState.Message = '';
        history.push("/Subscriptions");

    }
    return (
        <div className="content-root container-fluid dark-bg" >
            <div id="banner">
                <div className="pageBanner rp1">
                    <span className="rh5">An error occured while activating your subscription</span>
                </div>
            </div>


            <div className="info-box colorSet1 rp1">
                <FontAwesomeIcon className="info-box-icon rh4" icon={faExclamationTriangle}  />

                <div className="info-box-header rmb1">
                    Error message
                </div>

                {
                    appState.Message === '' ?
                        <div>
                            A detailed error message is not available.
                        </div>
                    :
                        <div>
                            {appState.Message}
                        </div>
                }
                

            </div>
            
            <Button  className="iconButton rmt1 rmb1" >
                <div className="rh6">
                    <div>Continue</div>
                    <FontAwesomeIcon className="rh4" icon={faArrowCircleRight} />
                </div>
            </Button>
        </div>
    )
}
export default SubActivationFailure;