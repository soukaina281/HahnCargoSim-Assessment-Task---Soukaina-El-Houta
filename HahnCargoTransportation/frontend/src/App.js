import React, { useState } from 'react';
import './App.css';
import Dashboard from './components/Dashboard';
import Login from './components/Login';

function App() {
    const [loggedIn, setLoggedIn] = useState(false);

    return (
        <div className="App">
            {loggedIn ? <Dashboard /> : <Login onLogin={() => setLoggedIn(true)} />}
        </div>
    );
}

export default App;
