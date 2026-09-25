// Import the functions you need from the SDKs you need
import { initializeApp } from "firebase/app";
import { getAnalytics } from "firebase/analytics";
// TODO: Add SDKs for Firebase products that you want to use
// https://firebase.google.com/docs/web/setup#available-libraries

// Your web app's Firebase configuration
// For Firebase JS SDK v7.20.0 and later, measurementId is optional
const firebaseConfig = {
  apiKey: "AIzaSyAe9_AivobN6IMA8W4HzzMPv9ygkGGrJhw",
  authDomain: "sonara-5a3c4.firebaseapp.com",
  projectId: "sonara-5a3c4",
  storageBucket: "sonara-5a3c4.firebasestorage.app",
  messagingSenderId: "401815399746",
  appId: "1:401815399746:web:a254713b5e11089d5cecdb",
  measurementId: "G-BSZ239JJN4"
};

// Initialize Firebase
const app = initializeApp(firebaseConfig);
const analytics = getAnalytics(app);
