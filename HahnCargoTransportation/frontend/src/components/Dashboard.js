import React, { useState, useEffect } from 'react';
import ApiService from '../Services/ApiService';
import TransporterStatus from './TransporterStatus';
import OrderStatus from './OrderStatus';
import './CSS/Dashboard.css';

const Dashboard = () => {
    const [coinAmount, setCoinAmount] = useState(0);
    const [transporters, setTransporters] = useState([]);
    const [orders, setOrders] = useState([]);
    const [simulationRunning, setSimulationRunning] = useState(false);
    const [username, setUsername] = useState('');

    useEffect(() => {
        const fetchData = async () => {
            setCoinAmount(await ApiService.getCoinAmount());
            setTransporters(await ApiService.getTransporters());
            setOrders(await ApiService.getOrders());
        };

        fetchData();
        const interval = setInterval(fetchData, 10000);
        return () => clearInterval(interval);
    }, []);

    useEffect(() => {
        const storedUsername = localStorage.getItem('username');
        setUsername(storedUsername);
    }, []);

    const handleStart = async () => {
        await ApiService.startSimulation();
        setSimulationRunning(true);
    };

    const handleStop = async () => {
        await ApiService.stopSimulation();
        setSimulationRunning(false);
    };

    return (
        <div className="dashboard-container">
            <div className="dashboard-box">
                <h2 className="title">Dashboard</h2>
                <p className="greeting">Hello {username}</p>
                <p className="coin-amount">Coin Amount: {coinAmount}</p>
                <button 
                    className={`button ${simulationRunning ? 'button-stop' : ''}`} 
                    onClick={simulationRunning ? handleStop : handleStart}
                >
                    {simulationRunning ? 'Stop Simulation' : 'Start Simulation'}
                </button>
                <TransporterStatus transporters={transporters} />
                <OrderStatus orders={orders} />
            </div>
        </div>
    );
};

export default Dashboard;
