import { ArrowLeft, ImageOff } from 'lucide-react';
import { useState } from 'react';

export default function HealthTips() {
  const [activeTipId, setActiveTipId] = useState(() => {
    const params = new URLSearchParams(window.location.search);
    return params.get('tip');
  });

  const handleTipChange = (id) => {
    setActiveTipId(id);
    const url = new URL(window.location);
    if (id) {
      url.searchParams.set('tip', id);
    } else {
      url.searchParams.delete('tip');
    }
    window.history.pushState({}, '', url);
  };

  const tips = [
    {
      id: 'posture',
      title: ' Posture',
      desc: 'Maintain proper posture to prevent injuries and improve performance.',
      cards: [
        { 
          title: 'Keep Your Head Up', 
          img: '/Images_and_Videos/Posture/keepheadup.png', 
          body: 'Look forward, not down at your feet. Your gaze should be about 10–20 feet ahead to keep your neck and spine aligned. Poor head positioning increases stress on the cervical spine significantly.',
          source: 'Mayo Clinic – Posture: Align yourself for good health'
        },
        { 
          title: 'Relax Your Shoulders', 
          img: '/Images_and_Videos/Posture/relaxshoulders.png',
          body: 'Keep shoulders back and down, not hunched forward. The WHO notes that musculoskeletal conditions — many caused by poor posture — are the leading contributor to disability worldwide. Shoulder tension leads to fatigue and long-term neck pain.',
          source: 'WHO – Musculoskeletal Health (2023)'
        },
        { 
          title: 'Engage Your Core', 
          img: '/Images_and_Videos/Posture/engagecore.png',
          body: 'Lightly tighten your abdominal muscles while walking or running. According to Mayo Clinic, core strengthening reduces the risk of back injuries and improves overall body stability and posture.',
          source: 'Mayo Clinic – Core exercises: Why you should strengthen your core'
        },
        { 
          title: 'Proper Foot Strike', 
          img: '/Images_and_Videos/Posture/footstrike.png',
          body: 'Land mid-foot rather than on your heel or toes. The American College of Sports Medicine recommends a mid-foot strike to reduce impact forces on the knee and hip joints during walking and running.',
          source: 'American College of Sports Medicine – Running Biomechanics'
        },
        { 
          title: 'Swing Your Arms', 
          img: '/Images_and_Videos/Posture/armswing.png',
          body: 'Keep arms at a 90-degree angle and swing them forward and back, not across your body. Proper arm swing reduces energy expenditure and helps maintain balance during physical activity.',
          source: 'Mayo Clinic – Walking: Trim your waistline, improve your health'
        },
      ]
    },
    {
      id: 'warmup',
      title: ' Warm Up',
      desc: 'Prepare your body before intense activity to avoid injury.',
      cards: [
        { 
          title: 'March in Place', 
          img: '/Images_and_Videos/Warmup/marchinplace.png', 
          body: 'Start by marching in place for 1–2 minutes, lifting your knees high. The American Heart Association recommends light aerobic movement before exercise to gradually raise heart rate and increase blood flow to muscles.',
          source: 'American Heart Association – Warm Up, Cool Down (2024)'
        },
        { 
          title: 'Hip Rotations', 
          img: '/Images_and_Videos/Warmup/hiprotation.png',
          body: 'Stand with feet shoulder-width apart and rotate your hips in a circle, 10 times each direction. Dynamic movements like this loosen the hip joints and lower back, reducing injury risk before physical activity.',
          source: 'Mayo Clinic – Stretching: Focus on flexibility'
        },
        { 
          title: 'Leg Swings', 
          img: '/Images_and_Videos/Warmup/legswing.png',
          body: 'Hold onto a wall and swing one leg forward and back 10 times, then side to side. Dynamic stretching before exercise has been shown to improve muscle performance and reduce injury risk compared to static stretching.',
          source: 'American College of Sports Medicine – Quantity and Quality of Exercise (2011)'
        },
        { 
          title: 'Heel-to-Butt Kicks', 
          img: '/Images_and_Videos/Warmup/heeltobutt.png',
          body: 'Jog in place while kicking your heels up toward your glutes for 30 seconds. This activates the quadriceps and raises heart rate gradually, which the American Heart Association identifies as key components of an effective warm-up.',
          source: 'American Heart Association – Warm Up, Cool Down (2024)'
        },
        { 
          title: 'Arm Circles', 
          img: '/Images_and_Videos/Warmup/armcircle.png',
          body: 'Extend arms out to the sides and make small circles, gradually getting bigger — 10 forward and 10 backward. This loosens the shoulder joints and improves range of motion before upper body activity.',
          source: 'Mayo Clinic – Stretching: Focus on flexibility'
        },
      ]
    },
    {
      id: 'cooldown',
      title: ' Cooldown',
      desc: 'Slow down your heart rate and stretch after exercise to prevent soreness.',
      cards: [
        { 
          title: 'Walk It Out', 
          img: '/Images_and_Videos/Cooldown/walkitout.png',
          body: 'After intense activity, walk slowly for 5 minutes. The American Heart Association warns that stopping exercise abruptly can cause blood to pool in the legs, leading to dizziness. A gradual cooldown prevents this.',
          source: 'American Heart Association – Warm Up, Cool Down (2024)'
        },
        { 
          title: 'Quad Stretch', 
          img: '/Images_and_Videos/Cooldown/quadstretch.png',
          body: 'Stand on one leg, pull the other foot toward your glutes and hold for 20–30 seconds per side. Regular post-exercise stretching improves flexibility and reduces delayed onset muscle soreness (DOMS).',
          source: 'Mayo Clinic – Stretching: Focus on flexibility'
        },
        { 
          title: 'Calf Stretch', 
          img: '/Images_and_Videos/Cooldown/calfstretch.png',
          body: 'Place your hands on a wall, step one foot back and press the heel down. Hold for 20 seconds each side. The WHO recommends regular stretching as part of physical activity routines to maintain mobility and prevent musculoskeletal issues.',
          source: 'WHO – Physical Activity Guidelines (2020)'
        },
        { 
          title: 'Hamstring Stretch', 
          img: '/Images_and_Videos/Cooldown/hamstringstretch.png',
          body: 'Sit on the floor with legs straight, reach toward your toes and hold for 30 seconds. This relieves tension built up from walking or running and helps maintain lower body flexibility over time.',
          source: 'Mayo Clinic – Stretching: Focus on flexibility'
        },
        { 
          title: 'Deep Breathing', 
          img: '/Images_and_Videos/Cooldown/deepbreathing.png',
          body: 'Take slow, deep breaths — inhale for 4 counts, hold for 2, exhale for 6. The WHO recognizes breathing exercises as an effective method to activate the parasympathetic nervous system and speed up post-exercise recovery.',
          source: 'WHO – Mental Health and Physical Activity (2023)'
        },
      ]
    },
    {
      id: 'fitness',
      title: ' Fitness Tips',
      desc: 'Simple habits to improve your fitness and reach your step goals.',
      cards: [
        { 
          title: 'Stay Hydrated', 
          img: '/Images_and_Videos/Fitness Tips/stayhydrated.png',
          body: 'The WHO recommends drinking enough water throughout the day to maintain physical and cognitive performance. Dehydration of even 2% of body weight can impair physical performance and increase fatigue during exercise.',
          source: 'WHO – Nutrition for Health and Development'
        },
        { 
          title: 'Increase Steps Gradually', 
          img: '/Images_and_Videos/Fitness Tips/increasestep.png',
          body: 'The WHO recommends adults aged 18–64 do at least 150–300 minutes of moderate-intensity activity per week. Gradually increasing daily steps by 500–1000 per week is a safe and effective way to reach this target.',
          source: 'WHO – Physical Activity Guidelines (2020)'
        },
        { 
          title: 'Prioritize Sleep', 
          img: '/Images_and_Videos/Fitness Tips/prioritizesleep.png',
          body: 'The WHO highlights that quality sleep is essential for physical recovery and overall health. Adults should aim for 7–9 hours per night. Poor sleep impairs muscle recovery, reduces motivation, and increases injury risk.',
          source: 'WHO – Sleep and Health'
        },
        { 
          title: 'Eat for Energy', 
          img: '/Images_and_Videos/Fitness Tips/eatforenergy.png',
          body: 'The WHO recommends a diet rich in fruits, vegetables, legumes, whole grains, and lean protein. A balanced diet provides the energy needed for physical activity and supports muscle repair after exercise.',
          source: 'WHO – Healthy Diet Fact Sheet (2020)'
        },
        { 
          title: 'Stay Consistent', 
          img: '/Images_and_Videos/Fitness Tips/stayconsistent.png',
          body: 'The American Heart Association emphasizes that regular physical activity — even short daily walks — provides greater long-term health benefits than occasional intense exercise. Building a daily habit is the most effective fitness strategy.',
          source: 'American Heart Association – Physical Activity Recommendations (2024)'
        },
      ]
    },
  ];

  const activeTip = tips.find(t => t.id === activeTipId);

  return (
  <div style={{
    width: 'min(900px, calc(100vw - 40px))',
    margin: '2rem auto 0',
    paddingBottom: '2rem'
  }}>
    <div style={{
      background: '#fff4d6',
      border: '5px solid #171717',
      borderRadius: '22px',
      padding: '2.5rem',
      boxShadow: '8px 8px 0 rgba(0, 0, 0, 0.35)'
    }}>

      {activeTipId && (
        <button
          onClick={() => handleTipChange(null)}
          style={{
            background: '#3fd66b',
            color: '#082313',
            border: '4px solid #171717',
            borderRadius: '12px',
            padding: '10px 16px',
            cursor: 'pointer',
            fontWeight: '900',
            boxShadow: '0 5px 0 #137333',
            display: 'flex',
            alignItems: 'center',
            gap: '8px',
            marginBottom: '1.5rem'
          }}
        >
          <ArrowLeft size={18} />
          Back
        </button>
      )}

      {/* Orange title board */}
      <div style={{
        background: '#f59b35',
        color: '#ffffff',
        border: '5px solid #171717',
        borderRadius: '14px',
        padding: '1rem',
        maxWidth: '430px',
        margin: '0 auto 1.5rem',
        textAlign: 'center',
        boxShadow: '0 7px 0 #9b531e'
      }}>
        <h2 style={{
          margin: 0,
          fontFamily: '"Press Start 2P", cursive',
          fontSize: 'clamp(0.9rem, 2.5vw, 1.3rem)',
          textShadow: '3px 3px 0 #171717',
          color: '#ffffff'
        }}>
          {activeTipId ? activeTip.title.toUpperCase() : 'HEALTH HUB'}
        </h2>
      </div>

      {!activeTipId && (
        <p style={{
          textAlign: 'center',
          color: '#1b2433',
          fontWeight: '700',
          fontSize: '1.1rem',
          marginBottom: '2rem'
        }}>
          Select a category below to view tutorials and tips.
        </p>
      )}

      {!activeTipId ? (
        <div style={{
          display: 'flex',
          flexDirection: 'column',
          gap: '18px',
          maxWidth: '480px',
          margin: '0 auto'
        }}>
          {tips.map((tip) => (
            <button
              key={tip.id}
              onClick={() => handleTipChange(tip.id)}
              style={{
                background: '#fff9e9',
                color: '#1b2433',
                border: '4px solid #171717',
                padding: '18px 22px',
                fontSize: '1.25rem',
                fontWeight: '900',
                borderRadius: '14px',
                cursor: 'pointer',
                boxShadow: '5px 5px 0 rgba(0,0,0,0.28)',
                transition: 'transform 0.12s ease, box-shadow 0.12s ease',
                display: 'flex',
                alignItems: 'center',
                gap: '14px',
                textAlign: 'left'
              }}
              onMouseOver={(e) => {
                e.currentTarget.style.transform = 'translateY(-3px)';
                e.currentTarget.style.background = '#ffd84d';
              }}
              onMouseOut={(e) => {
                e.currentTarget.style.transform = 'translateY(0)';
                e.currentTarget.style.background = '#fff9e9';
              }}
            >
              <span style={{
                width: '42px',
                height: '42px',
                background: '#f59b35',
                border: '3px solid #171717',
                borderRadius: '10px',
                display: 'flex',
                alignItems: 'center',
                justifyContent: 'center',
                fontSize: '1.4rem'
              }}>
                {tip.id === 'posture' ? '🧍' : tip.id === 'warmup' ? '🏃' : tip.id === 'cooldown' ? '🧘' : '❤️'}
              </span>
              {tip.title}
            </button>
          ))}
        </div>
      ) : (
        <div>
          <p style={{
            textAlign: 'center',
            color: '#1b2433',
            fontWeight: '700',
            fontSize: '1.05rem',
            margin: '0 auto 2rem',
            maxWidth: '650px'
          }}>
            {activeTip.desc}
          </p>

          <div style={{
            display: 'flex',
            flexDirection: 'column',
            gap: '20px'
          }}>
            {activeTip.cards.map((card, idx) => (
              <div
                key={idx}
                style={{
                  background: '#fff9e9',
                  border: '5px solid #171717',
                  borderRadius: '18px',
                  overflow: 'hidden',
                  boxShadow: '6px 6px 0 rgba(0,0,0,0.3)'
                }}
              >
                {card.img ? (
                  <img
                    src={card.img}
                    alt={card.title}
                    onError={(e) => {
                      e.target.style.display = 'none';
                      e.target.nextSibling.style.display = 'flex';
                    }}
                    style={{
                      width: '100%',
                      maxHeight: '420px',
                      objectFit: 'contain',
                      display: 'block',
                      background: '#d9f6ff',
                      borderBottom: '4px solid #171717'
                    }}
                  />
                ) : null}

                <div style={{
                  display: card.img ? 'none' : 'flex',
                  width: '100%',
                  height: '220px',
                  background: '#d9f6ff',
                  borderBottom: '4px solid #171717',
                  flexDirection: 'column',
                  alignItems: 'center',
                  justifyContent: 'center',
                  gap: '8px',
                  color: '#1b2433',
                  fontWeight: '800'
                }}>
                  <ImageOff size={36} />
                  <span>No image yet</span>
                </div>

                <div style={{ padding: '1.5rem' }}>
                  <h4 style={{
                    margin: '0 0 12px 0',
                    fontSize: '1.35rem',
                    color: '#9b531e',
                    fontWeight: '900'
                  }}>
                    {card.title}
                  </h4>

                  <p style={{
                    margin: 0,
                    color: '#1b2433',
                    lineHeight: '1.65',
                    fontSize: '1.05rem'
                  }}>
                    {card.body}
                  </p>

                  <p style={{
                    margin: '14px 0 0 0',
                    fontSize: '0.85rem',
                    color: '#4b5563',
                    fontStyle: 'italic',
                    borderTop: '3px dashed #d6b977',
                    paddingTop: '10px',
                    fontWeight: '700'
                  }}>
                    📚 Source: {card.source}
                  </p>
                </div>
              </div>
            ))}
          </div>
        </div>
      )}
    </div>
  </div>
);
}
