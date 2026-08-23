import React, { createContext, useContext, useEffect, useState } from 'react';
import { auth, db } from '../firebaseConfig';
import { onAuthStateChanged, signInWithEmailAndPassword, createUserWithEmailAndPassword, signOut, sendPasswordResetEmail, sendEmailVerification, updateEmail, EmailAuthProvider, reauthenticateWithCredential } from 'firebase/auth';
import { ref, set, update } from 'firebase/database';

const AuthContext = createContext();

export function useAuth() {
  return useContext(AuthContext);
}

export function AuthProvider({ children }) {
  const [currentUser, setCurrentUser] = useState(null);
  const [loading, setLoading] = useState(true);

  function login(email, password) {
    return signInWithEmailAndPassword(auth, email, password);
  }

  async function register(email, password) {
    const userCredential = await createUserWithEmailAndPassword(auth, email, password);
    const user = userCredential.user;
    
    // NEW: Send verification email!
    await sendEmailVerification(user);
    
    await set(ref(db, 'users/' + user.uid), {
      email: email,
      TotalLifetimeSteps: 0,
      currentDailySteps: 0
    });

    return userCredential;
  }
  
  async function changeEmail(currentPassword, newEmail) {
    if (!currentUser) throw new Error("No user logged in");
    
    const credential = EmailAuthProvider.credential(currentUser.email, currentPassword);
    
    // Re-authenticate user before allowing sensitive operation
    await reauthenticateWithCredential(currentUser, credential);
    
    // Update email in Auth
    await updateEmail(currentUser, newEmail);
    
    // Send verification to the new email
    await sendEmailVerification(currentUser);
    
    // Update email in Realtime Database
    await update(ref(db, 'users/' + currentUser.uid), {
      email: newEmail
    });
  }

  function logout() {
    return signOut(auth);
  }

  function resetPassword(email) {
    return sendPasswordResetEmail(auth, email);
  }

  useEffect(() => {
    const unsubscribe = onAuthStateChanged(auth, user => {
      setCurrentUser(user);
      setLoading(false);
    });

    return unsubscribe;
  }, []);

  const value = {
    currentUser,
    login,
    register,
    logout,
    resetPassword,
    changeEmail
  };

  return (
    <AuthContext.Provider value={value}>
      {!loading && children}
    </AuthContext.Provider>
  );
}