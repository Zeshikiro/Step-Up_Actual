import React, { useEffect, useState } from 'react';
import { useAuth } from './AuthContext';
import { db } from '../firebaseConfig';
import { ref, onValue } from 'firebase/database';
import { Activity, Award, LogOut, Footprints } from 'lucide-react';

export default function Profile() {
  const { currentUser, logout } = useAuth();
  const [userData, setUserData] = useState({ TotalLifetimeSteps: 0, currentDailySteps: 0 });
  const [userRank, setUserRank] = useState("Unranked");

  useEffect(() => {
    if (!currentUser) return;

    // 1. Fetch live user stats
    const userRef = ref(db, 'users/' + currentUser.uid);
    onValue(userRef, (snapshot) => {
      if (snapshot.exists()) {
        setUserData(snapshot.val());
      }
    });

    // 2. Calculate Rank based on TotalLifetimeSteps
    const allUsersRef = ref(db, 'users');
    onValue(allUsersRef, (snapshot) => {
      const data = snapshot.val();
      if (data) {
        const sortedList = Object.keys(data).map(key => ({
          id: key,
          ...data[key]
        })).filter(u => u.TotalLifetimeSteps !== undefined)
          .sort((a, b) => b.TotalLifetimeSteps - a.TotalLifetimeSteps);
        
        const rankIndex = sortedList.findIndex(u => u.id === currentUser.uid);
        if (rankIndex !== -1) {
          setUserRank("#" + (rankIndex + 1));
        }
      }
    });

  }, [currentUser]);

  if (!currentUser) return null;

  return (
    <div className="features-grid" style={{marginTop: '2rem'}}>
      <div className="glass-card" style={{gridColumn: '1 / -1', textAlign: 'center'}}>
        <div style={{background: 'linear-gradient(135deg, #6be2ff, #2979ff)', width: '80px', height: '80px', borderRadius: '50%', margin: '0 auto', display: 'flex', alignItems: 'center', justifyContent: 'center', boxShadow: '0 4px 15px rgba(107, 226, 255, 0.4)'}}>
          <UserIcon size={40} color="white"/>
        </div>
        <h2 style={{marginTop: '1rem'}}>{currentUser.email}</h2>
        <p style={{color: '#ccc', marginBottom: '2rem'}}>Step-Up Explorer</p>

        <div style={{display: 'grid', gridTemplateColumns: 'repeat(auto-fit, minmax(150px, 1fr))', gap: '15px', marginBottom: '2rem'}}>
          <div style={{background: 'rgba(255,255,255,0.05)', padding: '20px', borderRadius: '15px', border: '1px solid rgba(255,255,255,0.1)'}}>
            <Award color="#FFD700" size={32} style={{margin: '0 auto 10px'}}/>
            <h3 style={{fontSize: '1.8rem', margin: '0'}}>{userRank}</h3>
            <span style={{fontSize: '0.9rem', color: '#ccc'}}>Global Rank</span>
          </div>

          <div style={{background: 'rgba(255,255,255,0.05)', padding: '20px', borderRadius: '15px', border: '1px solid rgba(255,255,255,0.1)'}}>
            <Activity color="#6be2ff" size={32} style={{margin: '0 auto 10px'}}/>
            <h3 style={{fontSize: '1.8rem', margin: '0'}}>{(userData.TotalLifetimeSteps || 0).toLocaleString()}</h3>
            <span style={{fontSize: '0.9rem', color: '#ccc'}}>Lifetime Steps</span>
          </div>

          <div style={{background: 'rgba(255,255,255,0.05)', padding: '20px', borderRadius: '15px', border: '1px solid rgba(255,255,255,0.1)'}}>
            <Footprints color="#4ade80" size={32} style={{margin: '0 auto 10px'}}/>
            <h3 style={{fontSize: '1.8rem', margin: '0'}}>{(userData.currentDailySteps || 0).toLocaleString()}</h3>
            <span style={{fontSize: '0.9rem', color: '#ccc'}}>Steps Today</span>
          </div>
        </div>

        <button className="cta-button" onClick={logout} style={{display: 'inline-flex', alignItems: 'center', gap: '8px', background: 'rgba(255, 50, 50, 0.2)', border: '1px solid rgba(255, 50, 50, 0.5)', color: '#ff6b6b'}}>
          <LogOut size={18}/> Sign Out
        </button>
      </div>
    </div>
  );
}

const UserIcon = ({size, color}) => (
  <svg xmlns="http://www.w3.org/2000/svg" width={size} height={size} viewBox="0 0 24 24" fill="none" stroke={color} strokeWidth="2" strokeLinecap="round" strokeLinejoin="round">
    <path d="M19 21v-2a4 4 0 0 0-4-4H9a4 4 0 0 0-4 4v2"></path>
    <circle cx="12" cy="7" r="4"></circle>
  </svg>
);
