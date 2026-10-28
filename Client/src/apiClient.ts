import {Api} from "./api.ts";

export const api = new Api<string>({
    baseUrl: "http://localhost:5124",
    securityWorker: (token) => {
        if (!token) {
            return {};
        }

        return {
            headers: {
                Authorization: `Bearer ${token}`,
            }
        }
    }
});

const storedToken = sessionStorage.getItem('token');

if (storedToken) {
    api.setSecurityData(storedToken);
}

export function setAuthToken(token: string) {
    sessionStorage.setItem('token', token);
    api.setSecurityData(token);
}

export function clearAuthToken() {
    sessionStorage.removeItem('token');
    sessionStorage.removeItem('user');
    api.setSecurityData(null);
}

export function getAuthToken(): string | null {
    return sessionStorage.getItem("token");
}