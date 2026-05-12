/**
 * Dashboard interactivity and theme management
 */

document.addEventListener('DOMContentLoaded', () => {
    // --- Theme Management ---
    const themeToggle = document.getElementById('theme-toggle');
    const htmlElement = document.documentElement;
    const currentTheme = localStorage.getItem('theme') || 'light';

    // Set initial theme
    if (currentTheme === 'dark') {
        htmlElement.setAttribute('data-theme', 'dark');
        updateThemeIcon('dark');
    }

    themeToggle?.addEventListener('click', () => {
        const isDark = htmlElement.getAttribute('data-theme') === 'dark';
        const newTheme = isDark ? 'light' : 'dark';
        
        htmlElement.setAttribute('data-theme', newTheme);
        localStorage.setItem('theme', newTheme);
        updateThemeIcon(newTheme);
    });

    function updateThemeIcon(theme) {
        const icon = themeToggle?.querySelector('i');
        if (icon) {
            icon.className = theme === 'dark' ? 'fa-solid fa-sun' : 'fa-solid fa-moon';
        }
    }

    // --- Sidebar Toggle ---
    const sidebarToggle = document.getElementById('sidebar-toggle');
    const sidebar = document.querySelector('.sidebar');
    const mainContent = document.querySelector('.main-content');

    sidebarToggle?.addEventListener('click', () => {
        sidebar?.classList.toggle('collapsed');
        mainContent?.classList.toggle('collapsed');
        
        // On mobile, use 'show' class instead
        if (window.innerWidth <= 991) {
            sidebar?.classList.toggle('show');
        }
    });

    // Close sidebar when clicking outside on mobile
    document.addEventListener('click', (e) => {
        if (window.innerWidth <= 991 && 
            sidebar?.classList.contains('show') && 
            !sidebar.contains(e.target) && 
            !sidebarToggle.contains(e.target)) {
            sidebar.classList.remove('show');
        }
    });

    // --- Lucide Icons Init ---
    if (typeof lucide !== 'undefined') {
        lucide.createIcons();
    }
});
