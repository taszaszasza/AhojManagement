import "./index.css";
import {LoginView} from "./views/LoginView.tsx";
import {UserManagementView} from "./views/UserManagementView.tsx";
import {useState} from "react";
import {clearAuthToken, getAuthToken} from "@/apiClient.ts";
import type {UserResponseDto} from "@/api.ts";
import {Toaster} from "react-hot-toast";

export function App() {

    const [token, setToken] = useState<string | null>(() => getAuthToken());
    const [currentUser, setCurrentUser] = useState<UserResponseDto | null>(() => {
        const raw = sessionStorage.getItem("user");
        return raw ? JSON.parse(raw) : null;
    });

    const handleLoginSuccess = () => {
        setToken(getAuthToken());
        const raw = sessionStorage.getItem("user");
        setCurrentUser(raw ? JSON.parse(raw) : null);
    };

    const handleLogout = () => {
        clearAuthToken();
        setToken(null);
        setCurrentUser(null);
    };
    
    
  return (
      
      <div className="App">
          <Toaster position="top-right" />
          {!token || !currentUser ? (
              <LoginView onLoginSuccess={handleLoginSuccess} />
          ) : (
              <UserManagementView currentUser={currentUser} onLogout={handleLogout} />
          )}
    </div>
  );
}

export default App;
