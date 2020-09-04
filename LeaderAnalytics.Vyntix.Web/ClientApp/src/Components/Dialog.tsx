import React, { useEffect, useState } from 'react';
import { Image, Button, Modal } from 'react-bootstrap';
import { FontAwesomeIcon } from '@fortawesome/react-fontawesome';
import { faInfoCircle } from '@fortawesome/free-solid-svg-icons';

function Dialog(props: any) {
   

    return (
        <>
            <Modal show={props.message.length > 1} onHide={props.callback} centered={true}>
                <div className="modal-container">
                    <Modal.Header closeButton className="modal-header">
                        <Modal.Title><FontAwesomeIcon icon={faInfoCircle} />Info</Modal.Title>
                </Modal.Header>
                <Modal.Body className="modal-body">{props.message}</Modal.Body>
                <Modal.Footer className="modal-footer">
                    <Button variant="primary" onClick={props.callback}>
                        Ok
                    </Button>
                </Modal.Footer>
                </div>
            </Modal>
        </>
    );
}
export default Dialog;