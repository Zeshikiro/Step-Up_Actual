import React from 'react';
import { Shield, Lock, Eye, CheckCircle } from 'lucide-react';

export default function PrivacyPolicy() {
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
        textAlign: 'left',
        boxShadow: '8px 8px 0 rgba(0, 0, 0, 0.35)'
      }}>

        <div style={{
          background: '#f59b35',
          color: '#ffffff',
          border: '5px solid #171717',
          borderRadius: '14px',
          padding: '1rem',
          maxWidth: '500px',
          margin: '0 auto 2rem',
          textAlign: 'center',
          boxShadow: '0 7px 0 #9b531e'
        }}>
          <h2 style={{
            margin: 0,
            fontFamily: '\"Press Start 2P\", cursive',
            fontSize: 'clamp(0.85rem, 2.4vw, 1.2rem)',
            textShadow: '3px 3px 0 #171717',
            color: '#ffffff'
          }}>
            EULA & PRIVACY
          </h2>
        </div>

        <p style={{ fontWeight: 'bold', color: '#171717', lineHeight: '1.6', marginBottom: '2rem', textAlign: 'center' }}>
          Please read this agreement before using STEP-UP. By creating an account, signing in, or tapping “AGREE,” you confirm that you have read, understood, and accepted these terms.
        </p>

        <h3 style={{ color: '#9b531e', fontSize: '1.3rem', borderBottom: '3px dashed #171717', paddingBottom: '0.5rem' }}>
          1. Purpose of the Application
        </h3>
        <p style={{ fontWeight: '500', color: '#333', lineHeight: '1.6', marginBottom: '1.5rem' }}>
          STEP-UP is a capstone thesis project developed for academic and research purposes. The application is designed to encourage physical activity through step tracking, GPS-based walking features, gamification, avatar customization, leaderboard ranking, health tips, and community interaction. STEP-UP is not a commercial medical application and is not a substitute for professional medical advice, diagnosis, or treatment.
        </p>

        <h3 style={{ color: '#9b531e', fontSize: '1.3rem', borderBottom: '3px dashed #171717', paddingBottom: '0.5rem' }}>
          2. Academic and Research Use
        </h3>
        <p style={{ fontWeight: '500', color: '#333', lineHeight: '1.6', marginBottom: '1.5rem' }}>
          This application is used only for the evaluation and completion of the STEP-UP capstone study. Data gathered from the application may be used by the researchers for system testing, usability evaluation, research analysis, and academic documentation. The results may be included in the final research paper, presentation, or defense, but personal identities will not be publicly disclosed in the research output.
        </p>

        <h3 style={{ color: '#9b531e', fontSize: '1.3rem', borderBottom: '3px dashed #171717', paddingBottom: '0.5rem' }}>
          3. Data Collected (Privacy Policy)
        </h3>
        <p style={{ fontWeight: '500', color: '#333', lineHeight: '1.6', marginBottom: '1.5rem' }}>
          To make the application work properly, STEP-UP may collect and process the following data:<br/>
          • Email address or account login information<br/>
          • Step count and walking progress<br/>
          • Total lifetime steps and daily steps<br/>
          • GPS/location data used for map tracking<br/>
          • Avatar, points, rewards, leaderboard progress, and app activity<br/>
          • Community feed posts or messages made inside the app<br/>
          • Evaluation responses or feedback related to the capstone study<br/><br/>
          The application will only collect data needed for the features and evaluation of the STEP-UP study. User data will not be sold, rented, or shared with advertisers. Data may be stored using Firebase and other tools needed to operate the application. Only authorized researchers/developers may access the data for testing, maintenance, and research documentation.
        </p>

        <h3 style={{ color: '#9b531e', fontSize: '1.3rem', borderBottom: '3px dashed #171717', paddingBottom: '0.5rem' }}>
          4. GPS and Step Tracking Safety
        </h3>
        <p style={{ fontWeight: '500', color: '#333', lineHeight: '1.6', marginBottom: '1.5rem' }}>
          STEP-UP uses your device’s pedometer and GPS/location services to track physical movement and map walking activity. Location access is needed for map and route-related features. Users must remain aware of their surroundings while walking. Do not use the application in unsafe areas, private property, restricted locations, roadways, or places where phone use may cause accidents.
        </p>

        <h3 style={{ color: '#9b531e', fontSize: '1.3rem', borderBottom: '3px dashed #171717', paddingBottom: '0.5rem' }}>
          5. User Responsibilities
        </h3>
        <p style={{ fontWeight: '500', color: '#333', lineHeight: '1.6', marginBottom: '1.5rem' }}>
          By using STEP-UP, you agree to provide accurate account information, use the app only for its intended academic and fitness-related purpose, avoid posting harmful or inappropriate content, avoid cheating or manipulating step counts, use the app safely while walking, and respect other users.
        </p>

        <h3 style={{ color: '#9b531e', fontSize: '1.3rem', borderBottom: '3px dashed #171717', paddingBottom: '0.5rem' }}>
          6. Health and Safety Disclaimer
        </h3>
        <p style={{ fontWeight: '500', color: '#333', lineHeight: '1.6', marginBottom: '1.5rem' }}>
          STEP-UP provides general fitness tips and movement guidance for educational purposes only. It does not provide medical advice. If you have a medical condition, injury, dizziness, pain, shortness of breath, or any health concern, stop using the app and consult a qualified healthcare professional. The researchers and developers are not responsible for injuries, accidents, or health problems caused by unsafe use of the application.
        </p>
        
        <div style={{ marginTop: '3rem', textAlign: 'center' }}>
            <p style={{ fontWeight: '900', color: '#171717', fontSize: '1.1rem' }}>
                <CheckCircle size={20} style={{ verticalAlign: 'middle', marginRight: '5px', color: '#3fd66b' }}/> 
                Participation in the STEP-UP evaluation is voluntary. Users may stop using the application at any time.
            </p>
        </div>

      </div>
    </div>
  );
}
