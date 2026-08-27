import { Download, Heart, Home, Info, MessageCircle, Trophy, User } from 'lucide-react'
import { useState, useEffect } from 'react'
import './App.css'
import AboutUs from './components/AboutUs'
import { AuthProvider, useAuth } from './components/AuthContext'
import HealthTips from './components/HealthTips'
import Leaderboard from './components/Leaderboard'
import Login from './components/Login'
import Profile from './components/Profile'
import SocialFeed from './components/SocialFeed'
import VerifyEmail from './components/VerifyEmail'
import PrivacyPolicy from './components/PrivacyPolicy'
import ChangeEmail from './components/ChangeEmail'

function AppContent() {
  const { currentUser } = useAuth();
  const getActiveTabFromUrl = () => {
    const params = new URLSearchParams(window.location.search);
    if (params.get('mode') === 'verifyEmail') {
      return 'verify';
    }
    return params.get('tab') || 'home';
  };

  const [activeTab, setActiveTab] = useState(getActiveTabFromUrl);

  useEffect(() => {
    const handlePopState = () => {
      setActiveTab(getActiveTabFromUrl());
    };
    window.addEventListener('popstate', handlePopState);
    return () => window.removeEventListener('popstate', handlePopState);
  }, []);

      const handleTabChange = (tab) => {
    setActiveTab(tab);
    const url = new URL(window.location);
    url.searchParams.set('tab', tab);
    
    // clear mode since we only use it for initial load of verification
    if (tab !== 'verify') {
      url.searchParams.delete('mode');
      url.searchParams.delete('oobCode');
      url.searchParams.delete('apiKey');
      url.searchParams.delete('lang');
    }
    
    if (tab !== 'health') {
      url.searchParams.delete('tip'); // clear tip if not on health page
    }
    window.history.pushState({}, '', url);
    // Dispatch a popstate event to let Login.jsx know the URL changed without full refresh
    window.dispatchEvent(new PopStateEvent('popstate'));
  };

  return (
      <div className="app-container" style={{paddingBottom: '80px'}}>
        
       <div key={activeTab} className="scene-transition">
        {activeTab === 'home' && (
  <>
    {/* TOP GAME HEADER */}
<div style={{
  display: 'flex',
  justifyContent: 'space-between',
  alignItems: 'center',
  width: 'min(950px, calc(100vw - 40px))',
  margin: '0 auto',
  paddingTop: '1rem',
  marginBottom: '-1rem'
}}>
      <div style={{
        display: 'flex',
        alignItems: 'center',
        gap: '10px',
        fontWeight: '900',
        color: '#ffd84d',
        textShadow: '3px 3px 0 #171717',
        fontSize: '1.8rem'
      }}>
        👟 STEP-UP
      </div>

      <div style={{
        display: 'flex',
        gap: '12px'
      }}>
        {currentUser ? (
          <button
            onClick={() => handleTabChange('login')}
            style={{
              background: '#2f8ed8',
              color: '#ffffff',
              border: '4px solid #171717',
              borderRadius: '10px',
              padding: '12px 18px',
              fontWeight: '900',
              cursor: 'pointer',
              boxShadow: '0 5px 0 #174b75',
              display: 'flex',
              alignItems: 'center',
              gap: '8px'
            }}
          >
            <User size={18} />
            My Profile
          </button>
        ) : (
          <>
            <button
              onClick={() => handleTabChange('register')}
              style={{
                background: '#2f8ed8',
                color: '#ffffff',
                border: '4px solid #171717',
                borderRadius: '10px',
                padding: '12px 18px',
                fontWeight: '900',
                cursor: 'pointer',
                boxShadow: '0 5px 0 #174b75'
              }}
            >
              Join Now
            </button>

            <button
              onClick={() => handleTabChange('login')}
              style={{
                background: '#3fd66b',
                color: '#082313',
                border: '4px solid #171717',
                borderRadius: '10px',
                padding: '12px 18px',
                fontWeight: '900',
                cursor: 'pointer',
                boxShadow: '0 5px 0 #137333'
              }}
            >
              Log In
            </button>
          </>
        )}
      </div>
    </div>

    <header>
      <h1 className="title-gradient">Step - Up</h1>
      <p className="subtitle">
        Gamify your fitness journey. Track every step, customize your 3D avatar, and conquer the leaderboard.
      </p>
      {!currentUser && (
        <button className="cta-button" onClick={() => handleTabChange('register')}>
          Join Now
        </button>
      )}
    </header>

    <section className="features-grid">
      <div className="glass-card">
        <h3>📍 Mapbox GPS Tracking</h3>
        <div style={{
          fontSize: '3rem',
          textAlign: 'center',
          margin: '1rem 0'
        }}>
          🗺️
        </div>
        <p>
          Explore your neighborhood with real-time positioning. Leave a trail behind you as you walk your way to fitness.
        </p>
      </div>

      <div className="glass-card">
        <h3>👟 Native Pedometer</h3>
        <div style={{
          fontSize: '3rem',
          textAlign: 'center',
          margin: '1rem 0'
        }}>
          👟
        </div>
        <p>
          Accurate step tracking that automatically converts your daily movement into calories burned and points earned.
        </p>
      </div>

      <div className="glass-card">
        <h3>🧍 3D Avatar Customizer</h3>
        <div style={{
          fontSize: '3rem',
          textAlign: 'center',
          margin: '1rem 0'
        }}>
          🧍
        </div>
        <p>
          Spend your points in the shop to unlock premium outfits. Express yourself with thousands of combinations.
        </p>
      </div>

      <div className="glass-card">
        <h3>🔔 Smart Reminders</h3>
        <div style={{
          fontSize: '3rem',
          textAlign: 'center',
          margin: '1rem 0'
        }}>
          🔔
        </div>
        <p>
          Customized Android background notifications to keep you on track, with friendly "we miss you" alerts if you forget to walk!
        </p>
      </div>

      <div className="glass-card">
        <h3>📶 Offline Resiliency</h3>
        <div style={{
          fontSize: '3rem',
          textAlign: 'center',
          margin: '1rem 0'
        }}>
          📶
        </div>
        <p>
          Automatically detects network drops and protects your GPS data, seamlessly loading a custom fallback interface until internet returns.
        </p>
      </div>
    </section>
  </>
)}

        {activeTab === 'verify' && <VerifyEmail />}
        {activeTab === 'leaderboard' && <Leaderboard />}
        {activeTab === 'social' && <SocialFeed />}
        {activeTab === 'about' && <AboutUs />}
        {activeTab === 'privacy' && <PrivacyPolicy />}
        {activeTab === 'health' && <HealthTips />}
        {activeTab === 'change-email' && (currentUser ? <ChangeEmail /> : <Login />)}
        {(activeTab === 'login' || activeTab === 'register') && <AuthWrapper />}
        </div>

        <footer style={{
          display: 'flex',
          flexDirection: 'column',
          alignItems: 'center',
          gap: '12px',
          marginTop: '1rem',
          marginBottom: '4rem',
          padding: '16px',
          position: 'relative',
          zIndex: 2,
          width: '100%',
          boxSizing: 'border-box'
        }}>
          {/* Quick Setup Guide Card */}
          <div style={{
            background: '#fff9e9',
            border: '4px solid #171717',
            borderRadius: '16px',
            padding: '1.5rem',
            maxWidth: '600px',
            width: '100%',
            boxSizing: 'border-box',
            boxShadow: '5px 5px 0 rgba(0,0,0,0.25)',
            textAlign: 'left',
            color: '#1b2433',
            marginBottom: '1rem'
          }}>
            <h3 style={{ marginTop: 0, marginBottom: '1rem', textAlign: 'center', fontSize: '1.4rem', color: '#174b75' }}>
              🛠️ Quick Setup Guide
            </h3>
            <p style={{ margin: '0 0 15px 0', fontWeight: 'bold', fontSize: '1.05rem', lineHeight: '1.4' }}>
              Here is the Google Drive link to download STEP-UP, plus a super quick guide on how to set up your account:
            </p>
            <ul style={{ listStyle: 'none', padding: 0, margin: '0 0 1rem 0', display: 'flex', flexDirection: 'column', gap: '12px', fontSize: '0.95rem', lineHeight: '1.4' }}>
              <li>📲 <b>1. Download & Install:</b> Click the link below to grab the app! (Quick tip: You might need to allow "Install from Unknown Sources" in your phone settings).</li>
              <li>🌐 <b>2. Register:</b> Open the app and tap Sign In. It will quickly redirect you to our website to create your account.</li>
              <li>📧 <b>3. Verify Your Email:</b> Hop over to your Gmail and click the verification link we sent you. (If you don't see it right away, be sure to peek in your Spam folder!)</li>
              <li>🏃‍♂️ <b>4. Log In & Set Up:</b> Once verified, jump back into the app, log in, and quickly set up your profile!</li>
            </ul>
            <div style={{ background: '#e0f2fe', padding: '12px', borderRadius: '8px', border: '2px solid #0284c7', fontSize: '0.9rem', lineHeight: '1.4' }}>
              💡 <b>P.S.</b> After you log in, you will notice a STEP-UP notification pop up on your phone. Just leave it there! That little widget is what allows the app to keep tracking your steps in the background, even when you aren't actively using the app. Have fun, and just message us here if you encounter any problem or need help!
            </div>
          </div>

          <p style={{
            margin: 0,
            textAlign: 'center',
            color: '#1b2433',
            fontWeight: '700'
          }}>
            An Academic Thesis Project. Available now on Android.
          </p>
        
          <a
            href="https://drive.google.com/uc?export=download&id=1zrFKgHrgTtZv9VFMKNUkHliReI67aCPX"
            target="_blank"
            rel="noopener noreferrer"
            style={{
              display: 'inline-flex',
              alignItems: 'center',
              justifyContent: 'center',
              gap: '10px',
              background: 'linear-gradient(135deg, #4ade80, #22c55e)',
              color: '#000',
              padding: '14px 26px',
              borderRadius: '12px',
              border: '4px solid #171717',
              fontWeight: '900',
              textDecoration: 'none',
              boxShadow: '0 6px 0 #14532d',
              transition: 'transform 0.1s ease-in-out',
              textAlign: 'center',
              maxWidth: '90vw'
            }}
            onMouseOver={(e) => e.currentTarget.style.transform = 'scale(1.05)'}
            onMouseOut={(e) => e.currentTarget.style.transform = 'scale(1)'}
          >
            <Download size={20} style={{ flexShrink: 0 }} />
            <span>Download from Google Drive</span>
          </a>
        </footer>

       {/* BOTTOM NAVIGATION BAR */}
<nav style={{
  position: 'fixed',
  bottom: 0,
  left: 0,
  right: 0,
  background: '#17365c',
  borderTop: '5px solid #171717',
  boxShadow: '0 -6px 0 rgba(0, 0, 0, 0.35)',
  display: 'flex',
  justifyContent: 'space-around',
  alignItems: 'center',
  padding: '10px 0 12px',
  zIndex: 1000
}}>
  <button 
    onClick={() => handleTabChange('home')} 
    style={{
      background: activeTab === 'home' ? '#ffd84d' : 'transparent',
      border: activeTab === 'home' ? '3px solid #171717' : '3px solid transparent',
      color: activeTab === 'home' ? '#171717' : '#ffffff',
      cursor: 'pointer',
      display: 'flex',
      flexDirection: 'column',
      alignItems: 'center',
      gap: '4px',
      borderRadius: '12px',
      padding: '6px 12px',
      minWidth: '72px',
      fontWeight: 'bold'
    }}>
    <Home size={24} />
    <span style={{fontSize: '0.75rem'}}>Home</span>
  </button>

  <button 
    onClick={() => handleTabChange('leaderboard')} 
    style={{
      background: activeTab === 'leaderboard' ? '#ffd84d' : 'transparent',
      border: activeTab === 'leaderboard' ? '3px solid #171717' : '3px solid transparent',
      color: activeTab === 'leaderboard' ? '#171717' : '#ffffff',
      cursor: 'pointer',
      display: 'flex',
      flexDirection: 'column',
      alignItems: 'center',
      gap: '4px',
      borderRadius: '12px',
      padding: '6px 12px',
      minWidth: '72px',
      fontWeight: 'bold'
    }}>
    <Trophy size={24} />
    <span style={{fontSize: '0.75rem'}}>Ranks</span>
  </button>

  <button 
    onClick={() => handleTabChange('social')} 
    style={{
      background: activeTab === 'social' ? '#ffd84d' : 'transparent',
      border: activeTab === 'social' ? '3px solid #171717' : '3px solid transparent',
      color: activeTab === 'social' ? '#171717' : '#ffffff',
      cursor: 'pointer',
      display: 'flex',
      flexDirection: 'column',
      alignItems: 'center',
      gap: '4px',
      borderRadius: '12px',
      padding: '6px 12px',
      minWidth: '72px',
      fontWeight: 'bold'
    }}>
    <MessageCircle size={24} />
    <span style={{fontSize: '0.75rem'}}>Feed</span>
  </button>

  <button 
    onClick={() => handleTabChange('health')} 
    style={{
      background: activeTab === 'health' ? '#ffd84d' : 'transparent',
      border: activeTab === 'health' ? '3px solid #171717' : '3px solid transparent',
      color: activeTab === 'health' ? '#171717' : '#ffffff',
      cursor: 'pointer',
      display: 'flex',
      flexDirection: 'column',
      alignItems: 'center',
      gap: '4px',
      borderRadius: '12px',
      padding: '6px 12px',
      minWidth: '72px',
      fontWeight: 'bold'
    }}>
    <Heart size={24} />
    <span style={{fontSize: '0.75rem'}}>Tips</span>
  </button>

  <button 
    onClick={() => handleTabChange('about')} 
    style={{
      background: activeTab === 'about' ? '#ffd84d' : 'transparent',
      border: activeTab === 'about' ? '3px solid #171717' : '3px solid transparent',
      color: activeTab === 'about' ? '#171717' : '#ffffff',
      cursor: 'pointer',
      display: 'flex',
      flexDirection: 'column',
      alignItems: 'center',
      gap: '4px',
      borderRadius: '12px',
      padding: '6px 12px',
      minWidth: '72px',
      fontWeight: 'bold'
    }}>
    <Info size={24} />
    <span style={{fontSize: '0.75rem'}}>About</span>
  </button>

  <button 
    onClick={() => handleTabChange('login')} 
    style={{
      background: (activeTab === 'login' || activeTab === 'register') ? '#ffd84d' : 'transparent',
      border: (activeTab === 'login' || activeTab === 'register') ? '3px solid #171717' : '3px solid transparent',
      color: (activeTab === 'login' || activeTab === 'register') ? '#171717' : '#ffffff',
      cursor: 'pointer',
      display: 'flex',
      flexDirection: 'column',
      alignItems: 'center',
      gap: '4px',
      borderRadius: '12px',
      padding: '6px 12px',
      minWidth: '72px',
      fontWeight: 'bold'
    }}>
    <User size={24} />
    <span style={{fontSize: '0.75rem'}}>Profile</span>
  </button>
</nav>

      </div>
  )
}

function App() {
  return (
    <AuthProvider>
      <AppContent />
    </AuthProvider>
  );
}

// A simple wrapper to decide whether to show the Login screen or the Profile Dashboard
function AuthWrapper() {
  const { currentUser } = useAuth();
  // Prevent flashing the Profile page during the brief moment after registration 
  // before the system forcefully signs the unverified user out.
  return (currentUser && currentUser.emailVerified) ? <Profile /> : <Login />;
}

export default App





