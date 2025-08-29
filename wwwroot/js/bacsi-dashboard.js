// Google Translate
function googleTranslateElementInit() {
    new google.translate.TranslateElement({
        pageLanguage: 'vi',
        includedLanguages: 'vi,en,fr,es,de,ja',
        layout: google.translate.TranslateElement.InlineLayout.SIMPLE
    }, 'google_translate_element');
}

// Toggle Filters
function toggleFilters() {
    const content = document.getElementById('filtersContent');
    const toggle = document.getElementById('filtersToggle');
    if (!content || !toggle) return;
    const icon = toggle.querySelector('i');
    
    if (content.style.display === 'none' || content.style.display === '') {
        content.style.display = 'block';
        if (icon) icon.className = 'fas fa-chevron-up';
    } else {
        content.style.display = 'none';
        if (icon) icon.className = 'fas fa-chevron-down';
    }
}

// Chart Group Controls
function showChartGroup(group) {
    // Update button states
    document.querySelectorAll('.chart-btn').forEach(btn => btn.classList.remove('active'));
    if (event && event.target) event.target.classList.add('active');
    
    // Show/hide chart groups
    const charts = document.querySelectorAll('.chart-group');
    charts.forEach(chart => {
        if (group === 'all' || chart.classList.contains(group)) {
            chart.style.display = 'block';
        } else {
            chart.style.display = 'none';
        }
    });
}

// Toggle Data Table
function toggleDataTable() {
    const container = document.getElementById('dataTableContainer');
    const text = document.getElementById('tableToggleText');
    
    if (!container || !text) return;

    if (container.style.display === 'none' || container.style.display === '') {
        container.style.display = 'block';
        text.textContent = 'Ẩn dữ liệu';
    } else {
        container.style.display = 'none';
        text.textContent = 'Xem dữ liệu';
    }
}

// Reset Filters
function resetFilters() {
    // Reset all dropdowns to first option (empty value)
    const selects = document.querySelectorAll('.filter-select');
    selects.forEach(select => {
        select.selectedIndex = 0;
    });
}

// Helpers to build query from current filters
function getSelectedValue(selectName) {
    const el = document.querySelector(`select[name="${selectName}"]`);
    if (!el) return '';
    return el.value || '';
}

function appendParam(params, key, value) {
    if (value && value.trim() !== '') {
        params.append(key, value.trim());
    }
}

// Load All Data (Export all with current filters)
function loadAllData() {
    const params = new URLSearchParams();
    // Collect current single-select filters
    appendParam(params, 'code', getSelectedValue('code'));
    appendParam(params, 'projectName', getSelectedValue('projectName'));
    appendParam(params, 'year', getSelectedValue('year'));
    appendParam(params, 'city', getSelectedValue('city'));
    appendParam(params, 'job', getSelectedValue('job'));
    appendParam(params, 'chuyenKhoa', getSelectedValue('chuyenKhoa'));

    // Navigate to ExportToExcel with current filters
    const url = `/Bacsi/ExportToExcel?${params.toString()}`;
    window.location.href = url;
}

// Initialize Charts when page loads
document.addEventListener('DOMContentLoaded', function() {
    // Start with filters collapsed (CSS already hides; ensure state icon correct)
    const content = document.getElementById('filtersContent');
    const toggle = document.getElementById('filtersToggle');
    if (content && toggle) {
        content.style.display = 'none';
        const icon = toggle.querySelector('i');
        if (icon) icon.className = 'fas fa-chevron-down';
    }

    initializeCharts();
});

function initializeCharts() {
    // Age Distribution Chart
    const ageCtx = document.getElementById('ageChart');
    if (ageCtx) {
        new Chart(ageCtx.getContext('2d'), {
            type: 'bar',
            data: {
                labels: window.ageLabels || [],
                datasets: [{
                    label: 'Số lượng mẫu',
                    data: window.ageData || [],
                    backgroundColor: 'rgba(52, 152, 219, 0.8)',
                    borderColor: 'rgba(52, 152, 219, 1)',
                    borderWidth: 2,
                    borderRadius: 8
                }]
            },
            options: {
                responsive: true,
                maintainAspectRatio: false,
                plugins: {
                    legend: { display: false },
                    tooltip: {
                        backgroundColor: 'rgba(0,0,0,0.8)',
                        titleColor: 'white',
                        bodyColor: 'white'
                    }
                },
                scales: {
                    y: {
                        beginAtZero: true,
                        grid: { color: 'rgba(0,0,0,0.1)' }
                    },
                    x: {
                        grid: { display: false }
                    }
                }
            }
        });
    }

    // Gender Distribution Chart (doughnut like Manhinhchinh)
    const genderCtx = document.getElementById('genderChart');
    if (genderCtx) {
        new Chart(genderCtx.getContext('2d'), {
            type: 'doughnut',
            data: {
                labels: (window.genderLabels && window.genderLabels.length) ? window.genderLabels : ["Nam", "Nữ", "Không xác định"],
                datasets: [{
                    data: (window.genderData && window.genderData.length) ? window.genderData : [0, 0, 0],
                    backgroundColor: [
                        'rgba(54, 162, 235, 0.8)',
                        'rgba(255, 99, 132, 0.8)',
                        'rgba(255, 206, 86, 0.8)'
                    ],
                    borderWidth: 3,
                    borderColor: 'white'
                }]
            },
            options: {
                responsive: true,
                maintainAspectRatio: false,
                plugins: {
                    legend: { position: 'bottom' },
                    tooltip: {
                        backgroundColor: 'rgba(0,0,0,0.8)',
                        titleColor: 'white',
                        bodyColor: 'white'
                    }
                }
            }
        });
    }



    // Marital Status Chart
    const maritalCtx = document.getElementById('maritalChart');
    if (maritalCtx) {
        new Chart(maritalCtx.getContext('2d'), {
            type: 'bar',
            data: {
                labels: window.maritalLabels || [],
                datasets: [{
                    label: 'Số lượng mẫu',
                    data: window.maritalData || [],
                    backgroundColor: 'rgba(155, 89, 182, 0.8)',
                    borderColor: 'rgba(155, 89, 182, 1)',
                    borderWidth: 2,
                    borderRadius: 8
                }]
            },
            options: {
                responsive: true,
                maintainAspectRatio: false,
                plugins: {
                    legend: { display: false },
                    tooltip: {
                        backgroundColor: 'rgba(0,0,0,0.8)',
                        titleColor: 'white',
                        bodyColor: 'white'
                    }
                },
                scales: {
                    y: {
                        beginAtZero: true,
                        grid: { color: 'rgba(0,0,0,0.1)' }
                    },
                    x: {
                        grid: { display: false }
                    }
                }
            }
        });
    }

    // Income Chart
    const incomeCtx = document.getElementById('incomeChart');
    if (incomeCtx) {
        new Chart(incomeCtx.getContext('2d'), {
            type: 'line',
            data: {
                labels: window.incomeLabels || [],
                datasets: [{
                    label: 'Số lượng mẫu',
                    data: window.incomeData || [],
                    borderColor: 'rgba(46, 204, 113, 1)',
                    backgroundColor: 'rgba(46, 204, 113, 0.1)',
                    borderWidth: 3,
                    fill: true,
                    tension: 0.4
                }]
            },
            options: {
                responsive: true,
                maintainAspectRatio: false,
                plugins: {
                    legend: { display: false },
                    tooltip: {
                        backgroundColor: 'rgba(0,0,0,0.8)',
                        titleColor: 'white',
                        bodyColor: 'white'
                    }
                },
                scales: {
                    y: {
                        beginAtZero: true,
                        grid: { color: 'rgba(0,0,0,0.1)' }
                    },
                    x: {
                        grid: { display: false }
                    }
                }
            }
        });
    }

    // City Chart
    const cityCtx = document.getElementById('cityChart');
    if (cityCtx) {
        new Chart(cityCtx.getContext('2d'), {
            type: 'bar',
            data: {
                labels: window.cityLabels || [],
                datasets: [{
                    label: 'Số lượng mẫu',
                    data: window.cityData || [],
                    backgroundColor: 'rgba(241, 196, 15, 0.8)',
                    borderColor: 'rgba(241, 196, 15, 1)',
                    borderWidth: 2,
                    borderRadius: 8
                }]
            },
            options: {
                responsive: true,
                maintainAspectRatio: false,
                plugins: {
                    legend: { display: false },
                    tooltip: {
                        backgroundColor: 'rgba(0,0,0,0.8)',
                        titleColor: 'white',
                        bodyColor: 'white'
                    }
                },
                scales: {
                    y: {
                        beginAtZero: true,
                        grid: { color: 'rgba(0,0,0,0.1)' }
                    },
                    x: {
                        grid: { display: false }
                    }
                }
            }
        });
    }

    // (Removed) District Chart

    // Region Chart
    const regionCtx = document.getElementById('regionChart');
    if (regionCtx) {
        new Chart(regionCtx.getContext('2d'), {
            type: 'pie',
            data: {
                labels: window.regionLabels || [],
                datasets: [{
                    data: window.regionData || [],
                    backgroundColor: [
                        'rgba(231, 76, 60, 0.8)',
                        'rgba(52, 152, 219, 0.8)',
                        'rgba(46, 204, 113, 0.8)'
                    ],
                    borderWidth: 3,
                    borderColor: 'white'
                }]
            },
            options: {
                responsive: true,
                maintainAspectRatio: false,
                plugins: {
                    legend: { position: 'bottom' },
                    tooltip: {
                        backgroundColor: 'rgba(0,0,0,0.8)',
                        titleColor: 'white',
                        bodyColor: 'white'
                    }
                }
            }
        });
    }

    // Project Chart
    const projectCtx = document.getElementById('projectChart');
    if (projectCtx) {
        new Chart(projectCtx.getContext('2d'), {
            type: 'bar',
            data: {
                labels: window.projectLabels || [],
                datasets: [{
                    label: 'Số lượng mẫu',
                    data: window.projectData || [],
                    backgroundColor: 'rgba(52, 73, 94, 0.8)',
                    borderColor: 'rgba(52, 73, 94, 1)',
                    borderWidth: 2,
                    borderRadius: 8
                }]
            },
            options: {
                responsive: true,
                maintainAspectRatio: false,
                plugins: {
                    legend: { display: false },
                    tooltip: {
                        backgroundColor: 'rgba(0,0,0,0.8)',
                        titleColor: 'white',
                        bodyColor: 'white'
                    }
                },
                scales: {
                    y: {
                        beginAtZero: true,
                        grid: { color: 'rgba(0,0,0,0.1)' }
                    },
                    x: {
                        grid: { display: false }
                    }
                }
            }
        });
    }

    // Year Chart
    const yearCtx = document.getElementById('yearChart');
    if (yearCtx) {
        new Chart(yearCtx.getContext('2d'), {
            type: 'line',
            data: {
                labels: window.yearLabels || [],
                datasets: [{
                    label: 'Số lượng mẫu',
                    data: window.yearData || [],
                    borderColor: 'rgba(155, 89, 182, 1)',
                    backgroundColor: 'rgba(155, 89, 182, 0.1)',
                    borderWidth: 3,
                    fill: true,
                    tension: 0.4
                }]
            },
            options: {
                responsive: true,
                maintainAspectRatio: false,
                plugins: {
                    legend: { display: false },
                    tooltip: {
                        backgroundColor: 'rgba(0,0,0,0.8)',
                        titleColor: 'white',
                        bodyColor: 'white'
                    }
                },
                scales: {
                    y: {
                        beginAtZero: true,
                        grid: { color: 'rgba(0,0,0,0.1)' }
                    },
                    x: {
                        grid: { display: false }
                    }
                }
            }
        });
    }

    // Specialty (Chuyên khoa) Chart - REMOVED DUPLICATE INITIALIZATION
}

// Utility functions
function showLoading() {
    const loading = document.querySelector('.loading');
    if (loading) loading.style.display = 'block';
}

function hideLoading() {
    const loading = document.querySelector('.loading');
    if (loading) loading.style.display = 'none';
}

// Export functions
function exportToExcel() {
    // Mirror the same behavior as loadAllData for consistency
    loadAllData();
}

// Search and filter functions
function performSearch() {
    showLoading();
    // Submit filters form
    const form = document.querySelector('.filters-form');
    if (form) form.submit();
    hideLoading();
}

// Initialize tooltips and other UI elements
document.addEventListener('DOMContentLoaded', function() {
    // Add any additional initialization here
    console.log('Bacsi Dashboard initialized');
});
