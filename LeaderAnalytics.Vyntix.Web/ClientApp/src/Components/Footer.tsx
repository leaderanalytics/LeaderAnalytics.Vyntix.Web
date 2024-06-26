﻿import React, { useContext } from 'react';
import { Image, Nav, NavLink } from 'react-bootstrap';
import { FontAwesomeIcon } from '@fortawesome/react-fontawesome';
import {  faBook, faDownload, faEnvelope, faUsers, faUserShield, faFileContract } from '@fortawesome/free-solid-svg-icons'
import nuget  from '../Assets/nuget.png';
import { Link } from 'react-router-dom';
function Documentation() {

    return (
        <div>
            <hr className="separator" />
            <div id="footer-content" className="padding20 dark-bg">

                <div className="footer-row">

                    <div className="footer-cell">
                            <a href="https://github.com/leaderanalytics" target="_blank">
                                <svg height="32" viewBox="0 0 16 16" version="1.1" width="32" aria-hidden="true"><path fillRule="evenodd" d="M8 0C3.58 0 0 3.58 0 8c0 3.54 2.29 6.53 5.47 7.59.4.07.55-.17.55-.38 0-.19-.01-.82-.01-1.49-2.01.37-2.53-.49-2.69-.94-.09-.23-.48-.94-.82-1.13-.28-.15-.68-.52-.01-.53.63-.01 1.08.58 1.23.82.72 1.21 1.87.87 2.33.66.07-.52.28-.87.51-1.07-1.78-.2-3.64-.89-3.64-3.95 0-.87.31-1.59.82-2.15-.08-.2-.36-1.02.08-2.12 0 0 .67-.21 2.2.82.64-.18 1.32-.27 2-.27.68 0 1.36.09 2 .27 1.53-1.04 2.2-.82 2.2-.82.44 1.1.16 1.92.08 2.12.51.56.82 1.27.82 2.15 0 3.07-1.87 3.75-3.65 3.95.29.25.54.73.54 1.48 0 1.07-.01 1.93-.01 2.2 0 .21.15.46.55.38A8.013 8.013 0 0016 8c0-4.42-3.58-8-8-8z"></path></svg>
                            <span>&nbsp;GitHub</span>
                        </a>

                        <a className="mt-3" href="https://www.nuget.org/profiles/LeaderAnalytics" target="_blank">
                            <Image src={nuget} />
                        </a>
                    </div>

                    <div className="footer-cell">
                        <Nav.Link href="https://github.com/leaderanalytics" target="_blank" ><FontAwesomeIcon icon={faUsers} /><span>Community</span></Nav.Link>
                        <Nav.Link as={Link} to="/Documentation" href="/Documentation"><FontAwesomeIcon icon={faBook} /><span>Documentation</span></Nav.Link>
                    </div>

                    <div className="footer-cell">
                        <Nav.Link as={Link} to="/Downloads" href="/Downloads" ><FontAwesomeIcon icon={faDownload} /><span>Downloads</span></Nav.Link>
                        <Nav.Link as={Link} to="/Contact" href="/Contact" ><FontAwesomeIcon icon={faEnvelope} /><span>Contact Us</span></Nav.Link>
                    </div >


                    <div className="footer-cell">
                        <Nav.Link as={Link} to="/Privacy" href="/Privacy" ><FontAwesomeIcon icon={faUserShield} /><span>Privacy</span></Nav.Link>
                        <Nav.Link as={Link} to="/Terms" href="/Terms" ><FontAwesomeIcon icon={faFileContract} /><span>Terms</span></Nav.Link>
                    </div >

                    <div className="footer-cell">
                        <div className="grow-align-right">
                            <a href="https://www.LeaderAnalytics.com" target="_blank"><span className="logo rh6">Leader Analytics</span></a>
                            <p className="txt">Medford, OR</p>
                        </div>
                    </div>
                 </div>

                <div className="footer-row">
                    <div className="footer-cell">
                        <span className="txt">&copy; 2023 Leader Analytics</span>
                    </div>
                </div>
            </div>
        </div>

    )
}
export default Documentation;