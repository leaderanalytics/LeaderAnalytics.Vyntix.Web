import React, {useContext} from 'react';
import { useParams } from 'react-router';
import { useHistory } from 'react-router-dom'
import { Container, Image, Row, Col } from 'react-bootstrap';
import { GlobalContext, AppState } from '../AppState';
import { GetSubscriptionInfo } from '../Services/Services';
import { useAsyncEffect } from 'use-async-effect';
import models_that_lead from '../Assets/build_models_that_lead.png';
import managing_the_future from '../Assets/the_platform_for_managing_the_future2.png';
import number_jumble from '../Assets/number_jumble.jpg';
import { FontAwesomeIcon } from '@fortawesome/react-fontawesome';
import { faDatabase, faCheck, faBezierCurve, faMoneyCheckAlt, faChartLine, faSearchDollar } from '@fortawesome/free-solid-svg-icons';

function Home() {
    const history = useHistory();
    const appState: AppState = useContext(GlobalContext);
    const userName = appState.UserName;
    const { id } = useParams() as any;

    useAsyncEffect(async () => {
        if (id !== undefined && id === "1") {
            // After a user navigates to stripe portal, Stripe calls back using url "../lsi/1"
            // This tells us to reload the users subscription info in the event they made some change.
            await GetSubscriptionInfo(appState);
            history.push("/"); // get rid of /lsi/1
        }
    })


    return (
        <div className="content-root">
            <div className="center-content">  
                <Image src={models_that_lead} fluid id="banner-image" />
            </div>

            <div className="container-fluid dark-bg">
                <Container fluid >
                    <Row>
                        <Col className="rm2">
                            <div className="info-box rp1">
                                <FontAwesomeIcon icon={faDatabase} className="info-box-icon rh5" />
                                <div className="info-box-header rm1 rh5">
                                    Data Management
                                </div>
                                <div className="info-box-text rh6">
                                    <ul>
                                        <li>
                                            <div className="info-box-grid rmt1">
                                                <FontAwesomeIcon icon={faCheck}  />
                                                <span>Consume economic, fundamental, and price data from a single API.</span>
                                            </div>
                                        </li>
                                        <li>
                                            <div className="info-box-grid rmt1">
                                                <FontAwesomeIcon icon={faCheck}/>
                                                Use data vintages to avoid look-ahead bias when econonic/fundamental data is revised.
                                            </div>
                                        </li>
                                    </ul>
                                </div>
                            </div>
                        </Col>



                        <Col className="rm2" >
                            <div className="info-box rp1">
                                <FontAwesomeIcon icon={faBezierCurve} className="info-box-icon rh5" />
                                <div className="info-box-header rm1 rh5">
                                    Model Construction
                                </div>
                                <div className="info-box-text rh6">
                                    <ul>
                                        <li>
                                            <div className="info-box-grid rmt1">
                                                <FontAwesomeIcon icon={faCheck} />
                                                Align series by observation date or vintage date.
                                            </div>
                                        </li>
                                        <li>
                                            <div className="info-box-grid rmt1">
                                                <FontAwesomeIcon icon={faCheck} />
                                                Optimize dependent and independent variable selection.
                                            </div>
                                        </li>
                                    </ul>
                                </div>
                            </div>
                        </Col>
                    </Row>
                    <Row>
                        <Col className="rm2" >
                            <div className="info-box rp1">
                                <FontAwesomeIcon icon={faChartLine} className="info-box-icon rh5" />
                                <div className="info-box-header rm1 rh5">
                                    Strategy and Execution
                                </div>
                                <div className="info-box-text rh6">
                                    <ul>
                                        <li>
                                            <div className="info-box-grid rmt1">
                                                <FontAwesomeIcon icon={faCheck}/>
                                                Backtest portfolio and compare performance across models and assets.
                                            </div>
                                        </li>

                                        <li>
                                            <div className="info-box-grid rmt1">
                                                <FontAwesomeIcon icon={faCheck}/>
                                                Continuously integrate forecasts into trading strategy.
                                            </div>
                                        </li>

                                        <li>
                                            <div className="info-box-grid rmt1">
                                                <FontAwesomeIcon icon={faCheck} />
                                                Easily use multiple forecasts from multiple models for trades.
                                            </div>
                                        </li>

                                        <li>
                                            <div className="info-box-grid rmt1">
                                                <FontAwesomeIcon icon={faCheck} />
                                                Real-time broker integration.
                                            </div>
                                        </li>
                                    </ul>
                                </div>
                            </div>
                        </Col>


                        <Col className="rm2" >
                            <div className="info-box rp1">
                                <FontAwesomeIcon icon={faSearchDollar} className="info-box-icon rh5" />
                                <div className="info-box-header rm1 rh5">
                                    Analytics
                                </div>
                                <div className="info-box-text rh6">
                                    <ul>
                                        <li>
                                            <div className="info-box-grid rmt1">
                                                <FontAwesomeIcon icon={faCheck}/>
                                                Save and retrieve forecasts with complete references to the model and parameters that were used to generate them.
                                            </div>
                                        </li>

                                        <li>
                                            <div className="info-box-grid rmt1">
                                                <FontAwesomeIcon icon={faCheck}/>
                                                Use forecasts for charting and analysis exactly as historical data is used.      
                                            </div>
                                        </li>
                                    </ul>
                                </div>
                            </div>
                        </Col>
                    </Row>
                </Container>
            </div>

            <div className="container-fluid light-bg">
                <div className=" rh6 rp2" >
                    <div className="center-content rml1">
                        <Image src={managing_the_future} fluid />
                    </div>

                    <div className="rh6"  >
                        <div id="number_jumble" className="float-right" >
                            <Image src={number_jumble}  />
                        </div>

                        <p>
                            Vyntix simplifies forecast management. 
                        </p>

                        <p>
                            When you create a forecast and save it you simply merge your forecast into the same data file as your historical data.  
                            This allows you to query and plot your forecasts in exactly the same way you query and plot historical data.
                        </p>

                        <p>
                            When you save a forecast in Vyntix, you also save a bit of metadata that allows you to trace your forecast back to the model and 
                            the parameters that were used to create it.  
                        </p>

                        <p>
                            Vyntix natively supports the concept of data vintages as part of it's unique data file structure.  Data vintages allow you to continuously merge
                            your forecasts into your data stream without deleting or overwriting prior forecasts.  You can merge forecasts for various future periods and add new
                            forecasts as the forecast window closes.  You can also merge forecasts from any number of models using multiple parameter configurations. 
                        </p>

                        <p>
                            Data vintages are also useful for when you want to avoid look-ahead bias when using a historical cut-off period for dependent variables.  
                            See the documentation for more information.
                        </p>

                        <p>
                            Because you have a single, searchable repository you can continuously refine your forecasts via a process of systemic elimination.  
                            You can also easily search expired forecasts thus avoiding building the same model twice.
                        </p>

                        <p>
                            You can use data from any data provider in Vyntix.  Vyntix includes a downloader for <a href="https://fred.stlouisfed.org/" target="_blank" >FRED</a> that 
                            allows you to download and use data vintages that are maintained by the Federal Reserve Bank of St. Louis.
                        </p>
                    </div>
                </div>
            </div>
        </div>
    )
}
export default Home;