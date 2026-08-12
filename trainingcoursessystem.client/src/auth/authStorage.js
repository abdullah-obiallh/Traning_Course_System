const USER_KEY = "training_user";

export function saveUser(user) {
    localStorage.setItem(USER_KEY, JSON.stringify(user));
}

export function getUser() {
    const userJson = localStorage.getItem(USER_KEY);

    if (!userJson) {
        return null;
    }

    try {
        return JSON.parse(userJson);
    } catch {
        localStorage.removeItem(USER_KEY);
        return null;
    }
}

export function logoutUser() {
    localStorage.removeItem(USER_KEY);
}

export function isLoggedIn() {
    return getUser() !== null;
}