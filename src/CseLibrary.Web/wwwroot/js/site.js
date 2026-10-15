// D-55: show the server-rendered toasts (_Toasts.cshtml); Bootstrap handles timers, hover/focus pause and close.
document.querySelectorAll('.toast').forEach(el => bootstrap.Toast.getOrCreateInstance(el).show());
