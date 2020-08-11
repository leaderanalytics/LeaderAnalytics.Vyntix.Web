import React, {useContext} from 'react';
import { useParams } from 'react-router';
import { GlobalContext, AppState } from '../AppState';

function Home() {

    const appState: AppState = useContext(GlobalContext);
    const userName = appState.UserName;


    return (
        <div>
    
            This is home
            <div>UserName is {userName}</div>
             
        </div>
        

    )
}
export default Home;