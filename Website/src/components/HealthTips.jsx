import React, { useState } from 'react';
import { PlayCircle, Image as ImageIcon, ArrowLeft } from 'lucide-react';

export default function HealthTips() {
  const [activeTipId, setActiveTipId] = useState(null);

  const tips = [
    {
      id: 'posture',
      title: 'Posture',
      desc: 'Maintain a straight back and look forward to prevent injuries while walking or running.',
      imgPlaceholder: 'Insert Posture Image Here',
      vidPlaceholder: 'Insert Posture Video Here'
    },
    {
      id: 'cooldown',
      title: 'Cooldown',
      desc: 'Slow down your heart rate and stretch your legs, calves, and shoulders to prevent soreness.',
      imgPlaceholder: 'Insert Cooldown Image Here',
      vidPlaceholder: 'Insert Cooldown Video Here'
    },
    {
      id: 'warmup',
      title: 'Warm Up',
      desc: 'Activate your muscles before intense activity. Try hip rotations and heel-to-butt kicks.',
      imgPlaceholder: 'Insert Warm Up Image Here',
      vidPlaceholder: 'Insert Warm Up Video Here'
    },
    {
      id: 'fitness',
      title: 'Fitness Tips',
      desc: 'Stay hydrated, keep a consistent pace, and gradually increase your daily step goal.',
      imgPlaceholder: 'Insert Fitness Image Here',
      vidPlaceholder: 'Insert Fitness Video Here'
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
          
          <div style={{ display: 'grid', gridTemplateColumns: 'repeat(auto-fit, minmax(250px, 1fr))', gap: '1px', background: 'rgba(255,255,255,0.1)' }}>
            
            {/* Image Placeholder */}
            <div style={{ background: 'rgba(20, 20, 30, 0.8)', padding: '30px', display: 'flex', flexDirection: 'column', alignItems: 'center', justifyContent: 'center', minHeight: '200px' }}>
              <ImageIcon size={48} color="#4ade80" style={{ marginBottom: '15px' }} />
              <span style={{ color: '#aaa', fontSize: '0.9rem', textAlign: 'center' }}>{activeTip.imgPlaceholder}</span>
              <p style={{ fontSize: '0.75rem', color: '#666', marginTop: '10px' }}>(Replace with &lt;img src="..." /&gt;)</p>
            </div>

            {/* Video Placeholder */}
            <div style={{ background: 'rgba(20, 20, 30, 0.8)', padding: '30px', display: 'flex', flexDirection: 'column', alignItems: 'center', justifyContent: 'center', minHeight: '200px' }}>
              <PlayCircle size={48} color="#FF6B6B" style={{ marginBottom: '15px' }} />
              <span style={{ color: '#aaa', fontSize: '0.9rem', textAlign: 'center' }}>{activeTip.vidPlaceholder}</span>
              <p style={{ fontSize: '0.75rem', color: '#666', marginTop: '10px' }}>(Replace with &lt;video src="..." controls /&gt;)</p>
            </div>

          </div>
        </div>
      )}
    </div>
  );
}
