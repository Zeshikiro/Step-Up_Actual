import React, { useEffect, useState } from 'react';
import { db } from '../firebaseConfig';
import { ref, onValue } from 'firebase/database';
import { Trophy, Medal, Crown } from 'lucide-react';

export default function Leaderboard() {
  const [users, setUsers] = useState([]);

  useEffect(() => {
    const usersRef = ref(db, 'users');
    onValue(usersRef, (snapshot) => {
      const data = snapshot.val();
      if (data) {
        const userList = Object.keys(data).map(key => ({
          id: key,
          ...data[key]
        })).filter(u => u.TotalLifetimeSteps !== undefined)
          .sort((a, b) => b.TotalLifetimeSteps - a.TotalLifetimeSteps);
        
        setUsers(userList);
      }
    });
  }, []);

  const topThree = users.slice(0, 3);
  const rest = users.slice(3);

  return (
    <div className="features-grid" style={{marginTop: '2rem'}}>
      <div className="glass-card" style={{gridColumn: '1 / -1'}}>
        <h2 style={{textAlign: 'center', marginBottom: '2rem', display: 'flex', alignItems: 'center', justifyContent: 'center', gap: '10px'}}>
          <Trophy color="#FFD700" /> Global Leaderboard
        </h2>
        
        {/* PODIUM DISPLAY */}
        {topThree.length > 0 && (
          <div style={{
            display: 'flex', 
            justifyContent: 'center', 
            alignItems: 'flex-end', 
            gap: '20px',
            marginBottom: '3rem',
            marginTop: '2rem',
            minHeight: '200px'
          }}>
            {/* Rank 2 - Left */}
            {topThree.length >= 2 && (
              <div style={{display: 'flex', flexDirection: 'column', alignItems: 'center'}}>
                <Medal color="#C0C0C0" size={32} style={{marginBottom: '10px'}}/>
                <div style={{background: 'rgba(255,255,255,0.2)', padding: '20px', borderRadius: '50%', width: '80px', height: '80px', display: 'flex', alignItems: 'center', justifyContent: 'center'}}>
                  <span style={{fontSize: '1.2rem', fontWeight: 'bold'}}>#2</span>
                </div>
                <strong style={{marginTop: '10px'}}>{topThree[1].email.split('@')[0]}</strong>
                <span style={{color: '#ccc'}}>{topThree[1].TotalLifetimeSteps.toLocaleString()}</span>
                <div style={{width: '100px', height: '80px', background: '#C0C0C0', marginTop: '10px', borderRadius: '5px 5px 0 0'}}></div>
              </div>
            )}

            {/* Rank 1 - Center */}
            <div style={{display: 'flex', flexDirection: 'column', alignItems: 'center'}}>
              <Crown color="#FFD700" size={48} style={{marginBottom: '10px'}}/>
              <div style={{background: 'rgba(255,215,0,0.3)', border: '2px solid #FFD700', padding: '20px', borderRadius: '50%', width: '100px', height: '100px', display: 'flex', alignItems: 'center', justifyContent: 'center'}}>
                <span style={{fontSize: '1.5rem', fontWeight: 'bold', color: '#FFD700'}}>#1</span>
              </div>
              <strong style={{marginTop: '10px', fontSize: '1.2rem', color: '#FFD700'}}>{topThree[0].email.split('@')[0]}</strong>
              <span style={{color: '#ccc'}}>{topThree[0].TotalLifetimeSteps.toLocaleString()}</span>
              <div style={{width: '120px', height: '120px', background: '#FFD700', marginTop: '10px', borderRadius: '5px 5px 0 0'}}></div>
            </div>

            {/* Rank 3 - Right */}
            {topThree.length >= 3 && (
              <div style={{display: 'flex', flexDirection: 'column', alignItems: 'center'}}>
                <Medal color="#CD7F32" size={32} style={{marginBottom: '10px'}}/>
                <div style={{background: 'rgba(255,255,255,0.1)', padding: '20px', borderRadius: '50%', width: '80px', height: '80px', display: 'flex', alignItems: 'center', justifyContent: 'center'}}>
                  <span style={{fontSize: '1.2rem', fontWeight: 'bold'}}>#3</span>
                </div>
                <strong style={{marginTop: '10px'}}>{topThree[2].email.split('@')[0]}</strong>
                <span style={{color: '#ccc'}}>{topThree[2].TotalLifetimeSteps.toLocaleString()}</span>
                <div style={{width: '100px', height: '60px', background: '#CD7F32', marginTop: '10px', borderRadius: '5px 5px 0 0'}}></div>
              </div>
            )}
          </div>
        )}

        {/* LIST DISPLAY */}
        {rest.length > 0 && (
          <table style={{width: '100%', textAlign: 'left', marginTop: '1rem'}}>
            <thead>
              <tr>
                <th style={{padding: '10px', borderBottom: '1px solid rgba(255,255,255,0.2)'}}>Rank</th>
                <th style={{padding: '10px', borderBottom: '1px solid rgba(255,255,255,0.2)'}}>Player</th>
                <th style={{padding: '10px', borderBottom: '1px solid rgba(255,255,255,0.2)'}}>Total Steps</th>
              </tr>
            </thead>
            <tbody>
              {rest.map((user, index) => (
                <tr key={user.id}>
                  <td style={{padding: '10px'}}>#{index + 4}</td>
                  <td style={{padding: '10px'}}>{user.email ? user.email.split('@')[0] : 'Anonymous'}</td>
                  <td style={{padding: '10px'}}>{user.TotalLifetimeSteps.toLocaleString()}</td>
                </tr>
              ))}
            </tbody>
          </table>
        )}
        {users.length === 0 && <p style={{textAlign: 'center'}}>No data available. Go walk!</p>}
      </div>
    </div>
  );
}
