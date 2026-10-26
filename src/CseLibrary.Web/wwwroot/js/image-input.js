// Picture fields (_ImageInput.cshtml): preview, type/size check, and shrink to 800px in the browser before upload,
// which also drops phone-photo metadata such as GPS. The server checks type and size again on its own.
(() => {
    const MAX_SIDE = 800, MAX_BYTES = 2 * 1024 * 1024, TYPES = ["image/jpeg", "image/png", "image/webp"];

    async function shrink(file) {
        const bitmap = await createImageBitmap(file);
        const scale = Math.min(1, MAX_SIDE / Math.max(bitmap.width, bitmap.height));
        const canvas = document.createElement("canvas");
        canvas.width = Math.round(bitmap.width * scale);
        canvas.height = Math.round(bitmap.height * scale);
        canvas.getContext("2d").drawImage(bitmap, 0, 0, canvas.width, canvas.height);
        const type = file.type === "image/png" ? "image/png" : "image/jpeg"; // PNG keeps transparency
        const blob = await new Promise(resolve => canvas.toBlob(resolve, type, 0.85));
        return new File([blob], "picture" + (type === "image/png" ? ".png" : ".jpg"), { type });
    }

    document.querySelectorAll("[data-image-input]").forEach(box => {
        const input = box.querySelector("input[type=file]");
        const img = box.querySelector("[data-preview]");
        const placeholder = box.querySelector("[data-placeholder]");
        const remove = box.querySelector("[data-remove]");
        const flag = box.querySelector("[data-remove-flag]");
        const error = box.querySelector("[data-image-error]");
        const show = url => { img.src = url || ""; img.hidden = !url; placeholder.hidden = !!url; remove.hidden = !url; };
        const fail = message => { error.textContent = message; error.hidden = !message; input.value = ""; };

        input.addEventListener("change", async () => {
            error.hidden = true;
            const file = input.files[0];
            if (!file) return;
            if (!TYPES.includes(file.type)) return fail("Choose a JPG, PNG or WebP picture.");
            try {
                const small = await shrink(file);
                if (small.size > MAX_BYTES) return fail("The picture is larger than 2 MB.");
                const files = new DataTransfer();
                files.items.add(small);
                input.files = files.files;
                flag.value = "false";
                show(URL.createObjectURL(small));
            } catch {
                fail("This picture could not be read. Try another file.");
            }
        });

        // Clears the choice and, on save, removes the stored picture.
        remove.addEventListener("click", () => { input.value = ""; flag.value = "true"; error.hidden = true; show(null); });
    });
})();
