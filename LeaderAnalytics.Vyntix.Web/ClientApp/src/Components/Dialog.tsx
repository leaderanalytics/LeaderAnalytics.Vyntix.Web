import React, { useEffect, useState } from 'react';
import { Image, Button, Modal } from 'react-bootstrap';
import { FontAwesomeIcon } from '@fortawesome/react-fontawesome';
import { faInfoCircle, faHourglass, faExclamationTriangle } from '@fortawesome/free-solid-svg-icons';
import DialogType  from '../Model/DialogType';
import DialogProps from '../Model/DialogProps';

function Dialog(props: any) {

    const dialogProps = props.dialogProps as DialogProps;

    return (<>{
        
            dialogProps.DialogType === DialogType.None ?
                <></>
            : dialogProps.DialogType === DialogType.Info ?
                <Modal show={dialogProps.Message.length > 1} onHide={dialogProps.Callback} centered={true}>
                    <div className="modal-container">
                        <Modal.Header closeButton className="modal-header">
                            <Modal.Title><FontAwesomeIcon icon={faInfoCircle} />Info</Modal.Title>
                        </Modal.Header>
                        <Modal.Body className="modal-body">{dialogProps.Message}</Modal.Body>
                        <Modal.Footer className="modal-footer">
                            <Button variant="primary" onClick={dialogProps.Callback}>
                                Ok
                            </Button>
                        </Modal.Footer>
                    </div>
                </Modal>
        
            : dialogProps.DialogType === DialogType.Wait ?
                <Modal show={dialogProps.Message.length > 1} onHide={dialogProps.Callback} centered={true}>
                    <div className="modal-container">
                        <Modal.Header closeButton className="modal-header">
                            <Modal.Title><FontAwesomeIcon icon={faHourglass} />Please wait</Modal.Title>
                        </Modal.Header>
                        <Modal.Body className="modal-body">{dialogProps.Message}</Modal.Body>
                        <Modal.Footer className="modal-footer">
                        </Modal.Footer>
                    </div>
                    </Modal>

            : dialogProps.DialogType === DialogType.Error ?
                <Modal show={dialogProps.Message.length > 1} onHide={dialogProps.Callback} centered={true}>
                    <div className="modal-container">
                        <Modal.Header closeButton className="modal-header">
                            <Modal.Title><FontAwesomeIcon icon={faExclamationTriangle} style={{ color: "lightcoral" }} />Error</Modal.Title>
                        </Modal.Header>
                        <Modal.Body className="modal-body">{dialogProps.Message}</Modal.Body>
                        <Modal.Footer className="modal-footer">
                            <Button variant="primary" onClick={dialogProps.Callback}>
                                Ok
                            </Button>
                        </Modal.Footer>
                    </div>
                        </Modal>

            : <div>DialogType not found.</div>
        
        
        
        }</>)
}
export default Dialog;