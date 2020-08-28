import React,  { useContext } from 'react';


function Documentation() {

    return (
        <div className="container-fluid content-root">
            <div className="rh5">
                <div>
                    What's included in a Vyntix subscription
                </div>
                <div>
                    Company pricing features generous usage provisions. WHICH ARE??
                </div>

                <div className="rh5">
                    What's not included in a Vyntix subscription
                </div>

                <div className="rh6">
                    <p>
                        Our service does not include spyware or advertisments.  We do not disclose
                        information about our subscribers to any third party.  We send infrequent email
                        newsletters to our subscribers.  These newsletters contain information of value such
                        as development plans for requested features, usage tips, etc.  Email newsletters
                        can be discontinued by the subscriber at any time.
                        </p>
                    <p>
                        Leader Analytics does not sell data and none of our subscription plans include the
                        cost of data from any vendor.  Vyntix subscriptions include use of the Vyntix FRED Downloader
                            which is a utility that downloads data from the <a href="https://fred.stlouisfed.org/docs/api/fred/" target="_blank">FRED API</a>.
                            Data from the FRED API is available to the general public at no cost.
                            In the future, Leader Analytics may introduce additional downloaders that work with
                            data vendors that charge a subscription fee.  If the subscriber wishes to use one
                            of these optional downloaders they must purchase a plan directly from the data provider.
                    </p>
                </div>
            </div>
            <div>
                <iframe style={{ border: "1px solid #333333", overflow: "hidden" }}
                    src="//research.stlouisfed.org/fred-glance-widget.php" height="490px" width="320px"
                     frameBorder="0" scrolling="no">
                </iframe>
            </div>
        </div>
    )
}
export default Documentation;