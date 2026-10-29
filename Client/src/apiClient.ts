import {Api} from "./api.ts";

export const api = new Api<string>({
    baseUrl: `${window.location.protocol}//${window.location.hostname}:8001`,
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