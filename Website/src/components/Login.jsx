import React, { useState } from 'react';
import { useAuth } from './AuthContext';
import { LogIn, UserPlus, LogOut, Eye, EyeOff } from 'lucide-react';

export default function Login() {
  const { login, register, currentUser, logout, resetPassword } = useAuth();
  const [isRegistering, setIsRegistering] = useState(false);
  const [email, setEmail] = useState("");
  const [password, setPassword] = useState("");
  const [error, setError] = useState("");
  const [loading, setLoading] = useState(false);
  const [showPassword, setShowPassword] = useState(false);
  const [resetSent, setResetSent] = useState(false);

  const handleSubmit = async (e) => {
    e.preventDefault();
    try {
      setError("");
      setLoading(true);
      if (isRegistering) {
        await register(email, password);
      } else {
        await login(email, password);
      }
    } catch (err) {
      console.error(err);
      setError(isRegistering ? "Registration failed: " + err.message : "Failed to sign in: " + err.message);
    }
    setLoading(false);
  };

  const handleForgotPassword = async () => {
    if (!email) {
      setError("Please enter your email first.");
      return;
    }
    try {
      await resetPassword(email);
      setResetSent(true);
      setError("");
    } catch (err) {
      setError("Failed to send reset email: " + err.message);
    }
  };

  if (currentUser) {
    return (
      <div className="glass-card" style={{marginTop: '2rem', textAlign: 'center'}}>
        <h2>Welcome, {currentUser.email}!</h2>
        <p style={{marginTop: '1rem', color: '#ccc'}}>You are successfully connected to Step-Up.</p>
        <button className="cta-button" onClick={logout} style={{marginTop: '2rem', display: 'inline-flex', alignItems: 'center', gap: '8px'}}>
          <LogOut size={18}/> Log Out
        </button>
      </div>
    );
  }

  return (
    <div className="glass-card" style={{marginTop: '2rem', maxWidth: '400px', margin: '2rem auto'}}>
      <h2 style={{textAlign: 'center', display: 'flex', alignItems: 'center', justifyContent: 'center', gap: '10px'}}>
        {isRegistering ? <><UserPlus /> Create Account</> : <><LogIn /> Sign In</>}
      </h2>
      
      {error && <p style={{color: '#ff6b6b', textAlign: 'center', marginTop: '1rem'}}>{error}</p>}
      
      <form onSubmit={handleSubmit} style={{display: 'flex', flexDirection: 'column', gap: '15px', marginTop: '1.5rem'}}>
        <input 
          type="email" 
          placeholder="Email" 
          value={email}
          onChange={(e) => setEmail(e.target.value)}
          required 
          style={{padding: '12px', borderRadius: '8px', border: 'none', background: 'rgba(255,255,255,0.1)', color: 'white'}}
        />
        <div style={{position: 'relative'}}>
          <input 
            type={showPassword ? 'text' : 'password'}
            placeholder="Password" 
            value={password}
            onChange={(e) => setPassword(e.target.value)}
            required 
            style={{padding: '12px', paddingRight: '40px', borderRadius: '8px', border: 'none', background: 'rgba(255,255,255,0.1)', color: 'white', width: '100%', boxSizing: 'border-box'}}
          />
          <span
            onClick={() => setShowPassword(!showPassword)}
            style={{position: 'absolute', right: '12px', top: '50%', transform: 'translateY(-50%)', cursor: 'pointer', color: '#aaa'}}
          >
            {showPassword ? <EyeOff size={18}/> : <Eye size={18}/>}
          </span>
        </div>

        {!isRegistering && (
          <p
            onClick={handleForgotPassword}
            style={{textAlign: 'right', fontSize: '0.8rem', color: '#6be2ff', cursor: 'pointer', marginTop: '-8px'}}
          >
            {resetSent ? "✅ Reset email sent!" : "Forgot Password?"}
          </p>
        )}

        <button disabled={loading} className="cta-button" type="submit" style={{marginTop: '10px'}}>
          {isRegistering ? "Register" : "Log In"}
        </button>
      </form>

      <p style={{textAlign: 'center', marginTop: '1.5rem', fontSize: '0.9rem', cursor: 'pointer', color: '#6be2ff'}} onClick={() => setIsRegistering(!isRegistering)}>
        {isRegistering ? "Already have an account? Sign In" : "Need an account? Register here"}
      </p>
    </div>
  );
}