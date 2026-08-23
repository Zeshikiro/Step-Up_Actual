import React from 'react';
import { Shield, Lock, Eye, CheckCircle } from 'lucide-react';

export default function PrivacyPolicy() {
  return (
    <div style={{
      width: 'min(850px, calc(100vw - 40px))',
      margin: '2rem auto 0',
      paddingBottom: '2rem'
    }}>
      <div style={{
        background: '#fff4d6',
        border: '5px solid #171717',
        borderRadius: '22px',
        padding: '2.5rem',
        textAlign: 'left',
        boxShadow: '8px 8px 0 rgba(0, 0, 0, 0.35)'
      }}>

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
            PRIVACY POLICY
          </h2>
        </div>

        <h3 style={{ color: '#9b531e', fontSize: '1.5rem', borderBottom: '3px dashed #171717', paddingBottom: '0.5rem', display: 'flex', alignItems: 'center', gap: '10px' }}>
          <Eye size={24} /> Data Collection
        </h3>
        <p style={{ fontWeight: 'bold', color: '#333', lineHeight: '1.6' }}>
          Step-Up collects your email address for account authentication and your location/step data exclusively to gamify your fitness journey. We use Mapbox to render the map, which requires location permissions.
        </p>

        <h3 style={{ color: '#9b531e', fontSize: '1.5rem', borderBottom: '3px dashed #171717', paddingBottom: '0.5rem', display: 'flex', alignItems: 'center', gap: '10px', marginTop: '2rem' }}>
          <Lock size={24} /> Data Security
        </h3>
        <p style={{ fontWeight: 'bold', color: '#333', lineHeight: '1.6' }}>
          Your fitness progress and coins are securely saved to our Google Firebase database. Your password is cryptographically hashed and never visible to anyone, including the developers.
        </p>

        <h3 style={{ color: '#9b531e', fontSize: '1.5rem', borderBottom: '3px dashed #171717', paddingBottom: '0.5rem', display: 'flex', alignItems: 'center', gap: '10px', marginTop: '2rem' }}>
          <Shield size={24} /> Social & Leaderboards
        </h3>
        <p style={{ fontWeight: 'bold', color: '#333', lineHeight: '1.6' }}>
          By playing Step-Up, your lifetime step count is publicly visible on the global leaderboard. However, your exact real-time GPS location is never broadcasted to other players.
        </p>
        
        <div style={{ marginTop: '3rem', textAlign: 'center' }}>
            <p style={{ fontWeight: '900', color: '#171717', fontSize: '1.1rem' }}>
                <CheckCircle size={20} style={{ verticalAlign: 'middle', marginRight: '5px', color: '#3fd66b' }}/> 
                Your fitness is your own. We never sell your data to third-party ad networks.
            </p>
        </div>

      </div>
    </div>
  );
}
