import { useEffect } from 'react';
import { withRouter } from 'react-router-dom';
import { useHistory } from 'react-router-dom'

function ScrollToTop(props: any) {
    const history = useHistory();

    useEffect(() => {
        const unlisten = history.listen(() => {
            window.scrollTo(0, 0);
        });
        return () => {
            unlisten();
        }
    }, []);

    return (null);
}

export default withRouter(ScrollToTop);