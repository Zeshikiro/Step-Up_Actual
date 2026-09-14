// AuthContext.jsx — Global authentication state manager
// Wraps the entire app so any component can access the current user via useAuth()
// Handles: login, register (with email verification), logout, password reset, email change

import React, { createContext, useContext, useEffect, useState } from 'react';
import { auth, db } from '../firebaseConfig';
import { onAuthStateChanged, signInWithEmailAndPassword, createUserWithEmailAndPassword, signOut, sendPasswordResetEmail, sendEmailVerification, updateEmail, EmailAuthProvider, reauthenticateWithCredential } from 'firebase/auth';
import { ref, set, update } from 'firebase/database';

const AuthContext = createContext();

// Hook — lets any component grab { currentUser, login, register, logout, ... }
export function useAuth() {
  return useContext(AuthContext);
}

export function AuthProvider({ children }) {
  const [currentUser, setCurrentUser] = useState(null);
  const [loading, setLoading] = useState(true); // prevents flash of wrong UI while Firebase checks session

  // Standard email+password login
  function login(email, password) {
    return signInWithEmailAndPassword(auth, email, password);
  }

  // Register — creates Firebase Auth user + sends verification email + writes initial DB record
  async function register(email, password) {
    const userCredential = await createUserWithEmailAndPassword(auth, email, password);
    const user = userCredential.user;
    
    // Force email verification before they can use the app
    await sendEmailVerification(user);
    
    // Create their database entry with zeroed-out stats (Unity app will populate the rest)
    await set(ref(db, 'users/' + user.uid), {
      email: email,
      TotalLifetimeSteps: 0,
      currentDailySteps: 0
    });

    return userCredential;
  }
  
  // Email change — requires re-authentication for security (Firebase enforces this)
  async function changeEmail(currentPassword, newEmail) {
    if (!currentUser) throw new Error("No user logged in");
    
    // Re-auth with current password first (Firebase requirement for sensitive ops)
    const credential = EmailAuthProvider.credential(currentUser.email, currentPassword);
    await reauthenticateWithCredential(currentUser, credential);
    
    await updateEmail(currentUser, newEmail);
    await sendEmailVerification(currentUser);
    
    // Sync the new email to the Realtime Database so Unity app sees it too
    await update(ref(db, 'users/' + currentUser.uid), {
      email: newEmail
    });
  }

  function logout() {
    return signOut(auth);
  }

  // Sends a password reset link to their email
  function resetPassword(email) {
    return sendPasswordResetEmail(auth, email);
  }

  // Listen for auth state changes (login, logout, page refresh)
  useEffect(() => {
    const unsubscribe = onAuthStateChanged(auth, user => {
      setCurrentUser(user);
      setLoading(false);
    });

    return unsubscribe;
  }, []);

  // Expose all auth functions to the rest of the app
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