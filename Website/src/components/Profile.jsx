import { onValue, ref } from 'firebase/database';
import { Activity, Award, Footprints, LogOut, Mail, Lock } from 'lucide-react';
import { useEffect, useState } from 'react';
import { db } from '../firebaseConfig';
import { useAuth } from './AuthContext';

export default function Profile() {
  const { currentUser, logout, resetPassword } = useAuth();
  const [userData, setUserData] = useState({ TotalLifetimeSteps: 0, currentDailySteps: 0 });
  const [userRank, setUserRank] = useState("Unranked");

  useEffect(() => {
    if (!currentUser) return;

    const userRef = ref(db, 'users/' + currentUser.uid);
    onValue(userRef, (snapshot) => {
      if (snapshot.exists()) {
        setUserData(snapshot.val());
      }
    });

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
        textAlign: 'center',
        boxShadow: '8px 8px 0 rgba(0, 0, 0, 0.35)'
      }}>

        {/* Title board */}
        <div style={{
          background: '#f59b35',
          color: '#ffffff',
          border: '5px solid #171717',
          borderRadius: '14px',
          padding: '1rem',
          maxWidth: '430px',
          margin: '0 auto 1.8rem',
          textAlign: 'center',
          boxShadow: '0 7px 0 #9b531e'
        }}>
          <h2 style={{
            margin: 0,
            fontFamily: '"Press Start 2P", cursive',
            fontSize: 'clamp(0.85rem, 2.4vw, 1.2rem)',
            textShadow: '3px 3px 0 #171717',
            color: '#ffffff'
          }}>
            PLAYER PROFILE
          </h2>
        </div>

        {/* Avatar */}
        <div style={{
          background: '#ffd84d',
          width: '110px',
          height: '110px',
          borderRadius: '18px',
          border: '5px solid #171717',
          margin: '0 auto',
          display: 'flex',
          alignItems: 'center',
          justifyContent: 'center',
          boxShadow: '6px 6px 0 rgba(0,0,0,0.3)'
        }}>
          <UserIcon size={55} color="#171717" />
        </div>

        <h2 style={{
          marginTop: '1rem',
          marginBottom: '0.4rem',
          color: '#9b531e',
          fontSize: '1.65rem',
          wordBreak: 'break-word',
          textTransform: 'capitalize'
        }}>
          {userData?.username || currentUser.email.split('@')[0]}
        </h2>
        <p style={{
          color: '#1b2433',
          marginBottom: '0.5rem',
          fontWeight: '600',
          fontSize: '1rem'
        }}>
          {currentUser.email}
        </p>

        <p style={{
          color: '#1b2433',
          marginBottom: '2rem',
          fontWeight: '800',
          fontSize: '1.05rem'
        }}>
          Step-Up Explorer
        </p>

        {/* Stats cards */}
        <div style={{
          display: 'grid',
          gridTemplateColumns: 'repeat(auto-fit, minmax(180px, 1fr))',
          gap: '18px',
          marginBottom: '2rem'
        }}>
          <StatCard
            icon={<Award color="#ffd84d" size={36} />}
            value={userRank}
            label="Global Rank"
            bg="#fff9e9"
          />

          <StatCard
            icon={<Activity color="#2f8ed8" size={36} />}
            value={(userData.TotalLifetimeSteps || 0).toLocaleString()}
            label="Lifetime Steps"
            bg="#fff9e9"
          />

          <StatCard
            icon={<Footprints color="#3fd66b" size={36} />}
            value={(userData.currentDailySteps || 0).toLocaleString()}
            label="Steps Today"
            bg="#fff9e9"
          />
        </div>

        <div style={{ display: 'flex', gap: '10px', justifyContent: 'center', flexWrap: 'wrap', marginBottom: '1rem' }}>
          <button
            onClick={() => {
              window.location.href = '?tab=change-email';
            }}
            style={{
              background: '#2f8ed8',
              color: '#ffffff',
              border: '4px solid #171717',
              borderRadius: '12px',
              padding: '13px 20px',
              cursor: 'pointer',
              fontWeight: '900',
              fontSize: '1rem',
              fontFamily: 'inherit',
              boxShadow: '0 6px 0 #174b75',
              display: 'inline-flex',
              alignItems: 'center',
              gap: '8px'
            }}
          >
            <Mail size={18} />
            Update Email
          </button>
          
          <button
            onClick={async () => {
              try {
                await resetPassword(currentUser.email);
                alert("Password reset email sent! Please check your inbox.");
              } catch (err) {
                alert("Failed to send reset email: " + err.message);
              }
            }}
            style={{
              background: '#f59b35',
              color: '#ffffff',
              border: '4px solid #171717',
              borderRadius: '12px',
              padding: '13px 20px',
              cursor: 'pointer',
              fontWeight: '900',
              fontSize: '1rem',
              boxShadow: '0 6px 0 #9b531e',
              display: 'inline-flex',
              alignItems: 'center',
              gap: '8px'
            }}
          >
            <Lock size={18} />
            Update Password
          </button>
        </div>

        {/* Sign out button */}
        <button
          onClick={logout}
          style={{
            background: '#ef4444',
            color: '#ffffff',
            border: '4px solid #171717',
            borderRadius: '12px',
            padding: '13px 20px',
            cursor: 'pointer',
            fontWeight: '900',
            fontSize: '1rem',
            boxShadow: '0 6px 0 #991b1b',
            display: 'inline-flex',
            alignItems: 'center',
            gap: '8px'
          }}
        >
          <LogOut size={18} />
          Sign Out
        </button>
      </div>
    </div>
  );
}

function StatCard({ icon, value, label, bg }) {
  return (
    <div style={{
      background: bg,
      padding: '1.4rem',
      borderRadius: '18px',
      border: '4px solid #171717',
      boxShadow: '5px 5px 0 rgba(0,0,0,0.25)'
    }}>
      <div style={{
        width: '58px',
        height: '58px',
        background: '#f59b35',
        border: '4px solid #171717',
        borderRadius: '14px',
        margin: '0 auto 12px',
        display: 'flex',
        alignItems: 'center',
        justifyContent: 'center'
      }}>
        {icon}
      </div>

      <h3 style={{
        fontSize: '1.8rem',
        margin: '0',
        color: '#9b531e',
        fontWeight: '900'
      }}>
        {value}
      </h3>

      <span style={{
        fontSize: '1rem',
        color: '#1b2433',
        fontWeight: '800'
      }}>
        {label}
      </span>
    </div>
  );
}

const UserIcon = ({ size, color }) => (
  <svg xmlns="http://www.w3.org/2000/svg" width={size} height={size} viewBox="0 0 24 24" fill="none" stroke={color} strokeWidth="2" strokeLinecap="round" strokeLinejoin="round">
    <path d="M19 21v-2a4 4 0 0 0-4-4H9a4 4 0 0 0-4 4v2"></path>
    <circle cx="12" cy="7" r="4"></circle>
  </svg>
);