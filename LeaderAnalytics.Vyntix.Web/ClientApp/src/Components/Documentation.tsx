import React,  { useContext } from 'react';


function Documentation() {

    return (
        <div className="container-fluid content-root">

            <div>
                This is Documentation
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