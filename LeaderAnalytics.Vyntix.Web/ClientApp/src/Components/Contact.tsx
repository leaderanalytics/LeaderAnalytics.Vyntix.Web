import React, { useEffect, useState } from 'react';
import { useLocation } from 'react-router';
import { useAsyncEffect } from 'use-async-effect';

function Contact() {
    const location: any = useLocation();
    var currentPath = location.pathname;
    var searchParams = new URLSearchParams(location.search);
    const [count, setCount] = useState(0);


    const useFetch = (url: string) => {
        const [data, setData] = useState(null);
        const [loading, setLoading] = useState(true);

        useAsyncEffect(async (isMounted) => {
            const response = await fetch(url);
            const data = await response.json();
            const item = data.results[0];
            const [item2] = data.results;

            if (!isMounted())
                return;

            setData(item);
            setLoading(false);

        }, [count]);

        return { data, loading };
    };

    const { data, loading } = (useFetch('https://api.randomuser.me/') as any);

    useEffect(() => {
        currentPath = location.pathname;
        searchParams = new URLSearchParams(location.search);
    }, [location]);


    return (
        <div>
            <div>Contact</div>
            <div>Location: {currentPath}</div>
            <div>search: {searchParams}</div>
            <div>
                <button onClick={() => setCount(count + 1)}>Get a user</button>
            </div>
            <div>Count is: {count}</div>
            {loading ? <div>...loading</div> : <div>{(data as any).name.first}</div>}
        </div>
    )
}
export default Contact;