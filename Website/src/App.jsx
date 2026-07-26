import { Download, Heart, Home, Info, MessageCircle, Trophy, User } from 'lucide-react'
import { useState } from 'react'
import './App.css'
import AboutUs from './components/AboutUs'
import { AuthProvider, useAuth } from './components/AuthContext'
import HealthTips from './components/HealthTips'
import Leaderboard from './components/Leaderboard'
import Login from './components/Login'
import Profile from './components/Profile'
import SocialFeed from './components/SocialFeed'

function App() {
  const [activeTab, setActiveTab] = useState(() => {
    const params = new URLSearchParams(window.location.search);
    return params.get('tab') || 'home';
  });

  const handleTabChange = (tab) => {
    setActiveTab(tab);
    const url = new URL(window.location);
    url.searchParams.set('tab', tab);
    if (tab !== 'health') {
      url.searchParams.delete('tip'); // clear tip if not on health page
    }
    window.history.pushState({}, '', url);
  };

  return (
    <AuthProvider>
      <div className="app-container" style={{paddingBottom: '80px'}}>
        
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
        <button
          onClick={() => handleTabChange('auth')}
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
          Sign In
        </button>

        <button
          onClick={() => handleTabChange('auth')}
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
          Join Now
        </button>
      </div>
    </div>

    <header>
      <h1 className="title-gradient">Step - Up</h1>
      <p className="subtitle">
        Gamify your fitness journey. Track every step, customize your 3D avatar, and conquer the leaderboard.
      </p>
      <button className="cta-button" onClick={() => handleTabChange('auth')}>
        Join Now
      </button>
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

        {activeTab === 'leaderboard' && <Leaderboard />}
        {activeTab === 'social' && <SocialFeed />}
        {activeTab === 'about' && <AboutUs />}
        {activeTab === 'health' && <HealthTips />}
        {activeTab === 'auth' && <AuthWrapper />}

        <footer style={{
  display: 'flex',
  flexDirection: 'column',
  alignItems: 'center',
  gap: '12px',
  marginTop: '1rem',
  marginBottom: '4rem',
  padding: '16px',
  position: 'relative',
  zIndex: 2
}}>
  <p style={{
    margin: 0,
    textAlign: 'center',
    color: '#1b2433',
    fontWeight: '700'
  }}>
    A Capstone Thesis Project. Currently available on Android.
  </p>

  <a
    href="#"
    style={{
      display: 'inline-flex',
      alignItems: 'center',
      gap: '10px',
      background: 'linear-gradient(135deg, #4ade80, #22c55e)',
      color: '#000',
      padding: '14px 26px',
      borderRadius: '12px',
      border: '4px solid #171717',
      textDecoration: 'none',
      fontWeight: 'bold',
      fontSize: '1rem',
      boxShadow: '0 6px 0 #137333',
      transition: 'transform 0.2s ease'
    }}
    onMouseOver={(e) => e.currentTarget.style.transform = 'scale(1.05)'}
    onMouseOut={(e) => e.currentTarget.style.transform = 'scale(1)'}
  >
    <Download size={20} />
    Download on Google Play
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
    onClick={() => handleTabChange('auth')} 
    style={{
      background: activeTab === 'auth' ? '#ffd84d' : 'transparent',
      border: activeTab === 'auth' ? '3px solid #171717' : '3px solid transparent',
      color: activeTab === 'auth' ? '#171717' : '#ffffff',
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
    </AuthProvider>
  )
}

// A simple wrapper to decide whether to show the Login screen or the Profile Dashboard
function AuthWrapper() {
  const { currentUser } = useAuth();
  return currentUser ? <Profile /> : <Login />;
}

export default App
