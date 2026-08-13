import { initializeApp } from "firebase/app";
import { getAuth, setPersistence, browserLocalPersistence } from "firebase/auth";
import { getDatabase } from "firebase/database";

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
// Force local persistence so users stay logged in across sessions
setPersistence(auth, browserLocalPersistence).catch((error) => {
  console.error("Auth persistence error:", error);
});
export const db = getDatabase(app);
