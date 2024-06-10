import React, { useContext, useState } from 'react';
import { NavLink, Link } from 'react-router-dom';
import { FontAwesomeIcon } from '@fortawesome/react-fontawesome';
import {  faTools } from '@fortawesome/free-solid-svg-icons';
import AppInsights from '../Services/AppInsights';
import { Image, Button } from 'react-bootstrap';
import image4_small from '../Assets/ObserverDesktop/ObserverDesktop04_small.jpg';
import odimage1 from '../Assets/ObserverDesktop/ObserverDesktop01.jpg';
import odimage2 from '../Assets/ObserverDesktop/ObserverDesktop02.jpg';
import odimage3 from '../Assets/ObserverDesktop/ObserverDesktop03.jpg';
import odimage4 from '../Assets/ObserverDesktop/ObserverDesktop04.jpg';
import odimage5 from '../Assets/ObserverDesktop/ObserverDesktop05.jpg';
import odimage6 from '../Assets/ObserverDesktop/ObserverDesktop06.jpg';
import odimage7 from '../Assets/ObserverDesktop/ObserverDesktop07.jpg';
import odimage8 from '../Assets/ObserverDesktop/ObserverDesktop08.jpg';
import { faDownload } from '@fortawesome/free-solid-svg-icons';
import SlideShow from './SlideShow';
import SlideShowProps from '../Model/SlideShowProps';
import DialogType from '../Model/DialogType';

function Downloads() {
    AppInsights.LogPageView("Downloads");
    const observerDesktopImages = [
        odimage1,
        odimage2,
        odimage3,
        odimage4,
        odimage5,
        odimage6,
        odimage7,
        odimage8
    ]
    const [slideShowProps, setSlideShowProps] = useState(new SlideShowProps(observerDesktopImages, DialogType.None, () => { }));

    const ShowSlideShow = async () => {

        setSlideShowProps(new SlideShowProps(observerDesktopImages, DialogType.Info, () => { setSlideShowProps(new SlideShowProps([], DialogType.None, () => { })); }));
    };
    return (

        <div className="container-fluid content-root dark-bg">
            <SlideShow slideShowProps={slideShowProps}/>
            <div className="appList">
                <div id="banner">
                    <div className="pageBanner rp1">
                        <span className="rh5">Observer Desktop</span>
                    </div>
                </div>
                <div className="app">
                    <div className="appImages">
                        <button>
                            <Image src={image4_small} fluid className="appImage" onClick={ShowSlideShow} />
                        </button>
                    </div>

                    <div className="appDescription">
                        <p>
                            Observer Desktop is a utility for downloading data from the FRED API and saving it in a database on your local desktop or laptop machine.
                        </p>
                        <p>
                            Observer Desktop has powerful export features that allow you to export data from your local database to an Excel or CSV file.
                            You can export vintage data in list or matrix format.
                        </p>
                        <p>
                            Series can be viewed in list format or in a hierarchical format by FRED category.
                        </p>
                        <p>
                            The program downloads the following data elements:  Series meta data, observations, vintages, series tags, releases, release dates, sources,
                            child categories, related categories, category tags.
                        </p>
                        <p>
                            Observer Desktop contains features that allow you view data as a list, a matrix (pivot table), or a graph.  You can quickly view properties of a
                            series such as frequency and units.  Data can be viewed (and exported) in sparse or dense format.  Sparse format contains only data that is
                            changed for each vintage. Dense format repeats unchanged data from prior vintages.  Dense data is often easier to work with in a spreadsheet
                            and is sometimes easier to visualize.

                        </p>
                        <p>
                            NOTE: In order to use Observer Desktop you must install a Database management system (DBMS) such as Microsoft SQL Server or MySQL on your machine, or you must have access to a DBMS on your local area network.  See the <a href="https://vyntix.com/docs/observer/latest/intro.html" target="_blank">documentation</a> for more detail .
                        </p>

                      
                        <a
                            href="https://github.com/leaderanalytics/Observer.Desktop/releases/download/windows%2F1.0.10.1/LeaderAnalytics.Vyntix.Observer.Desktop-win-Setup.exe"
                            download="Observer Desktop for Windows"
                            
                            rel="noreferrer">
                            <Button className="iconButton">
                                <div className="rh6">
                                    <div>Download Observer Desktop for Windows</div>
                                    <FontAwesomeIcon className="rh4" icon={faDownload} />
                                </div>
                            </Button>
                        </a>
                    </div>
                </div>
            </div>
        </div>
        

    )
}
export default Downloads;