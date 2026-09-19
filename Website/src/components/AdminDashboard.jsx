// AdminDashboard.jsx — Infirmary Staff Admin Panel
// Restricted to users with role:"admin" in the /admins/{uid} Firebase node
// Shows: user stats overview, activity table with respondent codes (privacy), health/wellness info
// Security: Firebase Rules block non-admins from reading /admins, and this component
// double-checks on the client side too before rendering any data

import { useEffect, useState } from 'react';
import { ref, onValue, get } from 'firebase/database';
import { db } from '../firebaseConfig';
import { useAuth } from './AuthContext';
import { Shield, Users, Footprints, Activity, Heart, LogOut, Download } from 'lucide-react';

export default function AdminDashboard() {
  const { currentUser, logout } = useAuth();
  const [users, setUsers] = useState([]);
  const [loading, setLoading] = useState(true);
  const [isAdmin, setIsAdmin] = useState(false);
  const [adminCheckDone, setAdminCheckDone] = useState(false);
  const [activeSection, setActiveSection] = useState('overview');
  const [searchQuery, setSearchQuery] = useState('');

  // ============================
  // CHECK IF USER IS ADMIN VIA FIREBASE
  // Checks the /admins/{uid} node in the database
  // ============================
  useEffect(() => {
    if (!currentUser) {
      setAdminCheckDone(true);
      return;
    }

    const adminRef = ref(db, 'admins/' + currentUser.uid);
    get(adminRef).then((snapshot) => {
      if (snapshot.exists() && snapshot.val().role === 'admin') {
        setIsAdmin(true);
      } else {
        setIsAdmin(false);
      }
      setAdminCheckDone(true);
    }).catch(() => {
      setIsAdmin(false);
      setAdminCheckDone(true);
    });
  }, [currentUser]);

  // ============================
  // LOAD ALL USER DATA (only if admin)
  // ============================
  useEffect(() => {
    if (!isAdmin) return;

    const usersRef = ref(db, 'users');
    const unsubscribe = onValue(usersRef, (snapshot) => {
      const data = snapshot.val();
      if (data) {
        const userList = Object.keys(data).map((uid, index) => ({
          id: uid,
          respondentCode: `R-${String(index + 1).padStart(3, '0')}`,
          ...data[uid]
        }));
        setUsers(userList);
      }
      setLoading(false);
    });

    return () => unsubscribe();
  }, [isAdmin]);

  // ============================
  // LOADING / ACCESS DENIED SCREENS
  // ============================
  if (!adminCheckDone) {
    return (
      <div style={{ textAlign: 'center', padding: '3rem', color: '#1b2433', fontFamily: '"Segoe UI", "Inter", "Roboto", sans-serif' }}>
        <p style={{ fontSize: '1.2rem', fontWeight: '700' }}>Verifying admin access...</p>
      </div>
    );
  }

  if (!currentUser || !isAdmin) {
    return (
      <div style={{
        textAlign: 'center',
        padding: '3rem 1.5rem',
        maxWidth: '500px',
        margin: '0 auto',
        fontFamily: '"Segoe UI", "Inter", "Roboto", sans-serif'
      }}>
        <div style={{
          background: '#fee2e2',
          border: '4px solid #171717',
          borderRadius: '16px',
          padding: '2rem',
          boxShadow: '5px 5px 0 rgba(0,0,0,0.25)'
        }}>
          <Shield size={48} color="#dc2626" style={{ marginBottom: '1rem' }} />
          <h2 style={{ color: '#dc2626', margin: '0 0 1rem 0' }}>Access Restricted</h2>
          <p style={{ color: '#1b2433', lineHeight: '1.5' }}>
            This dashboard is only available to authorized Infirmary staff.
            Please log in with an authorized admin account to continue.
          </p>
        </div>
      </div>
    );
  }

  if (loading) {
    return (
      <div style={{ textAlign: 'center', padding: '3rem', color: '#1b2433' }}>
        <p style={{ fontSize: '1.2rem', fontWeight: '700' }}>Loading dashboard data...</p>
      </div>
    );
  }

  // ============================
  // COMPUTED STATS
  // ============================
  const totalUsers = users.length;
  const activeUsers = users.filter(u => (u.TotalLifetimeSteps || 0) > 0).length;
  const totalStepsAll = users.reduce((sum, u) => sum + (u.TotalLifetimeSteps || 0), 0);
  const avgSteps = totalUsers > 0 ? Math.round(totalStepsAll / totalUsers) : 0;
  const avgDailySteps = totalUsers > 0 ? Math.round(users.reduce((sum, u) => sum + Math.max(u.DailySteps || 0, u.currentDailySteps || 0), 0) / totalUsers) : 0;

  const bmiBreakdown = {
    Underweight: users.filter(u => u.bmiCategory === 'Underweight').length,
    Normal: users.filter(u => u.bmiCategory === 'Normal').length,
    Overweight: users.filter(u => u.bmiCategory === 'Overweight').length,
    Obese: users.filter(u => u.bmiCategory === 'Obese').length,
    'Not Set': users.filter(u => !u.bmiCategory).length,
  };

  const onboardedUsers = users.filter(u => u.OnboardingComplete === 1).length;

  // Filter users by search
  const filteredUsers = users.filter(u => {
    const q = searchQuery.toLowerCase();
    return (
      (u.username || '').toLowerCase().includes(q) ||
      (u.respondentCode || '').toLowerCase().includes(q)
    );
  });

  // ============================
  // EXPORT TO CSV
  // ============================
  const handleExport = () => {
    // 1. Gather all unique dates recorded in dailyHistory across all users
    const allDatesSet = new Set();
    users.forEach(u => {
      if (u.dailyHistory) {
        Object.keys(u.dailyHistory).forEach(dateStr => allDatesSet.add(dateStr));
      }
    });
    // Sort dates chronologically (e.g., 2026-09-18, 2026-09-19)
    const sortedDates = Array.from(allDatesSet).sort();

    // 2. Build the CSV Header dynamically
    let header = 'Respondent,Username,Today Steps,Yesterday Steps,Weekly Steps,Total Lifetime Steps,Step Goal,Streak,Age,Weight(kg),Height(cm),BMI,BMI Status,Onboarding';
    sortedDates.forEach(date => {
      header += `,${date} Steps,${date} Status`;
    });
    header += '\n';

    let csv = header;

    // 3. Build each user's row
    users.forEach(u => {
      const today = Math.max(u.DailySteps || 0, u.currentDailySteps || 0);
      const row = [
        u.respondentCode,
        u.username || 'N/A',
        today,
        u.YesterdaysSteps || 0,
        u.WeeklySteps || 0,
        u.TotalLifetimeSteps || 0,
        u.stepGoal || 0,
        u.CurrentStreak || 0,
        u.SavedAge || 'N/A',
        u.SavedWeight || 'N/A',
        u.SavedHeight || 'N/A',
        u.bmi ? parseFloat(u.bmi).toFixed(2) : 'N/A',
        u.bmiCategory || 'N/A',
        u.OnboardingComplete === 1 ? 'Yes' : 'No'
      ];

      // 4. Append history columns for this specific user
      sortedDates.forEach(date => {
        if (u.dailyHistory && u.dailyHistory[date]) {
          row.push(u.dailyHistory[date].steps || 0);
          row.push(u.dailyHistory[date].status || 'Inactive');
        } else {
          row.push(0);
          row.push('No Record'); // If they didn't have the app installed that day
        }
      });

      csv += row.join(',') + '\n';
    });

    const blob = new Blob([csv], { type: 'text/csv' });
    const url = URL.createObjectURL(blob);
    const a = document.createElement('a');
    a.href = url;
    a.download = 'StepUp_Activity_Report_' + new Date().toISOString().split('T')[0] + '.csv';
    a.click();
    URL.revokeObjectURL(url);
  };

  // ============================
  // STYLES
  // ============================
  const cardStyle = {
    background: '#ffffff',
    border: '4px solid #171717',
    borderRadius: '16px',
    padding: '1.25rem',
    boxShadow: '4px 4px 0 rgba(0,0,0,0.2)',
    textAlign: 'center',
    flex: '1 1 140px',
    minWidth: '140px'
  };

  const statNumber = {
    fontSize: '2rem',
    fontWeight: '900',
    color: '#174b75',
    margin: '0.5rem 0 0.25rem'
  };

  const statLabel = {
    fontSize: '0.85rem',
    fontWeight: '700',
    color: '#555',
    margin: 0
  };

  const sectionBtnStyle = (active) => ({
    background: active ? '#174b75' : '#e2e8f0',
    color: active ? '#fff' : '#1b2433',
    border: '3px solid #171717',
    borderRadius: '10px',
    padding: '10px 18px',
    fontWeight: '800',
    cursor: 'pointer',
    fontSize: '0.9rem',
    transition: 'all 0.15s'
  });

  // ============================
  // RENDER
  // ============================
  return (
    <div style={{
      maxWidth: '950px',
      margin: '0 auto',
      padding: '1rem',
      color: '#1b2433',
      fontFamily: '"Segoe UI", "Inter", "Roboto", -apple-system, sans-serif'
    }}>
      {/* HEADER */}
      <div style={{
        display: 'flex',
        justifyContent: 'space-between',
        alignItems: 'center',
        flexWrap: 'wrap',
        gap: '1rem',
        marginBottom: '1.5rem'
      }}>
        <div style={{ display: 'flex', alignItems: 'center', gap: '10px' }}>
          <Shield size={28} color="#174b75" />
          <h2 style={{ margin: 0, color: '#174b75', fontSize: '1.5rem' }}>
            STEP-UP Admin Dashboard
          </h2>
        </div>
        <div style={{ display: 'flex', gap: '8px', alignItems: 'center', flexWrap: 'wrap' }}>
          <button onClick={handleExport} style={{
            background: '#22c55e',
            color: '#000',
            border: '3px solid #171717',
            borderRadius: '10px',
            padding: '8px 16px',
            fontWeight: '800',
            cursor: 'pointer',
            display: 'flex',
            alignItems: 'center',
            gap: '6px',
            fontSize: '0.85rem'
          }}>
            <Download size={16} /> Export
          </button>
          <button onClick={logout} style={{
            background: '#ef4444',
            color: '#fff',
            border: '3px solid #171717',
            borderRadius: '10px',
            padding: '8px 16px',
            fontWeight: '800',
            cursor: 'pointer',
            display: 'flex',
            alignItems: 'center',
            gap: '6px',
            fontSize: '0.85rem'
          }}>
            <LogOut size={16} /> Sign Out
          </button>
        </div>
      </div>

      {/* SECTION TABS */}
      <div style={{ display: 'flex', gap: '8px', marginBottom: '1.5rem', flexWrap: 'wrap' }}>
        <button onClick={() => setActiveSection('overview')} style={sectionBtnStyle(activeSection === 'overview')}>
          📊 Dashboard
        </button>
        <button onClick={() => setActiveSection('students')} style={sectionBtnStyle(activeSection === 'students')}>
          👥 Activity Overview
        </button>
        <button onClick={() => setActiveSection('wellness')} style={sectionBtnStyle(activeSection === 'wellness')}>
          💚 Health Info
        </button>
      </div>

      {/* ========================== */}
      {/* OVERVIEW / DASHBOARD SECTION */}
      {/* ========================== */}
      {activeSection === 'overview' && (
        <>
          {/* Stat Cards Row */}
          <div style={{ display: 'flex', gap: '1rem', flexWrap: 'wrap', marginBottom: '1.5rem' }}>
            <div style={cardStyle}>
              <Users size={24} color="#2f8ed8" />
              <p style={statNumber}>{totalUsers}</p>
              <p style={statLabel}>Registered Users</p>
            </div>
            <div style={cardStyle}>
              <Activity size={24} color="#22c55e" />
              <p style={statNumber}>{activeUsers}</p>
              <p style={statLabel}>Active Users</p>
            </div>
            <div style={cardStyle}>
              <Footprints size={24} color="#f59e0b" />
              <p style={statNumber}>{totalStepsAll.toLocaleString()}</p>
              <p style={statLabel}>Total Steps Recorded</p>
            </div>
            <div style={cardStyle}>
              <Footprints size={24} color="#8b5cf6" />
              <p style={statNumber}>{avgSteps.toLocaleString()}</p>
              <p style={statLabel}>Avg Steps / User</p>
            </div>
          </div>

          {/* BMI Distribution */}
          <div style={{
            background: '#f0fdf4',
            border: '4px solid #171717',
            borderRadius: '16px',
            padding: '1.5rem',
            boxShadow: '4px 4px 0 rgba(0,0,0,0.2)',
            marginBottom: '1.5rem'
          }}>
            <h3 style={{ margin: '0 0 1rem', color: '#174b75' }}>
              <Heart size={20} style={{ marginRight: '8px', verticalAlign: 'middle' }} />
              BMI Distribution
            </h3>
            <div style={{ display: 'flex', gap: '1rem', flexWrap: 'wrap' }}>
              {Object.entries(bmiBreakdown).map(([category, count]) => (
                <div key={category} style={{
                  background: '#fff',
                  border: '3px solid #171717',
                  borderRadius: '12px',
                  padding: '0.75rem 1.25rem',
                  textAlign: 'center',
                  flex: '1 1 100px',
                  minWidth: '100px'
                }}>
                  <p style={{ margin: 0, fontWeight: '900', fontSize: '1.5rem', color: '#174b75' }}>{count}</p>
                  <p style={{ margin: '4px 0 0', fontWeight: '700', fontSize: '0.8rem', color: '#555' }}>{category}</p>
                </div>
              ))}
            </div>
          </div>

          {/* General Physical-Activity Summary */}
          <div style={{
            background: '#eff6ff',
            border: '4px solid #171717',
            borderRadius: '16px',
            padding: '1.5rem',
            boxShadow: '4px 4px 0 rgba(0,0,0,0.2)'
          }}>
            <h3 style={{ margin: '0 0 1rem', color: '#174b75' }}>📋 Physical-Activity Summary</h3>
            <ul style={{ margin: 0, paddingLeft: '1.5rem', lineHeight: '2', fontSize: '0.95rem' }}>
              <li><b>{onboardedUsers}</b> out of <b>{totalUsers}</b> users have completed onboarding setup</li>
              <li><b>{activeUsers}</b> users have recorded at least 1 step</li>
              <li>Average daily steps across all users: <b>{avgDailySteps.toLocaleString()}</b></li>
              <li>Total community steps recorded: <b>{totalStepsAll.toLocaleString()}</b></li>
              <li>Average lifetime steps per user: <b>{avgSteps.toLocaleString()}</b></li>
            </ul>
          </div>
        </>
      )}

      {/* ========================== */}
      {/* ACTIVITY OVERVIEW SECTION */}
      {/* ========================== */}
      {activeSection === 'students' && (
        <>
          {/* Search */}
          <div style={{ marginBottom: '1rem' }}>
            <input
              type="text"
              placeholder="Search by username or respondent code..."
              value={searchQuery}
              onChange={(e) => setSearchQuery(e.target.value)}
              style={{
                width: '100%',
                padding: '12px 16px',
                border: '4px solid #171717',
                borderRadius: '12px',
                fontSize: '1rem',
                fontWeight: '600',
                boxSizing: 'border-box',
                outline: 'none'
              }}
            />
          </div>

          {/* Activity Table */}
          <div style={{
            background: '#fff',
            border: '4px solid #171717',
            borderRadius: '16px',
            overflow: 'hidden',
            boxShadow: '4px 4px 0 rgba(0,0,0,0.2)'
          }}>
            <div style={{ overflowX: 'auto' }}>
              <table style={{
                width: '100%',
                borderCollapse: 'collapse',
                fontSize: '0.85rem'
              }}>
                <thead>
                  <tr style={{ background: '#174b75', color: '#fff' }}>
                    <th style={thStyle}>User ID</th>
                    <th style={thStyle}>Username</th>
                    <th style={thStyle}>Daily Steps</th>
                    <th style={thStyle}>Weekly Steps</th>
                    <th style={thStyle}>Lifetime Steps</th>
                    <th style={thStyle}>Streak</th>
                    <th style={thStyle}>Activity Status</th>
                  </tr>
                </thead>
                <tbody>
                  {filteredUsers.map((u, i) => {
                    const todayLive = u.currentDailySteps || 0;
                    const today = Math.max(todayLive, u.DailySteps || 0);
                    const lifetime = u.TotalLifetimeSteps || 0;
                    const weekly = u.WeeklySteps || 0;
                    const streak = u.CurrentStreak || 0;

                    // Compare LastLoginDate (written by Unity app) to today's local date
                    // This is the most accurate way to determine if they were ACTUALLY active today
                    // DailySteps alone is unreliable — it doesn't reset until the user opens the app
                    const todayStr = new Date().toLocaleDateString('en-CA'); // "YYYY-MM-DD" format
                    const lastLogin = u.LastLoginDate || '';
                    const openedAppToday = lastLogin === todayStr;

                    let activityStatus = 'Inactive';
                    let statusColor = '#94a3b8';

                    if (openedAppToday && todayLive > 0) {
                      // Opened the app today AND steps are actively being pushed
                      activityStatus = 'Active Now';
                      statusColor = '#22c55e';
                    } else if (openedAppToday) {
                      // Opened the app today (confirmed by date), may not have walked yet
                      activityStatus = 'Active Today';
                      statusColor = '#16a34a';
                    } else if (streak > 0 || weekly > 0) {
                      // Active recently but didn't open today
                      activityStatus = 'Recently Active';
                      statusColor = '#f59e0b';
                    } else if (lifetime > 0) {
                      // Walked before but not recently
                      activityStatus = 'Has History';
                      statusColor = '#6b7280';
                    } else if (u.OnboardingComplete === 1) {
                      // Set up the app but never walked
                      activityStatus = 'New User';
                      statusColor = '#3b82f6';
                    }

                    return (
                      <tr key={u.id} style={{ background: i % 2 === 0 ? '#f8fafc' : '#fff' }}>
                        <td style={tdStyle}>{u.respondentCode}</td>
                        <td style={{ ...tdStyle, fontWeight: '700' }}>{u.username || '—'}</td>
                        <td style={tdStyle}>{today.toLocaleString()}</td>
                        <td style={tdStyle}>{(u.WeeklySteps || 0).toLocaleString()}</td>
                        <td style={{ ...tdStyle, fontWeight: '700', color: '#174b75' }}>{lifetime.toLocaleString()}</td>
                        <td style={tdStyle}>{u.CurrentStreak || 0}🔥</td>
                        <td style={tdStyle}>
                          <span style={{
                            background: statusColor,
                            color: '#fff',
                            padding: '3px 10px',
                            borderRadius: '6px',
                            fontSize: '0.75rem',
                            fontWeight: '800',
                            whiteSpace: 'nowrap'
                          }}>
                            {activityStatus}
                          </span>
                        </td>
                      </tr>
                    );
                  })}
                </tbody>
              </table>
            </div>
            <div style={{ padding: '12px', textAlign: 'center', color: '#777', fontSize: '0.85rem', fontWeight: '600' }}>
              Showing {filteredUsers.length} of {totalUsers} users
            </div>
          </div>
        </>
      )}

      {/* ========================== */}
      {/* HEALTH / WELLNESS CONTENT SECTION */}
      {/* ========================== */}
      {activeSection === 'wellness' && (
        <>
          {/* BMI Guidance */}
          <div style={{
            background: '#eff6ff',
            border: '4px solid #171717',
            borderRadius: '16px',
            padding: '1.5rem',
            boxShadow: '4px 4px 0 rgba(0,0,0,0.2)',
            marginBottom: '1.5rem'
          }}>
            <h3 style={{ margin: '0 0 1rem', color: '#174b75' }}>📊 BMI Guidance</h3>
            <table style={{ width: '100%', borderCollapse: 'collapse', fontSize: '0.95rem' }}>
              <thead>
                <tr style={{ borderBottom: '3px solid #171717' }}>
                  <th style={{ padding: '10px', textAlign: 'left' }}>BMI Range</th>
                  <th style={{ padding: '10px', textAlign: 'left' }}>Status</th>
                  <th style={{ padding: '10px', textAlign: 'left' }}>Recommendation</th>
                </tr>
              </thead>
              <tbody>
                <tr style={{ borderBottom: '1px solid #ddd' }}>
                  <td style={{ padding: '10px' }}>Below 18.5</td>
                  <td style={{ padding: '10px' }}><span style={{ background: '#3b82f6', color: '#fff', padding: '2px 8px', borderRadius: '6px', fontWeight: '700' }}>Underweight</span></td>
                  <td style={{ padding: '10px' }}>Light walking encouraged. Focus on nutrition and gradual activity.</td>
                </tr>
                <tr style={{ borderBottom: '1px solid #ddd' }}>
                  <td style={{ padding: '10px' }}>18.5 – 24.9</td>
                  <td style={{ padding: '10px' }}><span style={{ background: '#22c55e', color: '#fff', padding: '2px 8px', borderRadius: '6px', fontWeight: '700' }}>Normal</span></td>
                  <td style={{ padding: '10px' }}>Maintain current activity level. 8,000–10,000 steps/day recommended.</td>
                </tr>
                <tr style={{ borderBottom: '1px solid #ddd' }}>
                  <td style={{ padding: '10px' }}>25.0 – 29.9</td>
                  <td style={{ padding: '10px' }}><span style={{ background: '#f59e0b', color: '#fff', padding: '2px 8px', borderRadius: '6px', fontWeight: '700' }}>Overweight</span></td>
                  <td style={{ padding: '10px' }}>Increase daily steps gradually. Aim for 10,000+ steps with consistency.</td>
                </tr>
                <tr>
                  <td style={{ padding: '10px' }}>30.0 and above</td>
                  <td style={{ padding: '10px' }}><span style={{ background: '#ef4444', color: '#fff', padding: '2px 8px', borderRadius: '6px', fontWeight: '700' }}>Obese</span></td>
                  <td style={{ padding: '10px' }}>Start with light activity. Seek medical guidance for a safe exercise plan.</td>
                </tr>
              </tbody>
            </table>
          </div>

          {/* Walking Recommendations */}
          <div style={{
            background: '#f0fdf4',
            border: '4px solid #171717',
            borderRadius: '16px',
            padding: '1.5rem',
            boxShadow: '4px 4px 0 rgba(0,0,0,0.2)',
            marginBottom: '1.5rem'
          }}>
            <h3 style={{ margin: '0 0 1rem', color: '#174b75' }}>🏃 Walking Recommendations</h3>
            <ul style={{ lineHeight: '2', fontSize: '0.95rem', paddingLeft: '1.5rem' }}>
              <li>The World Health Organization recommends at least <b>150 minutes of moderate-intensity physical activity per week</b> for adults.</li>
              <li>Walking <b>8,000–10,000 steps per day</b> is associated with lower mortality risk and improved cardiovascular health.</li>
              <li>Even short walks of <b>10–15 minutes</b> can provide mental health benefits such as reduced stress and improved mood.</li>
              <li>Users should aim to <b>increase steps gradually</b> (10% per week) to avoid overexertion injuries.</li>
              <li>Walking after meals can help regulate <b>blood sugar levels</b>, especially beneficial for users at risk of diabetes.</li>
            </ul>
          </div>

          {/* Safety Reminders */}
          <div style={{
            background: '#fefce8',
            border: '4px solid #171717',
            borderRadius: '16px',
            padding: '1.5rem',
            boxShadow: '4px 4px 0 rgba(0,0,0,0.2)',
            marginBottom: '1.5rem'
          }}>
            <h3 style={{ margin: '0 0 1rem', color: '#174b75' }}>⚠️ Safety Reminders</h3>
            <ul style={{ lineHeight: '2', fontSize: '0.95rem', paddingLeft: '1.5rem' }}>
              <li>STEP-UP is a <b>fitness encouragement tool</b>, not a medical device.</li>
              <li>BMI calculations are for general guidance only and do not replace professional health assessments.</li>
              <li>Users with pre-existing medical conditions should consult their physician before increasing physical activity.</li>
              <li>The app includes an <b>anti-cheat system</b> that detects unnatural movement patterns to ensure data integrity.</li>
              <li>All user data is stored securely in Firebase with authentication-based access controls.</li>
            </ul>
          </div>

          {/* Warm-up / Cooldown Tips */}
          <div style={{
            background: '#fdf2f8',
            border: '4px solid #171717',
            borderRadius: '16px',
            padding: '1.5rem',
            boxShadow: '4px 4px 0 rgba(0,0,0,0.2)'
          }}>
            <h3 style={{ margin: '0 0 1rem', color: '#174b75' }}>🧘 Warm-up / Cooldown Tips</h3>
            <ul style={{ lineHeight: '2', fontSize: '0.95rem', paddingLeft: '1.5rem' }}>
              <li><b>Before walking:</b> Do light stretching for 2–3 minutes, focusing on calves, hamstrings, and ankles.</li>
              <li><b>During walks:</b> Maintain good posture — head up, shoulders relaxed, arms swinging naturally.</li>
              <li><b>After walking:</b> Cool down with slow walking for 2–3 minutes, then stretch your legs and lower back.</li>
              <li><b>Hydration:</b> Drink water before, during, and after physical activity. Dehydration can cause dizziness and fatigue.</li>
              <li><b>Footwear:</b> Wear comfortable, supportive shoes to prevent blisters and joint strain.</li>
            </ul>
          </div>
        </>
      )}
    </div>
  );
}

// ============================
// HELPER STYLES
// ============================
const thStyle = {
  padding: '12px 10px',
  textAlign: 'left',
  fontWeight: '800',
  fontSize: '0.8rem',
  whiteSpace: 'nowrap'
};

const tdStyle = {
  padding: '10px',
  borderBottom: '1px solid #eee',
  whiteSpace: 'nowrap'
};
