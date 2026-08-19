import { useEffect, useState } from 'react';
import { useAuth } from './AuthContext';
import { Eye, EyeOff, LogIn, LogOut, UserPlus } from 'lucide-react';

export default function Login() {
  const { login, register, currentUser, logout, resetPassword } = useAuth();
  
    const getInitialMode = () => {
    const params = new URLSearchParams(window.location.search);
    return params.get('tab') === 'register';
  };

  const [isRegistering, setIsRegistering] = useState(getInitialMode);

  useEffect(() => {
    const handleUrlChange = () => {
      setIsRegistering(getInitialMode());
    };
    
    // Listen for custom popstate dispatched by handleTabChange in App.jsx
    window.addEventListener('popstate', handleUrlChange);
    return () => window.removeEventListener('popstate', handleUrlChange);
  }, []);

  const toggleMode = () => {
    const newMode = !isRegistering;
    setIsRegistering(newMode);
    
    const url = new URL(window.location);
    url.searchParams.set('tab', newMode ? 'register' : 'login');
    window.history.pushState({}, '', url);
  };
  const [email, setEmail] = useState("");
  const [password, setPassword] = useState("");
  const [error, setError] = useState("");
  const [loading, setLoading] = useState(false);
  const [showPassword, setShowPassword] = useState(false);
  const [resetSent, setResetSent] = useState(false);

  const handleSubmit = async (e) => {
    e.preventDefault();
    try {
      setError("");
      setLoading(true);
      if (isRegistering) {
        await register(email, password);
        setError("Registration successful! Please verify your email address (check your spam folder) before logging in!");
      } else {
        const userCred = await login(email, password);
        if (!userCred.user.emailVerified) {
          await logout();
          setError("Please verify your email address before logging in!");
        }
      }
    } catch (err) {
      console.error(err);
      
      let friendlyError = "An unexpected error occurred.";
      if (err.code === 'auth/invalid-credential' || err.code === 'auth/wrong-password' || err.code === 'auth/user-not-found') {
        friendlyError = "Incorrect email or password.";
      } else if (err.code === 'auth/email-already-in-use') {
        friendlyError = "An account with this email already exists.";
      } else if (err.code === 'auth/weak-password') {
        friendlyError = "Password should be at least 6 characters long.";
      } else if (err.code === 'auth/invalid-email') {
        friendlyError = "Please enter a valid email address.";
      } else {
        friendlyError = err.message; // Fallback if it's an unknown error
      }
      
      setError(isRegistering ? "Registration failed: " + friendlyError : "Sign in failed: " + friendlyError);
    }
    setLoading(false);
  };

  const handleForgotPassword = async () => {
    if (!email) {
      setError("Please enter your email first.");
      return;
    }
    try {
      await resetPassword(email);
      setResetSent(true);
      setError("");
    } catch (err) {
      setError("Failed to send reset email: " + err.message);
    }
  };

  return (
    <div style={{
      width: 'min(430px, calc(100vw - 40px))',
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

        <div style={{
          background: '#f59b35',
          color: '#ffffff',
          border: '5px solid #171717',
          borderRadius: '14px',
          padding: '1rem',
          margin: '0 auto 1.5rem',
          textAlign: 'center',
          boxShadow: '0 7px 0 #9b531e'
        }}>
          <h2 style={{
            margin: 0,
            fontFamily: '"Press Start 2P", cursive',
            fontSize: 'clamp(0.8rem, 2.4vw, 1.1rem)',
            textShadow: '3px 3px 0 #171717',
            color: '#ffffff',
            display: 'flex',
            alignItems: 'center',
            justifyContent: 'center',
            gap: '10px'
          }}>
            {isRegistering ? <><UserPlus size={22} /> REGISTER</> : <><LogIn size={22} /> SIGN IN</>}
          </h2>
        </div>

        {error && (
          <p style={{
            color: '#b91c1c',
            textAlign: 'center',
            marginTop: '1rem',
            fontWeight: '800',
            background: '#fee2e2',
            border: '3px solid #171717',
            borderRadius: '12px',
            padding: '10px'
          }}>
            {error}
          </p>
        )}

        <form onSubmit={handleSubmit} style={{
          display: 'flex',
          flexDirection: 'column',
          gap: '15px',
          marginTop: '1.5rem'
        }}>
          <input
            type="email"
            placeholder="Email"
            value={email}
            onChange={(e) => setEmail(e.target.value)}
            required
            style={{
              padding: '14px',
              borderRadius: '12px',
              border: '4px solid #171717',
              background: '#fff9e9',
              color: '#1b2433',
              fontSize: '1rem',
              fontWeight: '700'
            }}
          />

          <div style={{ position: 'relative' }}>
            <input
              type={showPassword ? 'text' : 'password'}
              placeholder="Password"
              value={password}
              onChange={(e) => setPassword(e.target.value)}
              required
              style={{
                padding: '14px',
                paddingRight: '45px',
                borderRadius: '12px',
                border: '4px solid #171717',
                background: '#fff9e9',
                color: '#1b2433',
                width: '100%',
                boxSizing: 'border-box',
                fontSize: '1rem',
                fontWeight: '700'
              }}
            />

            <span
              onClick={() => setShowPassword(!showPassword)}
              style={{
                position: 'absolute',
                right: '14px',
                top: '50%',
                transform: 'translateY(-50%)',
                cursor: 'pointer',
                color: '#171717'
              }}
            >
              {showPassword ? <EyeOff size={20} /> : <Eye size={20} />}
            </span>
          </div>

          {!isRegistering && (
            <p
              onClick={handleForgotPassword}
              style={{
                textAlign: 'right',
                fontSize: '0.9rem',
                color: '#0f6aa8',
                cursor: 'pointer',
                marginTop: '-6px',
                marginBottom: 0,
                fontWeight: '800'
              }}
            >
              {resetSent ? "✅ Reset email sent!" : "Forgot Password?"}
            </p>
          )}

          <button
            disabled={loading}
            type="submit"
            style={{
              marginTop: '10px',
              background: '#3fd66b',
              color: '#082313',
              border: '4px solid #171717',
              borderRadius: '12px',
              padding: '14px 18px',
              cursor: loading ? 'not-allowed' : 'pointer',
              fontWeight: '900',
              fontSize: '1.1rem',
              boxShadow: '0 6px 0 #137333'
            }}
          >
            {loading ? "Please wait..." : isRegistering ? "Register" : "Log In"}
          </button>
        </form>

        <p
          onClick={toggleMode}
          style={{
            textAlign: 'center',
            marginTop: '1.5rem',
            fontSize: '1rem',
            cursor: 'pointer',
            color: '#0f6aa8',
            fontWeight: '800'
          }}
        >
          {isRegistering ? "Already have an account? Sign In" : "Need an account? Register here"}
        </p>
      </div>
    </div>
  );
}

