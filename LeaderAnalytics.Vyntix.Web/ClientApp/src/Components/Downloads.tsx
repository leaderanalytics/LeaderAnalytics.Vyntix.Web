import React, { useContext } from 'react';
import { NavLink, Link } from 'react-router-dom';
import { FontAwesomeIcon } from '@fortawesome/react-fontawesome';
import {  faTools } from '@fortawesome/free-solid-svg-icons';
import AppInsights from '../Services/AppInsights';

function Downloads() {
    AppInsights.LogPageView("Downloads");
    
    return (

        <div className="container-fluid content-root dark-bg">
            &nbsp;
            <div id="freeSubContainer" className=" rh6 rp1 rm5" >
                <FontAwesomeIcon icon={faTools} className="rh5 rm1" />
                <p>
                    Vyntix is still under development.  No components are available for download at this time. See the <Link className="rh6" to="/Documentation" >documentation</Link> page for estimated availability.
                </p>
                <FontAwesomeIcon icon={faTools} className="rh5 rm1" />
            </div>
        </div>
        

    )
}
export default Downloads;