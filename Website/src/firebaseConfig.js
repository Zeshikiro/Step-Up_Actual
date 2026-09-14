// firebaseConfig.js — Central Firebase initialization
// This file connects the website to our Firebase project (step-up-72811)
// Both the Unity app and this website share the same Firebase backend

import { initializeApp } from "firebase/app";
import { getAuth, setPersistence, browserLocalPersistence } from "firebase/auth";
import { getDatabase } from "firebase/database";

// Firebase project credentials — matches the Unity app's GoogleService config
const firebaseConfig = {
  apiKey: "AIzaSyCKhxDsFH6rV0pwsBytRDrciu-cI8tpyI8",
  authDomain: "step-up-72811.firebaseapp.com",
  databaseURL: "https://step-up-72811-default-rtdb.firebaseio.com/",
  projectId: "step-up-72811",
  storageBucket: "step-up-72811.appspot.com",
  messagingSenderId: "86765820412",
  appId: "1:86765820412:android:583b1fefe7bc1a9ed73a79"
};

const app = initializeApp(firebaseConfig);

// Auth — handles login, register, email verification
export const auth = getAuth(app);

// Keep users logged in even after closing the browser tab
setPersistence(auth, browserLocalPersistence).catch((error) => {
  console.error("Auth persistence error:", error);
});

// Realtime Database — reads/writes user data, posts, leaderboard, admin info
export const db = getDatabase(app);
