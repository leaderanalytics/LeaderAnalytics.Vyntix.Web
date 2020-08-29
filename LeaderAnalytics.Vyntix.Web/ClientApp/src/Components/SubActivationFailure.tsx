import React,  { useContext } from 'react';
import { useParams } from 'react-router';

function SubActivationFailure() {

    const  name  = useParams();
    const  otherName  = useParams();
    return (
        <div>
            <div>About {name} and {otherName}</div>

            <div className="container-fluid content-root">
                This is SubActivationFailure
            </div>
        </div>
        

    )
}
export default SubActivationFailure;