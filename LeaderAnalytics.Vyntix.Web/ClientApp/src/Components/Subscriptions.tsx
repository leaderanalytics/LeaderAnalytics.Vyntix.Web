import React, { useContext } from 'react';
import { useParams } from 'react-router';

function Subscriptons() {

    const { name } = useParams();
    const { otherName } = useParams();
    return (
        <div>
            <div>About {name} and {otherName}</div>

            <div>
                This is About
            </div>
        </div>


    )
}
export default Subscriptons;