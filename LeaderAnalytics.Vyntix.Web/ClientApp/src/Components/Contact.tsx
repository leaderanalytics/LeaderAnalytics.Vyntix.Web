import React, { useEffect, useState } from 'react';
import { useLocation } from 'react-router';
import { useAsyncEffect } from 'use-async-effect';
import { GetPerson } from '../Services/Services';

function Contact() {
    const location: any = useLocation();
    var currentPath = location.pathname;
    var searchParams = new URLSearchParams(location.search);
    const [count, setCount] = useState(0);
    const [data, setData] = useState(null);
    const [loading, setLoading] = useState(true);

    useAsyncEffect(async (isMounted) => {

        if (!isMounted())
            return;

        const tmp = await GetPerson();
        setData(tmp);
        setLoading(false);
        
    }, [count]);

    useEffect(() => {
        currentPath = location.pathname;
        searchParams = new URLSearchParams(location.search);
    }, [location]);


    return (
        <div>
            <div>Contact</div>
            <div>Location: </div>
            <div>search: </div>
            <div>
                <button onClick={() => setCount(count + 1)}>Get a user</button>
            </div>
            <div>Count is: {count}</div>
            {loading ? <div>...loading</div> : <div>{(data as any).name.first}</div>}
        </div>
    )
}
export default Contact;