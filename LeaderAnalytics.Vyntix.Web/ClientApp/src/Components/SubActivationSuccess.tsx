import React,  { useContext } from 'react';
import { useParams } from 'react-router';
import { useLocation } from 'react-router';

function SubActivationSuccess() {
    const location = useLocation();
    const sessionID = location.search.split('=')[1]; 

    return (
        <div>
            <div>About {sessionID} </div>

            <div>
                This is SubActivationSuccess
            </div>
        </div>
    )
}
export default SubActivationSuccess;