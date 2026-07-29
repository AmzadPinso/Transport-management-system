/**
 * TMS PRO — Dashboard Interactivity
 * Covers: theme management, sidebar toggle, sidebar state persistence
 * (scroll position + collapsed state + active item visibility)
 */

(function () {
    'use strict';

    /* ─────────────────────────────────────────────────────────────
     * CONSTANTS — storage keys
     * ───────────────────────────────────────────────────────────── */
    var KEYS = {
        theme:          'tms_theme',
        sidebarScroll:  'tms_sidebar_scroll',
        sidebarCollapsed: 'tms_sidebar_collapsed'
    };

    /* ─────────────────────────────────────────────────────────────
     * HELPERS
     * ───────────────────────────────────────────────────────────── */
    function ss(key, value) {
        try { sessionStorage.setItem(key, value); } catch (e) {}
    }
    function sg(key) {
        try { return sessionStorage.getItem(key); } catch (e) { return null; }
    }
    function ls(key, value) {
        try { localStorage.setItem(key, value); } catch (e) {}
    }
    function lg(key) {
        try { return localStorage.getItem(key); } catch (e) { return null; }
    }

    /* ─────────────────────────────────────────────────────────────
     * PHASE 1 — INLINE BEFORE DOM READY
     * Apply theme & collapsed class immediately to avoid flash.
     * ───────────────────────────────────────────────────────────── */
    var savedTheme = lg(KEYS.theme) || 'light';
    if (savedTheme === 'dark') {
        document.documentElement.setAttribute('data-theme', 'dark');
    }

    // Apply collapsed sidebar state before first paint to prevent layout flash
    if (lg(KEYS.sidebarCollapsed) === 'true') {
        // We can't use querySelector yet (DOM not ready), so inject a <style>
        // that targets the sidebar directly via its class selector
        var style = document.createElement('style');
        style.id = 'tms-collapse-init';
        style.textContent = '.sidebar { width: var(--sidebar-collapsed-width) !important; } .main-content { margin-left: var(--sidebar-collapsed-width) !important; }';
        document.head.appendChild(style);
    }

    /* ─────────────────────────────────────────────────────────────
     * PHASE 2 — DOM CONTENT LOADED
     * ───────────────────────────────────────────────────────────── */
    document.addEventListener('DOMContentLoaded', function () {

        var sidebar      = document.querySelector('.sidebar');
        var sidebarNav   = document.querySelector('.sidebar-nav');   // the scrollable element
        var mainContent  = document.querySelector('.main-content');
        var sidebarToggle = document.getElementById('sidebar-toggle');
        var themeToggle  = document.getElementById('theme-toggle');

        /* ── 2.1  Theme ─────────────────────────────────────────── */
        if (savedTheme === 'dark') {
            _updateThemeIcon('dark');
        }

        if (themeToggle) {
            themeToggle.addEventListener('click', function () {
                var isDark = document.documentElement.getAttribute('data-theme') === 'dark';
                var next = isDark ? 'light' : 'dark';
                document.documentElement.setAttribute('data-theme', next);
                ls(KEYS.theme, next);
                _updateThemeIcon(next);
            });
        }

        function _updateThemeIcon(theme) {
            var icon = themeToggle && themeToggle.querySelector('i');
            if (icon) {
                icon.className = theme === 'dark'
                    ? 'fa-solid fa-sun'
                    : 'fa-solid fa-moon';
            }
        }

        /* ── 2.2  Sidebar collapsed state ───────────────────────── */
        // Phase 1 already prevented layout flash via injected <style>.
        // Now apply the class properly and remove the temporary style tag.
        if (lg(KEYS.sidebarCollapsed) === 'true') {
            sidebar && sidebar.classList.add('collapsed');
            mainContent && mainContent.classList.add('collapsed');
        }
        // Remove the temporary no-flash style (classes now take over)
        var initStyle = document.getElementById('tms-collapse-init');
        if (initStyle) { initStyle.parentNode.removeChild(initStyle); }

        if (sidebarToggle) {
            sidebarToggle.addEventListener('click', function () {
                if (!sidebar) return;

                sidebar.classList.toggle('collapsed');
                mainContent && mainContent.classList.toggle('collapsed');

                // Mobile: also toggle the overlay 'show' class
                if (window.innerWidth <= 991) {
                    sidebar.classList.toggle('show');
                }

                // Persist new state
                ls(KEYS.sidebarCollapsed, sidebar.classList.contains('collapsed') ? 'true' : 'false');
            });
        }

        // Close sidebar on mobile when clicking outside
        document.addEventListener('click', function (e) {
            if (window.innerWidth <= 991 &&
                sidebar && sidebar.classList.contains('show') &&
                !sidebar.contains(e.target) &&
                sidebarToggle && !sidebarToggle.contains(e.target)) {
                sidebar.classList.remove('show');
            }
        });

        /* ── 2.3  Sidebar scroll persistence ───────────────────────
         *
         * ROOT CAUSE: every MVC full-page navigation resets the DOM,
         * so .sidebar-nav scrollTop goes back to 0.
         *
         * FIX:
         *   • On beforeunload  → save sidebarNav.scrollTop to sessionStorage
         *   • On DOMContentLoaded → restore it immediately, then if the
         *     active item is still out of view, nudge scroll to show it.
         *
         * We use requestAnimationFrame so the scroll happens after the
         * browser has committed layout — no visible jump.
         * ───────────────────────────────────────────────────────── */
        if (sidebarNav) {

            // --- Restore saved scroll ---
            var savedScroll = parseInt(sg(KEYS.sidebarScroll) || '0', 10);

            // Apply immediately (synchronous) before first paint
            if (savedScroll > 0) {
                sidebarNav.scrollTop = savedScroll;
            }

            // Double-ensure after layout pass (handles any CSS that might
            // affect height after DOMContentLoaded)
            if (savedScroll > 0) {
                requestAnimationFrame(function () {
                    sidebarNav.scrollTop = savedScroll;
                });
            }

            // --- Ensure active item is visible ---
            // If user navigated directly via URL or the saved scroll wasn't
            // enough, scroll the active item into view smoothly.
            var activeLink = sidebarNav.querySelector('.sidebar-link.active');
            if (activeLink) {
                requestAnimationFrame(function () {
                    var itemTop   = activeLink.offsetTop;  // relative to sidebarNav top
                    var navHeight = sidebarNav.clientHeight;
                    var scrollTop = sidebarNav.scrollTop;

                    var itemBottom = itemTop + activeLink.offsetHeight;

                    // Only auto-scroll if item is outside the visible area
                    // and the user hasn't already scrolled to that region
                    var isAboveView = itemTop < scrollTop;
                    var isBelowView = itemBottom > (scrollTop + navHeight);

                    if (isAboveView || isBelowView) {
                        // Scroll so the active item sits near the middle of the panel
                        var targetScroll = itemTop - (navHeight / 2) + (activeLink.offsetHeight / 2);
                        sidebarNav.scrollTop = Math.max(0, targetScroll);
                    }
                });
            }

            // --- Save scroll before leaving page ---
            window.addEventListener('beforeunload', function () {
                ss(KEYS.sidebarScroll, sidebarNav.scrollTop);
            });

            // Also save on every scroll (throttled) so a crash/close
            // mid-session still captures the last position
            var _scrollTimer = null;
            sidebarNav.addEventListener('scroll', function () {
                if (_scrollTimer) return;
                _scrollTimer = setTimeout(function () {
                    ss(KEYS.sidebarScroll, sidebarNav.scrollTop);
                    _scrollTimer = null;
                }, 150);
            }, { passive: true });
        }

        /* ── 2.4  Lucide icons (optional) ──────────────────────── */
        if (typeof lucide !== 'undefined') {
            lucide.createIcons();
        }

    }); // end DOMContentLoaded

})(); // end IIFE
