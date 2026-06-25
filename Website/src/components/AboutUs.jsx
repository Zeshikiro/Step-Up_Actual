import { Info, User, Users } from 'lucide-react';

export default function AboutUs() {
  const members = [
    {
      name: 'John Ryan',
      role: 'Lead Game Developer',
      img: '/john.jpg',
      desc: 'Spearheaded the Unity engine integrations, Mapbox geolocation features, and Firebase backend architecture for the core Step-Up experience.'
    },
    {
      name: 'Raven Ashley Jose',
      role: 'Project Leader',
      img: '/raven.jpg',
      desc: 'Leads the Capstone project, manages overall team direction, and drives the research and writing of the official thesis paper.'
    },
    {
      name: 'Kristina Nunag',
      role: 'Documentation & Research',
      img: '/kristina.jpg',
      desc: 'Specializes in project documentation, academic research, and collaborating on the comprehensive thesis paper for the Capstone requirements.'
    }
  ];

  return (
    <div style={{
      width: 'min(950px, calc(100vw - 40px))',
      margin: '2rem auto 0',
      paddingBottom: '2rem'
    }}>

      {/* ABOUT PANEL */}
      <div style={{
        background: '#fff4d6',
        border: '5px solid #171717',
        borderRadius: '22px',
        padding: '2.5rem',
        boxShadow: '8px 8px 0 rgba(0, 0, 0, 0.35)',
        marginBottom: '2rem'
      }}>

        {/* Orange title board */}
        <div style={{
          background: '#f59b35',
          color: '#ffffff',
          border: '5px solid #171717',
          borderRadius: '14px',
          padding: '1rem',
          maxWidth: '470px',
          margin: '0 auto 2rem',
          textAlign: 'center',
          boxShadow: '0 7px 0 #9b531e'
        }}>
          <h2 style={{
            margin: 0,
            fontFamily: '"Press Start 2P", cursive',
            fontSize: 'clamp(0.85rem, 2.4vw, 1.25rem)',
            textShadow: '3px 3px 0 #171717',
            color: '#ffffff'
          }}>
            <Info size={23} color="#ffffff" style={{ verticalAlign: 'middle', marginRight: '10px' }} />
            ABOUT STEP-UP
          </h2>
        </div>

        <div style={{
          background: '#fff9e9',
          border: '4px solid #171717',
          borderRadius: '16px',
          padding: '1.5rem',
          boxShadow: '5px 5px 0 rgba(0,0,0,0.25)'
        }}>
          <p style={{
            marginTop: 0,
            lineHeight: '1.7',
            color: '#1b2433',
            fontSize: '1.05rem',
            fontWeight: '600'
          }}>
            <strong>Step-Up</strong> is a Capstone Thesis Project designed to make physical activity more engaging for students. 
            By combining native pedometer tracking, Mapbox GPS integration, gamification, and avatar customization, the system turns walking into a more rewarding and interactive experience.
          </p>

          <p style={{
            marginBottom: 0,
            lineHeight: '1.7',
            color: '#1b2433',
            fontSize: '1.05rem',
            fontWeight: '600'
          }}>
            The goal of STEP-UP is to encourage healthier daily habits by helping users track their steps, earn points, unlock avatar items, join leaderboards, and stay motivated through game-like features.
          </p>
        </div>
      </div>

      {/* TEAM PANEL */}
      <div style={{
        background: '#fff4d6',
        border: '5px solid #171717',
        borderRadius: '22px',
        padding: '2.5rem',
        boxShadow: '8px 8px 0 rgba(0, 0, 0, 0.35)'
      }}>

        {/* Orange title board */}
        <div style={{
          background: '#f59b35',
          color: '#ffffff',
          border: '5px solid #171717',
          borderRadius: '14px',
          padding: '1rem',
          maxWidth: '400px',
          margin: '0 auto 2rem',
          textAlign: 'center',
          boxShadow: '0 7px 0 #9b531e'
        }}>
          <h2 style={{
            margin: 0,
            fontFamily: '"Press Start 2P", cursive',
            fontSize: 'clamp(0.85rem, 2.4vw, 1.25rem)',
            textShadow: '3px 3px 0 #171717',
            color: '#ffffff'
          }}>
            <Users size={23} color="#ffffff" style={{ verticalAlign: 'middle', marginRight: '10px' }} />
            MEET THE TEAM
          </h2>
        </div>

        <div style={{
          display: 'grid',
          gridTemplateColumns: 'repeat(auto-fit, minmax(220px, 1fr))',
          gap: '20px'
        }}>
          {members.map((member, index) => (
            <div
              key={index}
              style={{
                background: '#fff9e9',
                border: '4px solid #171717',
                borderRadius: '18px',
                padding: '1.5rem',
                textAlign: 'center',
                boxShadow: '6px 6px 0 rgba(0,0,0,0.3)'
              }}
            >
              <div style={{
                width: '110px',
                height: '110px',
                borderRadius: '18px',
                margin: '0 auto',
                display: 'flex',
                alignItems: 'center',
                justifyContent: 'center',
                background: '#ffd84d',
                border: '5px solid #171717',
                overflow: 'hidden',
                boxShadow: '5px 5px 0 rgba(0,0,0,0.25)'
              }}>
                <img
                  src={member.img}
                  alt={member.name}
                  onError={(e) => {
                    e.target.style.display = 'none';
                    e.target.nextSibling.style.display = 'block';
                  }}
                  style={{
                    width: '100%',
                    height: '100%',
                    objectFit: 'cover'
                  }}
                />
                <div style={{ display: 'none' }}>
                  <User size={50} color="#171717" />
                </div>
              </div>

              <h3 style={{
                marginTop: '18px',
                marginBottom: '6px',
                color: '#9b531e',
                fontSize: '1.35rem',
                fontWeight: '900'
              }}>
                {member.name}
              </h3>

              <p style={{
                color: '#137333',
                fontSize: '1rem',
                fontWeight: '900',
                margin: '0 0 12px'
              }}>
                {member.role}
              </p>

              <p style={{
                fontSize: '0.95rem',
                color: '#1b2433',
                lineHeight: '1.6',
                margin: 0,
                fontWeight: '600'
              }}>
                {member.desc}
              </p>
            </div>
          ))}
        </div>
      </div>
    </div>
  );
}