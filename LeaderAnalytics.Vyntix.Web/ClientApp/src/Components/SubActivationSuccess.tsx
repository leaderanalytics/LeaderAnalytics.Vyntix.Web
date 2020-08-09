import React,  { useContext } from 'react';
import { useParams } from 'react-router';

function SubActivationSuccess() {

    const { name } = useParams();
    const { otherName } = useParams();
    return (
        <div>
            <div>About {name} and {otherName}</div>

            <div>
                This is SubActivationSuccess
            </div>
        </div>
    )
}
export default SubActivationSuccess;