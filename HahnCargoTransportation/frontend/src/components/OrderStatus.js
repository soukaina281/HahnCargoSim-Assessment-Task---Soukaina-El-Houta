import React from 'react';
import './CSS/OrderStatus.css';

const OrderStatus = ({ orders }) => {
    return (
        <div className="order-status">
            <h3>Orders</h3>
            <ul className="order-list">
                {orders.map(order => (
                    <li key={order.id} className="order-item">
                        <strong>ID:</strong> {order.id}, 
                        <strong> Origin:</strong> {order.originNodeId}, 
                        <strong> Target:</strong> {order.targetNodeId}, 
                        <strong> Load:</strong> {order.load}, 
                        <strong> Value:</strong> {order.value}, 
                        <strong> Delivery Date:</strong> {new Date(order.deliveryDateUtc).toLocaleString()}
                    </li>
                ))}
            </ul>
        </div>
    );
};

export default OrderStatus;
