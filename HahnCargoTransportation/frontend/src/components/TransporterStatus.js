import React from 'react';
import './CSS/TransporterStatus.css';

const TransporterStatus = ({ transporters }) => {
    return (
        <div className="transporter-status">
            <h3>Transporters</h3>
            <ul className="transporter-list">
                {transporters.map(transporter => (
                    <li key={transporter.id} className="transporter-item">
                        <div className="transporter-info">
                            <strong>ID:</strong> {transporter.id}
                        </div>
                        <div className="transporter-info">
                            <strong>Position Node:</strong> {transporter.positionNodeId}
                        </div>
                        <div className="transporter-info">
                            <strong>Busy:</strong> {transporter.busy ? 'Yes' : 'No'}
                        </div>
                    </li>
                ))}
            </ul>
        </div>
    );
};

export default TransporterStatus;
