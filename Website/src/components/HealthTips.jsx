import React, { useState } from 'react';
import { ArrowLeft } from 'lucide-react';

export default function HealthTips() {
  const [activeTipId, setActiveTipId] = useState(null);

  const tips = [
    {
      id: 'posture',
      title: 'Posture',
      desc: 'Maintain a straight back and look forward to prevent injuries while walking or running.',
      images: [
        '/Images and Videos/Posture/correct posture.jpg',
        '/Images and Videos/Posture/gaitcycle2-1.jpg',
        '/Images and Videos/Posture/posture.jpg'
      ]
    },
    {
      id: 'cooldown',
      title: 'Cooldown',
      desc: 'Slow down your heart rate and stretch your legs, calves, and shoulders to prevent soreness.'
    },
    {
      id: 'warmup',
      title: 'Warm Up',
      desc: 'Activate your muscles before intense activity. Try hip rotations and heel-to-butt kicks.',
      videoUrl: 'https://www.youtube.com/embed/WBhmNTykYiE'
    },
    {
      id: 'fitness',
      title: 'Fitness Tips',
      desc: 'Stay hydrated, keep a consistent pace, and gradually increase your daily step goal.',
      videoUrl: 'https://www.youtube.com/embed/H1jD__EoBNY'
    }
  ];

  const activeTip = tips.find(t => t.id === activeTipId);

  return (
    <div style={{ padding: '20px', maxWidth: '800px', margin: '0 auto', paddingBottom: '100px' }}>
      <header style={{ textAlign: 'center', marginBottom: '2rem', position: 'relative' }}>
        {activeTipId && (
          <button 
            onClick={() => setActiveTipId(null)}
            style={{ 
              position: 'absolute', left: 0, top: '10px', background: 'none', border: 'none', 
              color: '#6be2ff', cursor: 'pointer', display: 'flex', alignItems: 'center', gap: '5px',
              fontSize: '1rem', fontWeight: 'bold'
            }}>
            <ArrowLeft size={20}/> Back
          </button>
        )}
        <h2 className="title-gradient" style={{ fontSize: '2.5rem', margin: '10px 0' }}>Health Hub</h2>
        {!activeTipId && <p style={{ color: '#ccc' }}>Select a category below to view tutorials and tips.</p>}
      </header>

      {!activeTipId ? (
        <div style={{ display: 'flex', flexDirection: 'column', gap: '20px', maxWidth: '300px', margin: '0 auto', marginTop: '3rem' }}>
          {tips.map((tip) => (
            <button 
              key={tip.id}
              onClick={() => setActiveTipId(tip.id)}
              style={{
                background: 'white',
                color: 'black',
                border: 'none',
                padding: '20px',
                fontSize: '1.5rem',
                fontWeight: 'bold',
                borderRadius: '5px',
                cursor: 'pointer',
                boxShadow: '0 4px 15px rgba(0,0,0,0.5)',
                transition: 'transform 0.1s ease',
                textAlign: 'center'
              }}
              onMouseOver={(e) => e.currentTarget.style.transform = 'scale(1.05)'}
              onMouseOut={(e) => e.currentTarget.style.transform = 'scale(1)'}
            >
              {tip.title}
            </button>
          ))}
        </div>
      ) : (
        <div className="glass-card" style={{ padding: '0', overflow: 'hidden' }}>
          <div style={{ padding: '20px', borderBottom: '1px solid rgba(255,255,255,0.1)' }}>
            <h3 style={{ margin: '0 0 10px 0', fontSize: '1.8rem', color: '#6be2ff' }}>{activeTip.title}</h3>
            <p style={{ margin: 0, color: '#ddd' }}>{activeTip.desc}</p>
          </div>
          
          <div style={{ padding: '20px', background: 'rgba(20, 20, 30, 0.8)', display: 'flex', flexDirection: 'column', gap: '20px', alignItems: 'center' }}>
            
            {activeTip.images && activeTip.images.length > 0 && (
              <div style={{ display: 'flex', flexWrap: 'wrap', gap: '15px', justifyContent: 'center' }}>
                {activeTip.images.map((imgSrc, idx) => (
                  <img 
                    key={idx} 
                    src={imgSrc} 
                    alt={${activeTip.title} \} 
                    style={{ maxWidth: '100%', maxHeight: '400px', borderRadius: '8px', objectFit: 'contain', boxShadow: '0 4px 10px rgba(0,0,0,0.5)' }} 
                  />
                ))}
              </div>
            )}

            {activeTip.videoUrl && (
              <div style={{ width: '100%', maxWidth: '315px', margin: '0 auto', aspectRatio: '9/16' }}>
                <iframe 
                  width="100%" 
                  height="100%" 
                  src={activeTip.videoUrl} 
                  title="YouTube video player" 
                  frameBorder="0" 
                  allow="accelerometer; autoplay; clipboard-write; encrypted-media; gyroscope; picture-in-picture; web-share" 
                  referrerPolicy="strict-origin-when-cross-origin" 
                  allowFullScreen
                  style={{ borderRadius: '8px', boxShadow: '0 4px 10px rgba(0,0,0,0.5)' }}
                ></iframe>
              </div>
            )}

            {!activeTip.images && !activeTip.videoUrl && (
              <p style={{ color: '#aaa', fontStyle: 'italic', padding: '40px 0' }}>More content coming soon...</p>
            )}

          </div>
        </div>
      )}
    </div>
  );
}
