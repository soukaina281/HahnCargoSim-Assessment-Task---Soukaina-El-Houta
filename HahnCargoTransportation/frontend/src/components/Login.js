import React, { useState } from 'react';
import ApiService from '../Services/ApiService';
import './CSS/Login.css';

const Login = ({ onLogin }) => {
    const [username, setUsername] = useState('');
    const [password, setPassword] = useState('');
    const [error, setError] = useState(null);

    const handleLogin = async () => {
        try {
            await ApiService.login(username, password);
            onLogin();
        } catch (error) {
            setError('Login failed. Please check your username and password.');
        }
    };

    return (
        <div className="login-container">
            <div className="login-box">
                <h2 className="title">Login</h2>
                {error && <div className="error-message">{error}</div>}
                <input 
                    type="text" 
                    className="input" 
                    placeholder="Username" 
                    value={username} 
                    onChange={(e) => setUsername(e.target.value)} 
                />
                <input 
                    type="password" 
                    className="input" 
                    placeholder="Password" 
                    value={password} 
                    onChange={(e) => setPassword(e.target.value)} 
                />
                <button className="button" onClick={handleLogin}>Login</button>
            </div>
        </div>
    );
};

export default Login;
