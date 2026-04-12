
document.addEventListener('DOMContentLoaded', function () {
    const toggleBtn = document.getElementById('toggleBtn');
    const sidebar = document.getElementById('sidebar');
    const mainContent = document.getElementById('mainContent');

    if (toggleBtn && sidebar) {
        toggleBtn.addEventListener('click', function () {
            sidebar.classList.toggle('collapsed');
            mainContent.classList.toggle('expanded');
        });
    }

    const submenuParents = document.querySelectorAll('.has-submenu');

    submenuParents.forEach(parent => {
        const link = parent.querySelector('.nav-link');

        link.addEventListener('click', function (e) {
            e.preventDefault();

            submenuParents.forEach(otherParent => {
                if (otherParent !== parent) {
                    otherParent.classList.remove('active');
                    otherParent.querySelector('.submenu').classList.remove('active');
                }
            });

            parent.classList.toggle('active');
            const submenu = parent.querySelector('.submenu');
            submenu.classList.toggle('active');
        });
    });

    document.addEventListener('click', function (e) {
        if (!e.target.closest('.has-submenu')) {
            submenuParents.forEach(parent => {
                parent.classList.remove('active');
                parent.querySelector('.submenu').classList.remove('active');
            });
        }
    });

    function handleResize() {
        if (window.innerWidth <= 768) {
            sidebar.classList.add('collapsed');
            mainContent.classList.add('expanded');
        } else {
            sidebar.classList.remove('collapsed');
            mainContent.classList.remove('expanded');
        }
    }

    handleResize();
    window.addEventListener('resize', handleResize);

    const navLinks = document.querySelectorAll('.nav-link:not(.has-submenu > .nav-link)');

    navLinks.forEach(link => {
        link.addEventListener('click', function (e) {
            if (!this.parentElement.classList.contains('has-submenu')) {
                e.preventDefault();
                const pageTitle = document.getElementById('pageTitle');
                if (pageTitle) {
                    pageTitle.textContent = this.querySelector('.nav-text').textContent;
                }
            }
        });
    });
});