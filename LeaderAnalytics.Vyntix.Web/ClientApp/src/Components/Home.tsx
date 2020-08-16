import React, {useContext} from 'react';
import { useParams } from 'react-router';
import { GlobalContext, AppState } from '../AppState';

function Home() {

    const appState: AppState = useContext(GlobalContext);
    const userName = appState.UserName;


    return (
        <div>
    

            <div className="rh1">Build models that <i>lead</i></div>
             
        </div>
        

    )
}
export default Home;