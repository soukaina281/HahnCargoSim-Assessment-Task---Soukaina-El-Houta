import axios from 'axios';

const API_URL = 'https://localhost:7186/api'; // Replace with your backend URL

const login = async (username, password) => {
    const response = await axios.post(`${API_URL}/User/Login`, { username, password });

    localStorage.setItem('token', response.data.token);
    localStorage.setItem('username', response.data.userName);
    return response.data;
};

const startSimulation = async () => {
    const token = localStorage.getItem('token');
    await axios.post(`${API_URL}/Simulation/Start`, {}, {
        headers: { Authorization: `Bearer ${token}` }
    });
};

const stopSimulation = async () => {
    const token = localStorage.getItem('token');
    await axios.post(`${API_URL}/Simulation/Stop`, {}, {
        headers: { Authorization: `Bearer ${token}` }
    });
};

const getCoinAmount = async () => {
    const token = localStorage.getItem('token');
    const response = await axios.get(`${API_URL}/User/CoinAmount`, {
        headers: { Authorization: `Bearer ${token}` }
    });
    return response.data;
};

const getTransporters = async () => {
    const token = localStorage.getItem('token');
    const response = await axios.get(`${API_URL}/Transporter/GetAll`, {
        headers: { Authorization: `Bearer ${token}` }
    });
    return response.data;
};


const getOrders = async () => {
    const token = localStorage.getItem('token');
    const response = await axios.get(`${API_URL}/Order/Accepted`, {
        headers: { Authorization: `Bearer ${token}` }
    });
    return response.data;
};

const getGridData = async () => {
    const token = localStorage.getItem('token');
    const response = await axios.get(`${API_URL}/Grid/Get`, {
        headers: { Authorization: `Bearer ${token}` }
    });
    return response.data;
};

export default {
    login,
    startSimulation,
    stopSimulation,
    getCoinAmount,
    getTransporters,
    getOrders,
    getGridData
};
