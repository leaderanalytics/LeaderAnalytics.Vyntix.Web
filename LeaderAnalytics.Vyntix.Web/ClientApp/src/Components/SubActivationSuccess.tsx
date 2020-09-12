import React, { useContext } from 'react';
import { Button } from 'react-bootstrap';
import { useParams } from 'react-router';
import { useLocation } from 'react-router';
import { useHistory } from 'react-router-dom'
import { GlobalContext, AppState } from '../AppState';
import { FontAwesomeIcon } from '@fortawesome/react-fontawesome';
import { faBirthdayCake, faDownload } from '@fortawesome/free-solid-svg-icons';

function SubActivationSuccess() {
    const appState: AppState = useContext(GlobalContext);
    const history = useHistory();


    const clickHandler = () => {
        history.push("/Downloads");
    }

    return (
        <div className="content-root container-fluid dark-bg" >
            <div id="banner">
                <div className="pageBanner rp1">
                    <span className="rh5">Welcome to Vyntix your subscription is active</span>
                </div>
            </div>


            <div className="info-box colorSet1 rp1">
                <FontAwesomeIcon className="info-box-icon rh4" icon={faBirthdayCake} />

                <div className="info-box-header rmb1">
                    Subscription details
                </div>

                    TO DO : SUBSCRIPTION DETAILS HERE

            </div>

            <Button onClick={clickHandler} className="iconButton rmt1 rmb1" >
                <div className="rh6">
                    <div>Downloads</div>
                    <FontAwesomeIcon className="rh4" icon={faDownload} />
                </div>
            </Button>
        </div>
    )
}
export default SubActivationSuccess;