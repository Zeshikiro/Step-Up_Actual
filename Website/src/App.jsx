import React, { useState } from 'react'
import './App.css'
import { AuthProvider } from './components/AuthContext'
import Login from './components/Login'
import Leaderboard from './components/Leaderboard'
import SocialFeed from './components/SocialFeed'
import AboutUs from './components/AboutUs'
import Profile from './components/Profile'
import { useAuth } from './components/AuthContext'
import { Home, Trophy, MessageCircle, User, Info } from 'lucide-react'

function App() {
  const [activeTab, setActiveTab] = useState('home');

  return (
    <AuthProvider>
      <div className="app-container" style={{paddingBottom: '80px'}}>
        
        {/* TAB CONTENT */}
        {activeTab === 'home' && (
          <>
            <header>
              <h1 className="title-gradient">Step - Up</h1>
              <p className="subtitle">Gamify your fitness journey. Track every step, customize your 3D avatar, and conquer the leaderboard.</p>
              <button className="cta-button" onClick={() => setActiveTab('auth')}>Join Now</button>
            </header>

            <section className="features-grid">
              <div className="glass-card">
                <h3>📍 Mapbox GPS Tracking</h3>
                <p>Explore your neighborhood with real-time positioning. Leave a trail behind you as you walk your way to fitness.</p>
              </div>
              <div className="glass-card">
                <h3>👟 Native Pedometer</h3>
                <p>Accurate step tracking that automatically converts your daily movement into calories burned and points earned.</p>
              </div>
              <div className="glass-card">
                <h3>👗 3D Avatar Customizer</h3>
                <p>Spend your points in the shop to unlock premium outfits. Express yourself with thousands of combinations.</p>
              </div>
            </section>
          </>
        )}

        {activeTab === 'leaderboard' && <Leaderboard />}
        {activeTab === 'social' && <SocialFeed />}
        {activeTab === 'about' && <AboutUs />}
        {activeTab === 'auth' && <AuthWrapper />}

        <footer>
          <p style={{marginTop: '2rem'}}>A Capstone Thesis Project. Currently available on Android.</p>
        </footer>

        {/* BOTTOM NAVIGATION BAR */}
        <nav style={{
          position: 'fixed',
          bottom: 0,
          left: 0,
          right: 0,
          background: 'rgba(10, 10, 20, 0.9)',
          backdropFilter: 'blur(10px)',
          borderTop: '1px solid rgba(255,255,255,0.1)',
          display: 'flex',
          justifyContent: 'space-around',
          padding: '15px 0',
          zIndex: 1000
        }}>
          <button 
            onClick={() => setActiveTab('home')} 
            style={{background: 'none', border: 'none', color: activeTab === 'home' ? '#6be2ff' : '#ccc', cursor: 'pointer', display: 'flex', flexDirection: 'column', alignItems: 'center', gap: '5px'}}>
            <Home size={24} />
            <span style={{fontSize: '0.8rem'}}>Home</span>
          </button>
          <button 
            onClick={() => setActiveTab('leaderboard')} 
            style={{background: 'none', border: 'none', color: activeTab === 'leaderboard' ? '#6be2ff' : '#ccc', cursor: 'pointer', display: 'flex', flexDirection: 'column', alignItems: 'center', gap: '5px'}}>
            <Trophy size={24} />
            <span style={{fontSize: '0.8rem'}}>Ranks</span>
          </button>
          <button 
            onClick={() => setActiveTab('social')} 
            style={{background: 'none', border: 'none', color: activeTab === 'social' ? '#6be2ff' : '#ccc', cursor: 'pointer', display: 'flex', flexDirection: 'column', alignItems: 'center', gap: '5px'}}>
            <MessageCircle size={24} />
            <span style={{fontSize: '0.8rem'}}>Feed</span>
          </button>
          <button 
            onClick={() => setActiveTab('about')} 
            style={{background: 'none', border: 'none', color: activeTab === 'about' ? '#6be2ff' : '#ccc', cursor: 'pointer', display: 'flex', flexDirection: 'column', alignItems: 'center', gap: '5px'}}>
            <Info size={24} />
            <span style={{fontSize: '0.8rem'}}>About</span>
          </button>
          <button 
            onClick={() => setActiveTab('auth')} 
            style={{background: 'none', border: 'none', color: activeTab === 'auth' ? '#6be2ff' : '#ccc', cursor: 'pointer', display: 'flex', flexDirection: 'column', alignItems: 'center', gap: '5px'}}>
            <User size={24} />
            <span style={{fontSize: '0.8rem'}}>Profile</span>
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
