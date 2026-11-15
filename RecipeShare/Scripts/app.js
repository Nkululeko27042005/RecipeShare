/* ------------------------------------------------------------------ */
/*  RecipE frontend helpers                                            */
/* ------------------------------------------------------------------ */
const RE = {
    csrf: () => {
        const tokenInput = document.querySelector('input[name="__RequestVerificationToken"]');
        return tokenInput ? tokenInput.value : '';
    },

    async post(url, data = {}) {
        const body = new FormData();
        const csrfToken = RE.csrf();
        if (csrfToken) {
            body.append('__RequestVerificationToken', csrfToken);
        }
        Object.entries(data).forEach(([k, v]) => {
            if (v !== undefined && v !== null) {
                body.append(k, v);
            }
        });
        const res = await fetch(url, { method: 'POST', body, credentials: 'same-origin' });
        return res.json();
    },

    toast(msg, type = 'success') {
        const stack = document.getElementById('toastStack');
        if (!stack) return;
        const el = document.createElement('div');
        const colors = type === 'error'
            ? 'bg-ember text-cream'
            : 'bg-moss text-cream';
        el.className = `${colors} border-2 border-ink rounded-lg px-4 py-3 font-body text-sm shadow-book-sm fade-up`;
        el.textContent = msg;
        stack.appendChild(el);
        setTimeout(() => { el.style.opacity = '0'; el.style.transition = 'opacity .3s'; }, 2600);
        setTimeout(() => el.remove(), 3000);
    },

    timeAgo(dateStr) {
        if (!dateStr) return '';
        const d = new Date(dateStr);
        const s = Math.floor((Date.now() - d.getTime()) / 1000);
        if (isNaN(s)) return '';
        if (s < 60) return 'just now';
        if (s < 3600) return `${Math.floor(s / 60)}m ago`;
        if (s < 86400) return `${Math.floor(s / 3600)}h ago`;
        if (s < 604800) return `${Math.floor(s / 86400)}d ago`;
        return d.toLocaleDateString();
    }
};

/* User dropdown toggle */
document.addEventListener('click', e => {
    const wrap = e.target.closest('[data-user-menu]');
    const panel = document.querySelector('[data-user-menu-panel]');
    if (!panel) return;
    if (wrap) {
        panel.classList.toggle('hidden');
    } else {
        panel.classList.add('hidden');
    }
});
