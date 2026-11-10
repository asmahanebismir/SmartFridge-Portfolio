import { initializeApp } from "firebase/app";
import { getAuth } from "firebase/auth";

const firebaseConfig = {
  apiKey: "AIzaSyAkj8mb1ynkN5KbtuQUBsNQN_jjSswuVRI",
  authDomain: "smartfridge-13227.firebaseapp.com",
  projectId: "smartfridge-13227",
  storageBucket: "smartfridge-13227.firebasestorage.app",
  messagingSenderId: "1050220698604",
  appId: "1:1050220698604:web:9a384ee59e893b18853ec9",
};

const app = initializeApp(firebaseConfig);
export const auth = getAuth(app);