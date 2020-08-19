import React, {useContext} from 'react';
import { useParams } from 'react-router';
import { Container, Image, Row, Col } from 'react-bootstrap';
import { GlobalContext, AppState } from '../AppState';
import platform from '../Assets/platform1.png';
import { FontAwesomeIcon } from '@fortawesome/react-fontawesome';
import { faDatabase, faCheck, faBezierCurve, faMoneyCheckAlt, faChartLine, faSearchDollar } from '@fortawesome/free-solid-svg-icons';

function Home() {

    const appState: AppState = useContext(GlobalContext);
    const userName = appState.UserName;


    return (
        <div>
            <div className="center-content">  
                <Image src={platform} fluid id="banner-image" />
            </div>

            <div className="container-fluid">
                <Container fluid>
                    <Row>
                        <Col className="rmt1">
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



                        <Col className="rmt1" >
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
                        <Col className="rmt1" >
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


                        <Col className="rmt1" >
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
        </div>
    )
}
export default Home;