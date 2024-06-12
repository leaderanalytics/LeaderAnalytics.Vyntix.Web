import React from 'react';
import { Accordion, Card } from 'react-bootstrap';
import AppInsights from '../Services/AppInsights';

function Documentation() {
    AppInsights.LogPageView("Documentation");
    const CardClick = (name: string) => {
        AppInsights.LogEvent("Documentation Card Click", { "Name": name });
    }

    return (
        <div className="container-fluid content-root dark-bg">
            <div id="banner">
                <div className="pageBanner rp1">
                    <span className="rh5">Documentation</span>
                </div>
            </div>


            <div>
                <div className="pageBanner rp1">
                    <span className="rh5">Frequently Asked Questions</span>
                </div>
            </div>


            <Accordion>
                <Accordion.Item eventKey="0">
                    <Accordion.Header className="faq-header" onClick={() => CardClick('0')}>
                        <span className="rh6">When will Vyntix be released?</span>
                    </Accordion.Header>
                    <Accordion.Body>
                            <span className="rh6">
                                Vyntix components are being released on an ongoing basis.  
                                Vyntix FRED client has been released. Desktop components are in development.  
                                All components that are released ahead of Vyntix v1.0 are considered beta and are subject to breaking changes unless stated otherwise.
                            </span>
                    </Accordion.Body>
                </Accordion.Item>

                <Accordion.Item eventKey="1">
                    <Accordion.Header  className="faq-header" onClick={() => CardClick('1')}>
                            <span className="rh6">What will the monthly cost for a subscription be?</span>
                    </Accordion.Header>
                    <Accordion.Body>
                                <span className="rh6">For the lowest tier (individuals and very small organizations) the monthly cost will be about the same as the cost for lunch for one person.  
                                For larger organizations the cost will be about the same as bagels and coffee for the crew.
                                Subscriptions for non-business users will always be free.</span>
                    </Accordion.Body>
                </Accordion.Item>



                <Accordion.Item eventKey="2">
                    <Accordion.Header className="faq-header" onClick={(e) => CardClick('2')}>
                            <span className="rh6">Will Vyntix support data providers other than FRED?</span>
                    </Accordion.Header>
                    <Accordion.Body>
                                <span className="rh6">Yes, we will support data providers as is feasable and as requested by our subscribers.
                                Data providers receiving more requests will be prioritized.  Request your data provider <a href="https://github.com/leaderanalytics/Vyntix/issues" target="_blank">here</a>.
                                </span>
                    </Accordion.Body>
                </Accordion.Item>


                <Accordion.Item eventKey="3">
                    <Accordion.Header className="faq-header" onClick={() => CardClick('3')}>
                            <span className="rh6">What are the differences beween the subscription plans?</span>
                    </Accordion.Header>
                    <Accordion.Body>
                                <span className="rh6">There is only one subscription plan.  It is free for non-business use or priced differently based on revenue of the subscriber.</span>
                    </Accordion.Body>
                </Accordion.Item>


                <Accordion.Item eventKey="4">
                    <Accordion.Header className="faq-header" onClick={() => CardClick('4')}>
                            <span className="rh6">Does my Vyntix subscription include the cost of data?</span>
                    </Accordion.Header>
                    <Accordion.Body>
                                <span className="rh6">No.  At the time of this writng Vyntix only supports FRED - which is free to the general public.  In the future support may be added
                                for data providers who charge for their data.  In these cases the subscriber must purchase data directly from the provider.
                                </span>
                    </Accordion.Body>
                </Accordion.Item>
            </Accordion>

            <div>
                <div className="pageBanner rp1">
                    <span className="rh5">Service Documentation</span>
                </div>
            </div>



            <div className="section-grid">
                <div className="d-flex flex-column" style={{ flexGrow:1 }}>

                    <div className="rml2 rmr2 rmb2">
                        <div className="pageBanner section-header rp1">
                            <span className="rh5">Vyntix FRED Client</span>
                        </div>

                        <div className="section-body rh6 rp1"> 
                            <p><b>Status:</b> Released</p>
                            <p><b>Release Date:</b> Q4 2022</p>
                            <p><b>Description:</b> Access the FRED API and creates data objects that are usable in code.</p>
                            <p><a href="/docs" target="_blank">Documentation</a></p>
                        </div>
                    </div>

                    <div className="rm2">
                        <div className="pageBanner section-header rp1">
                            <span className="rh5">Vyntix Observer CLI</span>
                        </div>

                        <div className="section-body rh6 rp1">
                            <p><b>Status:</b> Windows version released</p>
                            <p><b>Release Date:</b> Q2 2024</p>
                            <p><b>Description:</b>Cross platform (Linux/Mac/Windows) command line utilitiy that a downloads data from FRED and saves it to a database.</p>
                            <p><a href="/docs" target="_blank">Documentation</a></p>
                        </div>
                    </div>


                    <div className="rm2">
                        <div className="pageBanner section-header rp1">
                            <span className="rh5">Vyntix Observer Desktop</span>
                        </div>

                        <div className="section-body rh6 rp1">
                            <p><b>Status:</b> Windows version released</p>
                            <p><b>Release Date:</b> Q2 2024</p>
                            <p><b>Description:</b> Cross platform (Linux/Mac/Windows) desktop utility that a downloads data from FRED and saves it to a database.</p>
                            <p><a href="/docs" target="_blank">Documentation</a></p>
                        </div>
                    </div>

                    <div className="rm2">
                        <div className="pageBanner section-header rp1">
                            <span className="rh5">Vyntix API</span>
                        </div>

                        <div className="section-body rh6 rp1">
                            <p><b>Status:</b> In development</p>
                            <p><b>Expected:</b> Q3 2024</p>
                            <p><b>Description:</b> An API for creating and managing forecasts.</p>
                            <p><a href="/docs" target="_blank">Documentation</a></p>
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