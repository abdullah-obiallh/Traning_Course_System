export function formatDate(value) {
    if (!value) {
        return "-";
    }

    return new Date(value).toLocaleDateString();
}

export function formatPercent(value) {
    return `${Number(value || 0).toFixed(2).replace(/\.00$/, "")}%`;
}