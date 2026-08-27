import { onValue, ref } from 'firebase/database';
import { Crown, Medal, Trophy } from 'lucide-react';
import { useEffect, useState } from 'react';
import { db } from '../firebaseConfig';

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

  const getName = (user) => {
    if (user?.username) return user.username;
    return user?.email ? user.email.split('@')[0] : 'Anonymous';
  };

  const getInitial = (user) => {
    return getName(user).charAt(0).toUpperCase();
  };

  return (
    <div style={{
      maxWidth: '850px',
      margin: '2rem auto 0',
      paddingBottom: '2rem'
    }}>
      <div style={{
        background: '#fff4d6',
        border: '5px solid #171717',
        borderRadius: '22px',
        padding: '2rem',
        boxShadow: '8px 8px 0 rgba(0, 0, 0, 0.35)'
      }}>
        {/* Orange title board */}
        <div style={{
          background: '#f59b35',
          color: '#ffffff',
          border: '5px solid #171717',
          borderRadius: '14px',
          padding: '1rem',
          maxWidth: '520px',
          margin: '0 auto 2.5rem',
          textAlign: 'center',
          boxShadow: '0 7px 0 #9b531e'
        }}>
          <h2 style={{
            margin: 0,
            fontFamily: '"Press Start 2P", cursive',
            fontSize: 'clamp(0.9rem, 2.5vw, 1.35rem)',
            textShadow: '3px 3px 0 #171717',
            color: '#ffffff'
          }}>
            <Trophy size={24} color="#ffd84d" style={{ verticalAlign: 'middle', marginRight: '10px' }} />
            GLOBAL LEADERBOARD
          </h2>
        </div>

        {/* No data */}
        {users.length === 0 && (
          <div style={{
            textAlign: 'center',
            background: '#fff9e9',
            border: '4px solid #171717',
            borderRadius: '16px',
            padding: '2rem'
          }}>
            <p style={{
              margin: 0,
              fontSize: '1.2rem',
              fontWeight: '700',
              color: '#1b2433'
            }}>
              No data available. Go walk!
            </p>
          </div>
        )}

        {/* Podium display */}
        {topThree.length > 0 && (
          <div style={{
            display: 'flex',
            justifyContent: 'center',
            alignItems: 'flex-end',
            gap: '18px',
            marginBottom: '2rem',
            flexWrap: 'wrap'
          }}>
            {/* Rank 2 */}
            {topThree.length >= 2 && (
              <div style={{
                display: 'flex',
                flexDirection: 'column',
                alignItems: 'center',
                minWidth: '160px'
              }}>
                <Medal color="#c0c0c0" size={34} style={{ marginBottom: '8px' }} />

                <div style={{
                  width: '84px',
                  height: '84px',
                  borderRadius: '12px',
                  border: '4px solid #171717',
                  background: '#c0c0c0',
                  display: 'flex',
                  alignItems: 'center',
                  justifyContent: 'center',
                  fontSize: '2rem',
                  fontWeight: '900',
                  color: '#171717',
                  boxShadow: '5px 5px 0 rgba(0,0,0,0.3)'
                }}>
                  {getInitial(topThree[1])}
                </div>

                <strong style={{
                  marginTop: '10px',
                  color: '#1b2433',
                  fontSize: '1rem',
                  textAlign: 'center',
                  wordBreak: 'break-word'
                }}>
                  {getName(topThree[1])}
                </strong>

                <span style={{
                  color: '#1b2433',
                  fontWeight: '700'
                }}>
                  {topThree[1].TotalLifetimeSteps.toLocaleString()}
                </span>

                <div style={{
                  width: '130px',
                  height: '80px',
                  background: '#c0c0c0',
                  border: '4px solid #171717',
                  borderBottom: 'none',
                  marginTop: '10px',
                  display: 'flex',
                  alignItems: 'center',
                  justifyContent: 'center',
                  fontWeight: '900',
                  fontSize: '1.4rem',
                  color: '#171717'
                }}>
                  #2
                </div>
              </div>
            )}

            {/* Rank 1 */}
            <div style={{
              display: 'flex',
              flexDirection: 'column',
              alignItems: 'center',
              minWidth: '180px'
            }}>
              <Crown color="#ffd84d" size={48} style={{ marginBottom: '8px' }} />

              <div style={{
                width: '105px',
                height: '105px',
                borderRadius: '14px',
                border: '5px solid #171717',
                background: '#ffd84d',
                display: 'flex',
                alignItems: 'center',
                justifyContent: 'center',
                fontSize: '2.5rem',
                fontWeight: '900',
                color: '#171717',
                boxShadow: '6px 6px 0 rgba(0,0,0,0.35)'
              }}>
                {getInitial(topThree[0])}
              </div>

              <strong style={{
                marginTop: '10px',
                color: '#9b531e',
                fontSize: '1.15rem',
                textAlign: 'center',
                wordBreak: 'break-word'
              }}>
                {getName(topThree[0])}
              </strong>

              <span style={{
                color: '#1b2433',
                fontWeight: '800'
              }}>
                {topThree[0].TotalLifetimeSteps.toLocaleString()}
              </span>

              <div style={{
                width: '150px',
                height: '120px',
                background: '#ffd84d',
                border: '5px solid #171717',
                borderBottom: 'none',
                marginTop: '10px',
                display: 'flex',
                alignItems: 'center',
                justifyContent: 'center',
                fontWeight: '900',
                fontSize: '1.8rem',
                color: '#171717'
              }}>
                #1
              </div>
            </div>

            {/* Rank 3 */}
            {topThree.length >= 3 && (
              <div style={{
                display: 'flex',
                flexDirection: 'column',
                alignItems: 'center',
                minWidth: '160px'
              }}>
                <Medal color="#cd7f32" size={34} style={{ marginBottom: '8px' }} />

                <div style={{
                  width: '84px',
                  height: '84px',
                  borderRadius: '12px',
                  border: '4px solid #171717',
                  background: '#cd7f32',
                  display: 'flex',
                  alignItems: 'center',
                  justifyContent: 'center',
                  fontSize: '2rem',
                  fontWeight: '900',
                  color: '#171717',
                  boxShadow: '5px 5px 0 rgba(0,0,0,0.3)'
                }}>
                  {getInitial(topThree[2])}
                </div>

                <strong style={{
                  marginTop: '10px',
                  color: '#1b2433',
                  fontSize: '1rem',
                  textAlign: 'center',
                  wordBreak: 'break-word'
                }}>
                  {getName(topThree[2])}
                </strong>

                <span style={{
                  color: '#1b2433',
                  fontWeight: '700'
                }}>
                  {topThree[2].TotalLifetimeSteps.toLocaleString()}
                </span>

                <div style={{
                  width: '130px',
                  height: '65px',
                  background: '#cd7f32',
                  border: '4px solid #171717',
                  borderBottom: 'none',
                  marginTop: '10px',
                  display: 'flex',
                  alignItems: 'center',
                  justifyContent: 'center',
                  fontWeight: '900',
                  fontSize: '1.4rem',
                  color: '#171717'
                }}>
                  #3
                </div>
              </div>
            )}
          </div>
        )}

        {/* Rest of players */}
        {rest.length > 0 && (
          <div style={{
            background: '#fff9e9',
            border: '4px solid #171717',
            borderRadius: '16px',
            overflow: 'hidden'
          }}>
            {rest.map((user, index) => (
              <div
                key={user.id}
                style={{
                  display: 'grid',
                  gridTemplateColumns: '70px 1fr 150px',
                  gap: '10px',
                  alignItems: 'center',
                  padding: '14px 18px',
                  borderBottom: index === rest.length - 1 ? 'none' : '3px solid #171717',
                  color: '#1b2433',
                  fontWeight: '700'
                }}
              >
                <span>#{index + 4}</span>
                <span>{getName(user)}</span>
                <span style={{ textAlign: 'right' }}>
                  {user.TotalLifetimeSteps.toLocaleString()}
                </span>
              </div>
            ))}
          </div>
        )}
      </div>
    </div>
  );
}