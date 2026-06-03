import React from 'react';
import { Info, Users, User } from 'lucide-react';

export default function AboutUs() {
  return (
    <div className="features-grid" style={{marginTop: '2rem', paddingBottom: '2rem'}}>
      
      {/* Project Background Section */}
      <div className="glass-card" style={{gridColumn: '1 / -1'}}>
        <h2 style={{display: 'flex', alignItems: 'center', gap: '10px', color: '#6be2ff'}}>
          <Info size={28} /> About Step - Up
        </h2>
        <p style={{marginTop: '1rem', lineHeight: '1.6', color: '#e0e0e0'}}>
          <strong>Step - Up</strong> is a Capstone Thesis Project designed to revolutionize the way we approach personal fitness. 
          By combining native pedometer tracking with Mapbox GPS integration, we have gamified the walking experience. 
          Every step you take in the real world translates directly into your virtual progress, allowing you to unlock 
          custom 3D avatar outfits, climb the global leaderboards, and engage with a community of fellow walkers.
        </p>
        <p style={{marginTop: '1rem', lineHeight: '1.6', color: '#e0e0e0'}}>
          Our mission is to make health and fitness engaging, rewarding, and accessible to everyone by blending 
          augmented reality concepts with healthy daily habits.
        </p>
      </div>

      {/* Team Section */}
      <div className="glass-card" style={{gridColumn: '1 / -1', marginTop: '1rem'}}>
        <h2 style={{display: 'flex', alignItems: 'center', gap: '10px', color: '#6be2ff', marginBottom: '2rem'}}>
          <Users size={28} /> Meet the Team
        </h2>
        
        <div style={{display: 'grid', gridTemplateColumns: 'repeat(auto-fit, minmax(200px, 1fr))', gap: '20px'}}>
          
          {/* Member 1: John Ryan */}
          <div style={{background: 'rgba(255,255,255,0.05)', padding: '20px', borderRadius: '15px', textAlign: 'center', border: '1px solid rgba(255,255,255,0.1)'}}>
            <div style={{width: '100px', height: '100px', borderRadius: '50%', margin: '0 auto', display: 'flex', alignItems: 'center', justifyContent: 'center', background: 'rgba(107, 226, 255, 0.2)', border: '2px solid rgba(107, 226, 255, 0.5)', overflow: 'hidden'}}>
              <img src="/john.jpg" alt="John Ryan" onError={(e) => { e.target.style.display = 'none'; e.target.nextSibling.style.display = 'block'; }} style={{width: '100%', height: '100%', objectFit: 'cover'}} />
              <div style={{display: 'none'}}><User size={48} color="#6be2ff" /></div>
            </div>
            <h3 style={{marginTop: '15px', marginBottom: '5px'}}>John Ryan</h3>
            <p style={{color: '#6be2ff', fontSize: '0.9rem', fontWeight: 'bold'}}>Lead Game Developer</p>
            <p style={{fontSize: '0.85rem', color: '#ccc', marginTop: '10px'}}>
              Spearheaded the Unity engine integrations, Mapbox geolocation features, and Firebase backend architecture for the core Step-Up experience.
            </p>
          </div>

          {/* Member 2: Raven Ashley Jose */}
          <div style={{background: 'rgba(255,255,255,0.05)', padding: '20px', borderRadius: '15px', textAlign: 'center', border: '1px solid rgba(255,255,255,0.1)'}}>
            <div style={{width: '100px', height: '100px', borderRadius: '50%', margin: '0 auto', display: 'flex', alignItems: 'center', justifyContent: 'center', background: 'rgba(107, 226, 255, 0.2)', border: '2px solid rgba(107, 226, 255, 0.5)', overflow: 'hidden'}}>
              <img src="/raven.jpg" alt="Raven Ashley Jose" onError={(e) => { e.target.style.display = 'none'; e.target.nextSibling.style.display = 'block'; }} style={{width: '100%', height: '100%', objectFit: 'cover'}} />
              <div style={{display: 'none'}}><User size={48} color="#6be2ff" /></div>
            </div>
            <h3 style={{marginTop: '15px', marginBottom: '5px'}}>Raven Ashley Jose</h3>
            <p style={{color: '#6be2ff', fontSize: '0.9rem', fontWeight: 'bold'}}>Project Leader</p>
            <p style={{fontSize: '0.85rem', color: '#ccc', marginTop: '10px'}}>
              Leads the Capstone project, manages overall team direction, and drives the research and writing of the official thesis paper.
            </p>
          </div>

          {/* Member 3: Kristina Nunag */}
          <div style={{background: 'rgba(255,255,255,0.05)', padding: '20px', borderRadius: '15px', textAlign: 'center', border: '1px solid rgba(255,255,255,0.1)'}}>
            <div style={{width: '100px', height: '100px', borderRadius: '50%', margin: '0 auto', display: 'flex', alignItems: 'center', justifyContent: 'center', background: 'rgba(107, 226, 255, 0.2)', border: '2px solid rgba(107, 226, 255, 0.5)', overflow: 'hidden'}}>
              <img src="/kristina.jpg" alt="Kristina Nunag" onError={(e) => { e.target.style.display = 'none'; e.target.nextSibling.style.display = 'block'; }} style={{width: '100%', height: '100%', objectFit: 'cover'}} />
              <div style={{display: 'none'}}><User size={48} color="#6be2ff" /></div>
            </div>
            <h3 style={{marginTop: '15px', marginBottom: '5px'}}>Kristina Nunag</h3>
            <p style={{color: '#6be2ff', fontSize: '0.9rem', fontWeight: 'bold'}}>Documentation & Research</p>
            <p style={{fontSize: '0.85rem', color: '#ccc', marginTop: '10px'}}>
              Specializes in project documentation, academic research, and collaborating on the comprehensive thesis paper for the Capstone requirements.
            </p>
          </div>

        </div>
      </div>
    </div>
  );
}
