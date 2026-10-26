// Author dialog (_AuthorDialog.cshtml): registers an author in place and shows names already registered,
// so "D. Knuth" is not added next to "Donald E. Knuth". Fires "author:picked" on the dialog with { id, name }
// when a new author is saved or an existing one is chosen. window.authorDialog.open(name) opens it prefilled.
(() => {
    const dlg = document.getElementById("authorDialog");
    if (!dlg) return;
    const form = dlg.querySelector("form");
    const input = form.elements.Name;
    const error = dlg.querySelector("[data-error]");
    const saved = dlg.querySelector("[data-saved]");
    const similar = dlg.querySelector("[data-similar]");
    const list = dlg.querySelector("[data-similar-list]");
    const pickMode = dlg.dataset.mode === "pick";
    const modal = bootstrap.Modal.getOrCreateInstance(dlg);
    const norm = s => s.trim().replace(/\s+/g, " ").toLowerCase();
    let timer, exact = null, created = 0;

    const say = (el, text) => { el.textContent = text; el.hidden = !text; };
    const clearPhoto = () => dlg.querySelector("[data-image-input] [data-remove]")?.click();
    const pick = author => { dlg.dispatchEvent(new CustomEvent("author:picked", { detail: author })); modal.hide(); };

    // Searches by the longest word of the name (usually the surname) to catch other spellings of it.
    async function findSimilar() {
        const name = input.value;
        const word = name.split(/[\s.,]+/).sort((a, b) => b.length - a.length)[0] || "";
        exact = null;
        input.classList.remove("is-invalid");
        say(error, "");
        if (word.length < 3) { similar.hidden = true; return; }

        const res = await fetch(`${dlg.dataset.lookup}?term=${encodeURIComponent(word)}`);
        if (!res.ok || input.value !== name) return; // a newer keystroke owns the result
        const { results } = await res.json();
        exact = results.find(r => norm(r.text) === norm(name)) || null;

        list.replaceChildren(...results.slice(0, 5).map(r => {
            const li = document.createElement("li");
            const label = document.createElement("span");
            label.innerHTML = '<i class="bi bi-person" aria-hidden="true"></i> ';
            label.append(r.text);
            const action = pickMode ? document.createElement("button") : document.createElement("a");
            action.className = "btn btn-sm btn-outline-primary";
            if (pickMode) { action.type = "button"; action.textContent = "Use this"; action.onclick = () => pick({ id: r.id, name: r.text }); }
            else { action.href = `${dlg.dataset.details}/${r.id}`; action.textContent = "View"; }
            li.append(label, action);
            return li;
        }));
        similar.hidden = results.length === 0;
        if (exact) { input.classList.add("is-invalid"); say(error, "This author is already registered."); }
    }

    input.addEventListener("input", () => { clearTimeout(timer); timer = setTimeout(findSimilar, 250); say(saved, ""); });

    form.addEventListener("submit", async e => {
        e.preventDefault();
        const again = e.submitter?.hasAttribute("data-again") ?? false;
        if (!input.value.trim()) { input.classList.add("is-invalid"); say(error, "Enter the author's name."); input.focus(); return; }
        if (exact) { input.focus(); return; }

        const buttons = form.querySelectorAll("button[type=submit]");
        buttons.forEach(b => b.disabled = true);
        try {
            const res = await fetch(form.action, { method: "POST", body: new FormData(form) });
            const body = await res.json().catch(() => ({ error: "Could not save. Reload the page and try again." }));
            if (!res.ok) { input.classList.add("is-invalid"); say(error, body.error); return; }
            created++;
            if (pickMode) { pick(body); return; }
            if (!again) { modal.hide(); return; }
            say(saved, `${body.name} registered. Add the next one.`);
            input.value = "";
            similar.hidden = true;
            clearPhoto();
            input.focus();
        } finally {
            buttons.forEach(b => b.disabled = false);
        }
    });

    dlg.addEventListener("shown.bs.modal", () => input.focus());
    dlg.addEventListener("hidden.bs.modal", () => {
        if (created && !pickMode) { location.reload(); return; } // the list shows the new rows
        form.reset();
        clearPhoto();
        input.classList.remove("is-invalid");
        say(error, ""); say(saved, "");
        similar.hidden = true;
        exact = null;
    });

    window.authorDialog = {
        open(name = "") {
            modal.show();
            input.value = name.trim();
            if (input.value) findSimilar();
        }
    };
})();
