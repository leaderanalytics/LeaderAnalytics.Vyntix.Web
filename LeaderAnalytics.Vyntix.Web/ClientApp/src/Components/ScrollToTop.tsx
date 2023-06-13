import React from 'react';
import { useEffect } from 'react';
import { useLocation, useNavigate, useParams } from 'react-router-dom';

function ScrollToTop(props: any) {
    let location = useLocation();

    useEffect(() => {
        
            window.scrollTo(0, 0);
        
    }, [location]);

    return (null);
}

function withRouter(Component:any) {
    function ComponentWithRouterProp(props:any) {
        let location = useLocation();
        let navigate = useNavigate();
        let params = useParams();
        return (
            <Component
                {...props}
                router={{ location, navigate, params }}
            />
        );
    }

    return ComponentWithRouterProp;
}

export default withRouter(ScrollToTop);