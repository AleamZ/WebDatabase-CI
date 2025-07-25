const allSideMenu = document.querySelectorAll('#sidebar .side-menu.top li a');

allSideMenu.forEach(item => {
	const li = item.parentElement;

	item.addEventListener('click', function () {
		allSideMenu.forEach(i => {
			i.parentElement.classList.remove('active');
		})
		li.classList.add('active');
	})
});




// TOGGLE SIDEBAR
const menuBar = document.querySelector('#content nav .bx.bx-menu');
const sidebar = document.getElementById('sidebar');

if (menuBar) {
	menuBar.addEventListener('click', function () {
		sidebar.classList.toggle('hide');
	});
}







const searchButton = document.querySelector('#content nav form .form-input button');
const searchButtonIcon = document.querySelector('#content nav form .form-input button .bx');
const searchForm = document.querySelector('#content nav form');

if (searchButton && searchButtonIcon && searchForm) {
	searchButton.addEventListener('click', function (e) {
		if (window.innerWidth < 576) {
			e.preventDefault();
			searchForm.classList.toggle('show');
			if (searchForm.classList.contains('show')) {
				searchButtonIcon.classList.replace('bx-search', 'bx-x');
			} else {
				searchButtonIcon.classList.replace('bx-x', 'bx-search');
			}
		}
	});
}





if (window.innerWidth < 768) {
	if (sidebar) {
		sidebar.classList.add('hide');
	}
} else if (window.innerWidth > 576) {
	if (searchButtonIcon) {
		searchButtonIcon.classList.replace('bx-x', 'bx-search');
	}
	if (searchForm) {
		searchForm.classList.remove('show');
	}
}


window.addEventListener('resize', function () {
	if (this.innerWidth > 576) {
		if (searchButtonIcon) {
			searchButtonIcon.classList.replace('bx-x', 'bx-search');
		}
		if (searchForm) {
			searchForm.classList.remove('show');
		}
	}
})



const switchMode = document.getElementById('switch-mode');

if (switchMode) {
	switchMode.addEventListener('change', function () {
		if (this.checked) {
			document.body.classList.add('dark');
		} else {
			document.body.classList.remove('dark');
		}
	})
}

// Note: menu-toggle functionality is now handled in _Layout.cshtml to avoid conflicts
// This section is commented out to prevent duplicate event listeners

/* 
document.addEventListener('DOMContentLoaded', function () {
	var sidebar = document.getElementById('sidebar');
	var toggleBtn = document.getElementById('menu-toggle');

	if (toggleBtn && sidebar) {
		toggleBtn.addEventListener('click', function (e) {
			e.preventDefault();
			e.stopPropagation();

			sidebar.classList.toggle('hide');

			// Close all dropdowns when toggling sidebar
			document.querySelectorAll('.dropdown').forEach(function (item) {
				item.classList.remove('active');
			});

			// Force layout recalculation
			setTimeout(function () {
				window.dispatchEvent(new Event('resize'));
			}, 300);
		});
	}

	// Handle dropdown menu
	var dropdowns = document.querySelectorAll('.dropdown > a');
	dropdowns.forEach(function (dropdown) {
		dropdown.addEventListener('click', function (e) {
			e.preventDefault();
			var parent = this.parentElement;

			// If sidebar is collapsed, don't open dropdown
			if (sidebar.classList.contains('hide')) {
				return;
			}

			// Close all other dropdowns
			document.querySelectorAll('.dropdown').forEach(function (item) {
				if (item !== parent) {
					item.classList.remove('active');
				}
			});

			// Toggle current dropdown
			parent.classList.toggle('active');
		});
	});
});
*/