import React, { useState } from 'react';
import { Mail, ArrowLeft } from 'lucide-react';
import { useAuth } from './AuthContext';

export default function ChangeEmail() {
  const { changeEmail } = useAuth();
  
  const [newEmail, setNewEmail] = useState("");
  const [currentPassword, setCurrentPassword] = useState("");
  const [emailMessage, setEmailMessage] = useState("");
  const [emailError, setEmailError] = useState("");

  const handleChangeEmailSubmit = async (e) => {
    e.preventDefault();
    setEmailMessage("");
    setEmailError("");
    try {
      await changeEmail(currentPassword, newEmail);
      setEmailMessage("Success! Verification email sent to new address.");
      setNewEmail("");
      setCurrentPassword("");
    } catch (err) {
      setEmailError(err.message || "Failed to change email.");
    }
  };

  return (
    <div style={{
      width: 'min(850px, calc(100vw - 40px))',
      margin: '2rem auto 0',
      paddingBottom: '2rem'
    }}>
      <div style={{
        background: '#fff9e9',
        border: '5px solid #171717',
        borderRadius: '22px',
        padding: '2.5rem',
        textAlign: 'left',
        boxShadow: '8px 8px 0 rgba(0, 0, 0, 0.35)'
      }}>
        
        <a 
          href="?tab=login"
          style={{
            display: 'inline-flex',
            alignItems: 'center',
            gap: '8px',
            color: '#171717',
            textDecoration: 'none',
            fontWeight: '900',
            marginBottom: '1.5rem',
            background: '#ffd84d',
            padding: '8px 16px',
            borderRadius: '10px',
            border: '3px solid #171717',
            boxShadow: '0 4px 0 #171717'
          }}
        >
          <ArrowLeft size={20} />
          Go Back
        </a>

        <div style={{
          background: '#f59b35',
          color: '#ffffff',
          border: '5px solid #171717',
          borderRadius: '14px',
          padding: '1rem',
          maxWidth: '500px',
          margin: '0 auto 2rem',
          textAlign: 'center',
          boxShadow: '0 7px 0 #9b531e'
        }}>
          <h2 style={{
            margin: 0,
            fontFamily: '\"Press Start 2P\", cursive',
            fontSize: 'clamp(0.85rem, 2.4vw, 1.2rem)',
            textShadow: '3px 3px 0 #171717',
            color: '#ffffff'
          }}>
            <Mail size={24} style={{ verticalAlign: 'middle', marginRight: '10px' }} />
            CHANGE EMAIL
          </h2>
        </div>

        <p style={{ fontSize: '1rem', color: '#1b2433', marginBottom: '1.5rem', fontWeight: 'bold' }}>
          For security, please enter your current password and your new email address.
        </p>
        
        {emailError && <div style={{ color: '#ef4444', marginBottom: '1rem', fontWeight: 'bold', fontSize: '1.1rem' }}>{emailError}</div>}
        {emailMessage && <div style={{ color: '#3fd66b', marginBottom: '1rem', fontWeight: 'bold', fontSize: '1.1rem' }}>{emailMessage}</div>}
        
        <form onSubmit={handleChangeEmailSubmit} style={{ display: 'flex', flexDirection: 'column', gap: '15px' }}>
          <input
            type="email"
            placeholder="New Email Address"
            value={newEmail}
            onChange={(e) => setNewEmail(e.target.value)}
            required
            style={{
              padding: '15px',
              borderRadius: '12px',
              border: '4px solid #171717',
              fontFamily: 'inherit',
              fontSize: '1rem',
              fontWeight: 'bold'
            }}
          />
          <input
            type="password"
            placeholder="Current Password"
            value={currentPassword}
            onChange={(e) => setCurrentPassword(e.target.value)}
            required
            style={{
              padding: '15px',
              borderRadius: '12px',
              border: '4px solid #171717',
              fontFamily: 'inherit',
              fontSize: '1rem',
              fontWeight: 'bold'
            }}
          />
          <button
            type="submit"
            style={{
              background: '#3fd66b',
              color: '#082313',
              border: '4px solid #171717',
              borderRadius: '12px',
              padding: '15px',
              fontWeight: '900',
              fontSize: '1.1rem',
              cursor: 'pointer',
              marginTop: '0.5rem',
              boxShadow: '0 6px 0 #137333'
            }}
          >
            Confirm Email Change
          </button>
        </form>
      </div>
    </div>
  );
}
