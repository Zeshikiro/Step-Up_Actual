import { initializeApp } from "firebase/app";
import { getAuth } from "firebase/auth";
import { getDatabase } from "firebase/database";

// TODO: Replace these with your actual Firebase Web config keys!
// You can find these in the Firebase Console -> Project Settings -> Web App
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
export const auth = getAuth(app);
export const db = getDatabase(app);
