const API_BASE_URL = "";

export async function apiGet(url) {
    const response = await fetch(API_BASE_URL + url);

    if (!response.ok) {
        const message = await response.text();
        throw new Error(message || "Failed to load data");
    }

    return await response.json();
}

export async function apiPost(url, data) {
    const response = await fetch(API_BASE_URL + url, {
        method: "POST",
        headers: {
            "Content-Type": "application/json"
        },
        body: JSON.stringify(data)
    });

    if (!response.ok) {
        const message = await response.text();
        throw new Error(message || "Request failed");
    }

    return await response.json();
}

export async function apiPut(url, data) {
    const response = await fetch(API_BASE_URL + url, {
        method: "PUT",
        headers: {
            "Content-Type": "application/json"
        },
        body: JSON.stringify(data)
    });

    if (!response.ok) {
        const message = await response.text();
        throw new Error(message || "Update failed");
    }

    return await response.json();
}

export async function apiDelete(url) {
    const response = await fetch(API_BASE_URL + url, {
        method: "DELETE"
    });

    if (!response.ok) {
        const message = await response.text();
        throw new Error(message || "Delete failed");
    }

    return await response.text();
}