import { type FormEvent, useState } from "react";
import { api, setAuthToken } from "@/apiClient.ts";
import toast from "react-hot-toast";

interface LoginViewProps {
    onLoginSuccess: () => void;
}

export function LoginView(props: LoginViewProps) {
    const [login, setLogin] = useState("");
    const [password, setPassword] = useState("");
    const [loading, setLoading] = useState(false);

    const handleSubmit = async (e: FormEvent) => {
        e.preventDefault();
        
        const cleanLogin = login.trim();
        if (!cleanLogin || !password) {
            toast.error("Login and password are required");
        }
        setLoading(true);

        try {
            const response = await api.users.usersLogin({ login, password });
            const data = response.data;

            if (data.token && data.user) {
                setAuthToken(data.token);
                sessionStorage.setItem("user", JSON.stringify(data.user));
                toast.success(`Welcome, ${data.user.firstName}!`);
                props.onLoginSuccess();
            }
        } catch (error: any) {
            const message =
                error?.error?.detail ||
                error?.error?.title ||
                error?.message ||
                "Login failed";
            toast.error(message);
        } finally {
            setLoading(false);
        }
    };

    return (
        <div className="auth-container">
            <h2>Sign In</h2>
    <form onSubmit={handleSubmit}>
    <div>
        <label htmlFor="login">Login: </label>
    <input
    id="login"
    type="text"
    value={login}
    onChange={(e) => setLogin(e.target.value)}
    required
    autoComplete="username"
    />
    </div>

    <div className="form-group">
    <label htmlFor="password">Password: </label>
    <input
    id="password"
    type="password"
    value={password}
    onChange={(e) => setPassword(e.target.value)}
    required
    autoComplete="current-password"
        />
        </div>

        <button type="submit" className="btn-primary btn-full" disabled={loading}>
        {loading ? "Signing in..." : "Log In"}
        </button>
        </form>
        </div>
);
}