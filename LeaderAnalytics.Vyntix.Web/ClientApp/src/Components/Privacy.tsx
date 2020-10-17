import React, { useContext } from 'react';
import { NavLink, Link } from 'react-router-dom';
import { FontAwesomeIcon } from '@fortawesome/react-fontawesome';
import {  faTools } from '@fortawesome/free-solid-svg-icons';

function Privacy() {

    
    return (

        <div className="container-fluid content-root dark-bg">

            <div id="banner">
                <div className="pageBanner rp1">
                    <span className="rh5">Vyntix Privacy Policy</span>
                </div>
            </div>

            <div className=" rh6 rp1 rm5" >

                <p className="rh5">Date of last update: Sept 28, 2020</p>

                <p>This Privacy Policy governs the manner in which Leader Analytics collects, uses, maintains and discloses information collected from each user (“User” or "Users") of the vyntix.com website (“Site”).
                    This privacy policy applies to the Site and all products and services offered by Leader Analytics.</p>

                <p className="rh5">Personal identification information</p>
                We may collect personal identification information from Users in a variety of ways, including, but not limited to, when Users visit the Site, 
                sign in to the Site, subscribe to the newsletter, fill out a form, and in connection with other activities, services, 
                features or resources we make available on the Site. Users may be asked for, as appropriate, name, email address, mailing address, and phone number. We may also collect the User's IP Address
                at the time they vist any one of the pages on the Site.
                

                <p className="rh5">Non-personal identification information</p>
                    We may collect non-personal identification information about Users whenever they interact with our Site. Non-personal identification information may include the browser name, the type of computer and technical information about Users means of connection to our Site, such as the operating system, and the Internet service providers utilized and other similar information.

                <p className="rh5">Web browser cookies</p>
                        Our Site may use “cookies” to enhance User experience. User’s web browser places cookies on their hard drive for record-keeping purposes and sometimes to track information about them. User may choose to set their web browser to refuse cookies, or to alert you when cookies are being sent. If they do so, note that some parts of the Site may not function properly.

                <p className="rh5">How we use collected information</p>
                    Leader Analytics may collect and use Users personal information for the following purposes:
                <ul>
                    <li>
                        To improve customer service -
                        Information you provide helps us respond to your customer service requests and support needs more efficiently.
                    </li>
                    <li>
                        To enforce our subscription terms -
                        We may use information we collect to verify you are entitled to access services that are available only to subscribers.  
                        We may also use that information to verify you are using your subscription within the subscription terms.
                    </li>
                    <li>
                        To improve our Site -
                        We may use feedback you provide to improve our products and services.
                    </li>
                    <li>
                        To send periodic emails -
                        We may use the email address to send User information and updates pertaining to their subscription. 
                        It may also be used to respond to their inquiries, questions, and/or other requests. 
                        If User decides to opt-in to our mailing list, they will receive emails that may include company news, updates, related product or service information, etc.
                        These emails will be relatively infrequent and will always contain information that is genuinely of interest and value to the User.
                        We include detailed unsubscribe instructions at the bottom of each email should the User decide to opt-out at any time.
                        </li>
                </ul> 

                <p className="rh5">Information we do not collect</p>
                    We do not gather any information about the data elements, methodologies, algorithms, or code our Users use to build models.  Our software and services do not use or contain code that 
                    attempts to harvest this information. Our network and servers contain no facility for storing or analyzing information of this nature.
                Our software may log some data elements if it encounters an error.  This information may be necessary in assisting us in resolving and fixing technical issues.
                This information is requested and retained only for the purpose of resolving a specific technical issue.  Furthermore, during the course of resolving technical 
                issues, we may ask the User for specific details about data series they are using as well as other relevent information.  We will use this information only to assist the User in resolving their
                technical issue.  It is the responsiblity of the User to protect confidential information during the course of resolving technical issues. 

                <p className="rh5">How we protect your information</p>
                    We adopt appropriate data collection, storage and processing practices and security measures to protect against unauthorized access, alteration, disclosure or destruction of your personal information, username, password, transaction information and data stored on our Site.

                <p className="rh5">Sharing your personal information</p>
                                We do not sell, trade, or rent Users personal identification information to others. We may share generic aggregated demographic information not linked to any personal identification information regarding visitors and users with our business partners, trusted affiliates and advertisers for the purposes outlined above.We may use third party service providers to help us operate our business and the Site or administer activities on our behalf, such as sending out newsletters or surveys. We may share your information with these third parties for those limited purposes provided that you have given us your permission.

                <p className="rh5">Third party websites </p>
                Users may find advertising or other content on our Site that link to the sites and services of our partners, suppliers, advertisers, sponsors, licensors and other third parties. 
                We do not control the content or links that appear on these sites and are not responsible for the practices employed by websites linked to or from our Site. 
                In addition, these sites or services, including their content and links, may be constantly changing. 
                These sites and services may have their own privacy policies and customer service policies. 
                Browsing and interaction on any other website, including websites which have a link to our Site, is subject to that website’s own terms and policies.

                <p className="rh5">Third party data providers</p>
                Users may choose to purchase data from third parties to use with their Vyntix subscription.  Such purchases are discretionary and are undertaken at the sole risk of the User.   

                <p className="rh5">Changes to this privacy policy</p>
                                        Leader Analytics has the discretion to update this privacy policy at any time. When we do, we will revise the updated date at the bottom of this page. We encourage Users to frequently check this page for any changes to stay informed about how we are helping to protect the personal information we collect. You acknowledge and agree that it is your responsibility to review this privacy policy periodically and become aware of modifications.

                <p className="rh5">Your acceptance of these terms</p>
                                            By using this Site, you signify your acceptance of this policy. If you do not agree to this policy, please do not use our Site. Your continued use of the Site following the posting of changes to this policy will be deemed your acceptance of those changes.

                <p className="rh5">Contacting us</p>
                If you have any questions about this Privacy Policy, the practices of this site, or your dealings with this site, please contact us at: leaderanalytics@outlook.com.

                 
            </div>
            &nbsp;
        </div>
        

    )
}
export default Privacy;