import React, { useContext } from 'react';
import { Accordion, Card } from 'react-bootstrap';

function Documentation() {

    return (
        <div className="container-fluid content-root dark-bg">
            <div id="banner">
                <div className="pageBanner rp1">
                    <span className="rh5">Documentation</span>
                </div>
            </div>


            <div>
                <div className="pageBanner rp1">
                    <span className="rh3">Frequently Asked Questions</span>
                </div>
            </div>


            <Accordion>


                <Card>
                    <Accordion.Toggle as={Card.Header} eventKey="0" className="faq-header">
                        <span className="rh5">When will Vyntix be released?</span>
                    </Accordion.Toggle>
                    <Accordion.Collapse eventKey="0">
                        <Card.Body>
                            <span className="rh6">
                                Vyntix will be released in phases starting with the Vyntx FRED Client in late Q4 2020.  
                                This will be followed by the Vyntix FRED Downloader, then the Vyntix Data Manager in the subsequent  two quarters.
                                All components that are released ahead of Vyntix v1.0 are considered alpha and are subject to breaking changes unless stated otherwise.
                            </span>
                        </Card.Body>
                    </Accordion.Collapse>
                </Card>



                <Card>
                    <Accordion.Toggle as={Card.Header} eventKey="1" className="faq-header">
                        <span className="rh5">What will the monthly cost for a subscription be?</span>
                    </Accordion.Toggle>
                    <Accordion.Collapse eventKey="1">
                        <Card.Body>
                            <span className="rh6">For the lowest tier (individuals and very small organizations) the monthly cost will be about the same as the cost for lunch for one person.  
                            For larger organizations the cost will be about the same as bagels and coffee for the crew.
                            Subscriptions for non-business users will always be free.</span>
                        </Card.Body>
                    </Accordion.Collapse>
                </Card>



                <Card>
                    <Accordion.Toggle as={Card.Header} eventKey="2" className="faq-header">
                        <span className="rh5">Will Vyntix support data providers other than FRED?</span>
                    </Accordion.Toggle>
                    <Accordion.Collapse eventKey="2">
                        <Card.Body>
                            <span className="rh6">Yes, we will support data providers as is feasable and as requested by our subscribers.
                            Data providers receiving more requests will be prioritized.  Request your data provider <a href="https://github.com/leaderanalytics/Vyntix/issues" target="_blank">here</a>.
                            </span>
                        </Card.Body>
                    </Accordion.Collapse>
                </Card>


                <Card>
                    <Accordion.Toggle as={Card.Header} eventKey="3" className="faq-header">
                        <span className="rh5">What are the differences beween the subscription plans?</span>
                    </Accordion.Toggle>
                    <Accordion.Collapse eventKey="3">
                        <Card.Body>
                            <span className="rh6">There is only one subscription plan.  It is priced differently based on the company size of the subscriber.</span>
                        </Card.Body>
                    </Accordion.Collapse>
                </Card>


                <Card>
                    <Accordion.Toggle as={Card.Header} eventKey="4" className="faq-header">
                        <span className="rh5">Does my Vyntix subscription include the cost of data?</span>
                    </Accordion.Toggle>
                    <Accordion.Collapse eventKey="4">
                        <Card.Body>
                            <span className="rh6">No.  At the time of this writng Vyntix only supports FRED - which is free to the general public.  In the future support may be added
                            for data providers who charge for their data.  In these cases the subscriber must purchase data directly from the provider.
                            </span>
                        </Card.Body>
                    </Accordion.Collapse>
                </Card>
            </Accordion>

            <div>
                <div className="pageBanner rp1">
                    <span className="rh3">Service Documentation</span>
                </div>
            </div>



            <div className="section-grid">
                <div className="d-flex flex-column" style={{ flexGrow:1 }}>

                    <div className="rml2 rmr2 rmb2">
                        <div className="pageBanner section-header rp1">
                            <span className="rh4">Vyntix FRED Client</span>
                        </div>

                        <div className="section-body rh6 rp1"> 
                            <p><b>Status:</b> In development</p>
                            <p><b>Expected:</b> Q4 2020</p>
                            <p><b>Description:</b> Access the FRED API and creates data objects that are usable in code.</p>
                            <p><a href="https://leaderanalytics.github.io/Vyntix/" target="_blank">Documentation</a></p>
                        </div>
                    </div>

                    <div className="rm2">
                        <div className="pageBanner section-header rp1">
                            <span className="rh4">Vyntix FRED Downloader</span>
                        </div>

                        <div className="section-body rh6 rp1">
                            <p><b>Status:</b> In development</p>
                            <p><b>Expected:</b> Q1 2021</p>
                            <p><b>Description:</b> Cross platform (Linux/Mac/Windows) command line utilitiy that a downloads data from FRED and saves it to a file or a database.</p>
                            <p><a href="https://leaderanalytics.github.io/Vyntix/" target="_blank">Documentation</a></p>
                        </div>
                    </div>


                    <div className="rm2">
                        <div className="pageBanner section-header rp1">
                            <span className="rh4">Vyntix Data Manager</span>
                        </div>

                        <div className="section-body rh6 rp1">
                            <p><b>Status:</b> In development</p>
                            <p><b>Expected:</b> Q2 2021</p>
                            <p><b>Description:</b> Windows desktop utility for managing data from multiple data providers.</p>
                            <p><a href="https://leaderanalytics.github.io/Vyntix/" target="_blank">Documentation</a></p>
                        </div>
                    </div>

                    <div className="rm2">
                        <div className="pageBanner section-header rp1">
                            <span className="rh4">Vyntix API</span>
                        </div>

                        <div className="section-body rh6 rp1">
                            <p><b>Status:</b> In development</p>
                            <p><b>Expected:</b> Q3 2021</p>
                            <p><b>Description:</b> An API for creating and managing forecasts.</p>
                            <p><a href="https://leaderanalytics.github.io/Vyntix/" target="_blank">Documentation</a></p>
                        </div>
                    </div>


                    <div className="rm2">
                        <div className="pageBanner section-header rp1">
                            <span className="rh4">Vyntix Desktop</span>
                        </div>

                        <div className="section-body rh6 rp1">
                            <p><b>Status:</b> In development</p>
                            <p><b>Expected:</b> Q3 2021</p>
                            <p><b>Description:</b> Windows desktop application for managing forecasts.</p>
                            <p><a href="https://leaderanalytics.github.io/Vyntix/" target="_blank">Documentation</a></p>
                        </div>
                    </div>
                </div>



                <div id="fred-widget">
                    <iframe style={{ border: "1px solid #333333", overflow: "hidden" }}
                        src="//research.stlouisfed.org/fred-glance-widget.php" height="490px" width="320px"
                         frameBorder="0" scrolling="no">
                    </iframe>
                </div>
            </div>
        </div>
    )
}
export default Documentation;