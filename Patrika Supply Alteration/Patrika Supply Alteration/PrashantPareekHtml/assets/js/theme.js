/* ═══════════════════════════════════════════
   FLOATING THEME FAB — draggable + toggle
═══════════════════════════════════════════ */
(function() {
  const FAB_KEY = 'dcr-theme-fab-pos';
  const THEME_KEY = 'dcr-theme-mode';

  function applyTheme(dark) {
    document.body.classList.toggle('dark-mode', dark);
    const fab = document.getElementById('theme-fab');
    if (fab) fab.textContent = dark ? '☀️' : '🌙';
  }

  function initFAB() {
    const fab = document.getElementById('theme-fab');
    if (!fab) return;

    // Restore saved theme
    const savedTheme = localStorage.getItem(THEME_KEY);
    applyTheme(savedTheme === 'dark');

    // Restore position
    const savedPos = JSON.parse(localStorage.getItem(FAB_KEY) || 'null');
    if (savedPos) {
      fab.style.bottom = 'auto';
      fab.style.right  = 'auto';
      fab.style.top    = savedPos.top  + 'px';
      fab.style.left   = savedPos.left + 'px';
    }

    let dragging = false, startX, startY, origLeft, origTop, moved;

    function getPos() {
      const r = fab.getBoundingClientRect();
      return { top: r.top, left: r.left };
    }

    function onDown(e) {
      dragging = true; moved = false;
      const p = e.touches ? e.touches[0] : e;
      startX = p.clientX; startY = p.clientY;
      const pos = getPos();
      origLeft = pos.left; origTop = pos.top;
      fab.style.bottom = 'auto'; fab.style.right = 'auto';
      fab.style.top  = origTop  + 'px';
      fab.style.left = origLeft + 'px';
      fab.style.transition = 'none';
      e.preventDefault();
    }

    function onMove(e) {
      if (!dragging) return;
      const p = e.touches ? e.touches[0] : e;
      const dx = p.clientX - startX, dy = p.clientY - startY;
      if (Math.abs(dx) > 4 || Math.abs(dy) > 4) moved = true;
      let newLeft = origLeft + dx;
      let newTop  = origTop  + dy;
      // Clamp within viewport
      newLeft = Math.max(8, Math.min(window.innerWidth  - fab.offsetWidth  - 8, newLeft));
      newTop  = Math.max(8, Math.min(window.innerHeight - fab.offsetHeight - 8, newTop));
      fab.style.left = newLeft + 'px';
      fab.style.top  = newTop  + 'px';
      e.preventDefault();
    }

    function onUp(e) {
      if (!dragging) return;
      dragging = false;
      fab.style.transition = '';
      // Save position
      localStorage.setItem(FAB_KEY, JSON.stringify({ top: parseFloat(fab.style.top), left: parseFloat(fab.style.left) }));
      if (!moved) {
        // It was a tap — toggle theme
        const isDark = document.body.classList.contains('dark-mode');
        applyTheme(!isDark);
        localStorage.setItem(THEME_KEY, isDark ? 'light' : 'dark');
      }
    }

    fab.addEventListener('mousedown',  onDown, { passive: false });
    fab.addEventListener('touchstart', onDown, { passive: false });
    window.addEventListener('mousemove',  onMove, { passive: false });
    window.addEventListener('touchmove',  onMove, { passive: false });
    window.addEventListener('mouseup',    onUp);
    window.addEventListener('touchend',   onUp);
  }

  if (document.readyState === 'loading') {
    document.addEventListener('DOMContentLoaded', initFAB);
  } else {
    initFAB();
  }
})();
