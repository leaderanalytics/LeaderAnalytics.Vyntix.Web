import React, { useEffect, useState, useRef } from 'react';
import { Image, Button } from 'react-bootstrap';
import { useLocation } from 'react-router';
import { useAsyncEffect } from 'use-async-effect';
import { FontAwesomeIcon } from '@fortawesome/react-fontawesome';
import { faMailBulk, faEnvelope } from '@fortawesome/free-solid-svg-icons';
import { SendContactRequest } from '../Services/Services';
import ContactRequest from '../Model/ContactRequest';
import AsyncResult from '../Model/AsyncResult';
import Dialog from './Dialog';
import DialogType from '../Model/DialogType';
import AppConfig from '../appconfig';
import DialogProps from '../Model/DialogProps';

function Contact() {
    // https://stackoverflow.com/questions/54895883/reset-to-initial-state-with-react-hooks
    const myForm = useRef(null);
    const inputRef = useRef(null);
    const CAPTCHA_URL = AppConfig.host + "email/captchaImage";
    const [dialogProps, setDialogProps] = useState(new DialogProps("", DialogType.None, () => { }));
    const [name, setName] = useState("");
    const [email, setEmail] = useState("");
    const [phone, setPhone] = useState("");
    const [requirement, setRequirement] = useState("Company Information");
    const [comment, setComment] = useState("");
    const [captcha, setCaptcha] = useState("");
    const [captchaImgUrl, setCaptchaUrl] = useState(CAPTCHA_URL + '?d=' + new Date().getTime().toString());
    const [reloadCount, setReloadCount] = useState(0);

    const clearForm = () => {
        (myForm?.current as any).reset();
        setDialogProps(new DialogProps("", DialogType.None, () => { }));
        setName("");
        setEmail("");
        setPhone("");
        setRequirement("");
        setComment("");
        setCaptcha("");
        setCaptchaUrl(CAPTCHA_URL + '?d=' + new Date().getTime().toString());
        setReloadCount(reloadCount + 1);
    }

    const handleSubmit = async (event: any) => {
        
        event.preventDefault();
        let msg = new ContactRequest();
        msg.Name = name;
        msg.EMail = email;
        msg.Phone = phone;
        msg.Requirement = requirement;
        msg.Message = comment;
        msg.Captcha = captcha;

        if (msg.Message.length < 1) {
            setDialogProps(new DialogProps("Please input a message.", DialogType.Error, () => { setDialogProps(new DialogProps("", DialogType.None, () => { })); }));
            return;
        }

        if (msg.Captcha.length < 3) {
            setDialogProps(new DialogProps("Please input the numbers shown in the image.", DialogType.Error, () => { setDialogProps(new DialogProps("", DialogType.None, () => { })); }));
            return;
        }
        setDialogProps(new DialogProps("Please wait while your message is sent.", DialogType.Wait, () => { setDialogProps(new DialogProps("", DialogType.None, () => { })); }));
        const result: AsyncResult = await SendContactRequest(msg);
        

        if (result.Success) {
            setDialogProps(new DialogProps("Your message was sent successfully.", DialogType.Info, () => { clearForm(); setDialogProps(new DialogProps("", DialogType.None, () => { })); }));
        }
        else {
            const errorMsg = await result.ErrorMessage;
            setDialogProps(new DialogProps(errorMsg, DialogType.Error, () => { setDialogProps(new DialogProps("", DialogType.None, () => { })); }));
        }
    }

    useEffect(() => {
        (inputRef?.current as any).focus();
    },[reloadCount])

    return (
        <div className="container-fluid content-root dark-bg ">
            <Dialog dialogProps={dialogProps} />
            <div id="banner">
                <div className="pageBanner rp1">
                    <span className="rh5">Contact Us</span>
                </div>
            </div>

            <div className="flex-row center-content">

                <div className="d-flex">
                    <form onSubmit={handleSubmit} ref={myForm} >
                        <div className="form-row">
                            <div id="form-intro" className="rp2 col-md-12 center-content">
                                <p className="rh6">Please use this form for company or subscription inquiries only.  Use the <a href="https://github.com/leaderanalytics" target="_blank">GitHub issue pages</a> for product related or technical questions.</p>
                            </div>
                        </div>
                        <div className="form-row rmt2">
                            <div className="form-group col-md-6">
                                <label >Your name</label>
                                <input type="text" className="form-control" name="name" onChange={e => setName(e.target.value)} ref={inputRef} />
                            </div>
                            <div className="form-group col-md-6">
                                <label>Email address</label>
                                <input type="email" className="form-control" name="email" onChange={e => setEmail(e.target.value)}/>
                            </div>
                        </div>
                        <div className="form-row">
                            <div className="form-group col-md-6">
                                <label>Phone number</label>
                                <input type="tel" className="form-control" name="phone" onChange={e => setPhone(e.target.value)}/>
                            </div>
                            <div className="form-group col-md-6">
                                <label>Requirement</label>
                                <select name="requirement" className="form-control" onChange={e => setRequirement(e.target.value)} >
                                    <option value="CompanyInfo">Company Information</option>
                                    <option value="SubscriptionIssue">Subscription Issue</option>
                                    <option value="Other">Other</option>
                                </select>
                            </div>

                            <div className="form-group col-md-12">
                                <label>Message</label>
                                <textarea className="form-control" name="comment" onChange={e => setComment(e.target.value)}></textarea>
                            </div>


                            <div className="form-group col-md-3 rmt3">
                                <div>
                                    <Image id="captchaImage" src={captchaImgUrl} />
                                </div>
                                <div >
                                    <label className="control-label">Enter the numbers shown above:&nbsp; </label>
                                    <input type="text" width="90" className="form-control" onChange={e => setCaptcha(e.target.value)}></input>
                                </div>
                            </div>


                            <div className="form-group col-md-6 rmt2 d-flex flex-column justify-content-end">
                                <Button type="submit" className="iconButton"  >
                                    <div className="rh6">
                                        <div>Send Message</div>
                                        <FontAwesomeIcon className="rh4" icon={faEnvelope} />
                                    </div>
                                </Button>
                            </div>
                        </div>
                    </form >
                </div>
            </div >
        </div>
    )
}
export default Contact;