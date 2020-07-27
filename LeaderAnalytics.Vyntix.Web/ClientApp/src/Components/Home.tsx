import React, {useContext} from 'react';
import { useParams } from 'react-router';
import { GlobalContext, GlobalSettings } from '../GlobalSettings';

function Home() {

    const globalSettings: GlobalSettings = useContext(GlobalContext);
    const userName = globalSettings.UserName;


    return (
        <div>
    
            This is home
            <div>UserName is {userName}</div>
             
        </div>
        

    )
}
export default Home;