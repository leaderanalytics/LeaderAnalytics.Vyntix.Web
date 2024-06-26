import React from 'react';
import ReactDOM from 'react-dom';
import App from './App';
import * as serviceWorker from './serviceWorker';
import './App.scss';
import './Assets/fonts/entsans.ttf';
import { PublicClientApplication } from "@azure/msal-browser";
import { msalConfig } from "./msalconfig";
import { createRoot } from 'react-dom/client';

const container = document.getElementById('root');
const root = createRoot(container!);
export const msalInstance = new PublicClientApplication(msalConfig);

root.render(
    <React.StrictMode>
        <App pca={msalInstance} />
  </React.StrictMode>
);

// If you want your app to work offline and load faster, you can change
// unregister() to register() below. Note this comes with some pitfalls.
// Learn more about service workers: https://bit.ly/CRA-PWA
serviceWorker.unregister();
