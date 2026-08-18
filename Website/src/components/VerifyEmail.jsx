import { useEffect, useState } from 'react';
import { auth } from '../firebaseConfig';
import { applyActionCode } from 'firebase/auth';
import { CheckCircle, XCircle } from 'lucide-react';

export default function VerifyEmail() {
  const [status, setStatus] = useState('verifying'); // 'verifying', 'success', 'error'
  const [errorMessage, setErrorMessage] = useState('');

  useEffect(() => {
    const params = new URLSearchParams(window.location.search);
    const mode = params.get('mode');
    const actionCode = params.get('oobCode');

    if (mode === 'verifyEmail' && actionCode) {
      applyActionCode(auth, actionCode)
        .then(() => {
          setStatus('success');
        })
        .catch((error) => {
          console.error(error);
          setStatus('error');
          if (error.code === 'auth/invalid-action-code') {
            setErrorMessage('This verification link is invalid or has already been used.');
          } else {
            setErrorMessage(error.message);
          }
        });
    } else {
      setStatus('error');
      setErrorMessage('Invalid verification link.');
    }
  }, []);

  const handleReturnToLogin = () => {
    const url = new URL(window.location);
    url.searchParams.delete('mode');
    url.searchParams.delete('oobCode');
    url.searchParams.delete('apiKey');
    url.searchParams.delete('lang');
    url.searchParams.set('tab', 'login');
    window.history.pushState({}, '', url);
    window.dispatchEvent(new PopStateEvent('popstate'));
  };

  return (
    <div style={{
      width: 'min(500px, calc(100vw - 40px))',
      margin: '4rem auto 2rem',
      paddingBottom: '2rem'
    }}>
      <div style={{
        background: '#fff4d6',
        border: '5px solid #171717',
        borderRadius: '22px',
        padding: '2.5rem',
        textAlign: 'center',
        boxShadow: '8px 8px 0 rgba(0, 0, 0, 0.35)'
      }}>

        <div style={{
          background: status === 'success' ? '#3fd66b' : status === 'error' ? '#ef4444' : '#f59b35',
          color: status === 'error' ? '#ffffff' : '#082313',
          border: '5px solid #171717',
          borderRadius: '14px',
          padding: '1.5rem',
          margin: '0 auto 1.5rem',
          textAlign: 'center',
          boxShadow: '0 7px 0 #171717',
          display: 'flex',
          flexDirection: 'column',
          alignItems: 'center',
          gap: '15px'
        }}>
          {status === 'verifying' && <h2 style={{ margin: 0, fontFamily: '"Press Start 2P", cursive', fontSize: '1rem', textShadow: '2px 2px 0px #ffffff' }}>Verifying...</h2>}
          {status === 'success' && (
            <>
              <CheckCircle size={48} color="#082313" />
              <h2 style={{ margin: 0, fontFamily: '"Press Start 2P", cursive', fontSize: '1rem', lineHeight: '1.5', textShadow: '2px 2px 0px #ffffff' }}>EMAIL VERIFIED!</h2>
            </>
          )}
          {status === 'error' && (
            <>
              <XCircle size={48} color="#ffffff" />
              <h2 style={{ margin: 0, fontFamily: '"Press Start 2P", cursive', fontSize: '1rem', lineHeight: '1.5', textShadow: '3px 3px 0px #171717' }}>VERIFICATION FAILED</h2>
            </>
          )}
        </div>

        {status === 'verifying' && (
          <p style={{ color: '#1b2433', fontWeight: '800', fontSize: '1.1rem' }}>
            Please wait while we verify your email address...
          </p>
        )}

        {status === 'success' && (
          <p style={{ color: '#1b2433', fontWeight: '800', fontSize: '1.1rem' }}>
            Your email has been successfully verified! You can now access your STEP-UP account.
          </p>
        )}

        {status === 'error' && (
          <p style={{ color: '#b91c1c', fontWeight: '800', fontSize: '1.1rem', background: '#fee2e2', padding: '10px', borderRadius: '10px', border: '3px solid #171717' }}>
            {errorMessage}
          </p>
        )}

        {(status === 'success' || status === 'error') && (
          <button
            onClick={handleReturnToLogin}
            style={{
              marginTop: '1.5rem',
              background: '#2f8ed8',
              color: '#ffffff',
              border: '4px solid #171717',
              borderRadius: '12px',
              padding: '14px 24px',
              cursor: 'pointer',
              fontWeight: '900',
              fontSize: '1.1rem',
              boxShadow: '0 6px 0 #174b75',
              width: '100%'
            }}
          >
            RETURN TO LOGIN
          </button>
        )}

      </div>
    </div>
  );
}
