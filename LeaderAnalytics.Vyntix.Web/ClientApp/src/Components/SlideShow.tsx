import React, { useEffect, useState } from 'react';
import { Image, Button, Modal } from 'react-bootstrap';
import { FontAwesomeIcon } from '@fortawesome/react-fontawesome';
import { faInfoCircle, faHourglass, faExclamationTriangle } from '@fortawesome/free-solid-svg-icons';
import SlideShowProps from '../Model/SlideShowProps';
import DialogType from '../Model/DialogType';
import { Slide } from 'react-slideshow-image';
import 'react-slideshow-image/dist/styles.css';

function SlideShow(props: any) {

    const slideShowProps = props.slideShowProps as SlideShowProps;

    return (<>{
        slideShowProps.DialogType === DialogType.None ?
            <></>
        : slideShowProps.DialogType === DialogType.Info ?
            <Modal show={true} onHide={slideShowProps.Callback} centered={true} dialogClassName="slideshow-container"  >
                <Modal.Header closeButton className="modal-header" onClick={slideShowProps.Callback}>
                </Modal.Header>
                <Modal.Body className="modal-body">
                    <Slide>
                        {slideShowProps.Images.map((x, i) =>
                            <div className="each-slide-effect" key={i}>
                                <div style={{ 'backgroundImage': `url(${slideShowProps.Images[i]})`}}>
                            </div>
                        </div>
                        )}
                    </Slide>
                </Modal.Body>
            </Modal>
        : <div>DialogType not found.</div>

    }</>)
}
export default SlideShow;