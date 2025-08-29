// Manhinhchinh Dashboard JavaScript
// Chart initialization and management

console.log('=== MANHINHCHINH DASHBOARD JS LOADED ===');
console.log('File loaded at:', new Date().toISOString());

function initializeCharts() {
    console.log('🔍 DEBUG - initializeCharts called');
    
    // Initialize all charts
    console.log('🔍 DEBUG - Initializing ageChart...');
    initializeAgeChart();
    
    console.log('🔍 DEBUG - Initializing genderChart...');
    initializeGenderChart();
    
    console.log('🔍 DEBUG - Initializing maritalChart...');
    initializeMaritalChart();
    
    console.log('🔍 DEBUG - Initializing incomeChart...');
    initializeIncomeChart();
    
    console.log('🔍 DEBUG - Initializing cityChart...');
    initializeCityChart();
    
    console.log('🔍 DEBUG - Initializing districtChart...');
    initializeDistrictChart();
    
    console.log('🔍 DEBUG - Initializing khuvucChart...');
    initializeKhuvucChart();
    
    console.log('🔍 DEBUG - Initializing projectChart...');
    initializeProjectChart();
    
    console.log('🔍 DEBUG - Initializing yearChart...');
    initializeYearChart();
    
    console.log('🔍 DEBUG - Initializing regionChart...');
    initializeRegionChart();
    
    // Show all chart groups by default
    showChartGroup('all');
}

function initializeAgeChart() {
    const ctx = document.getElementById('ageChart');
    if (!ctx || !window.ageLabels || !window.ageData) return;
    
    new Chart(ctx, {
        type: 'line',
        data: {
            labels: window.ageLabels,
            datasets: [{
                label: 'Số lượng mẫu',
                data: window.ageData,
                borderColor: '#3b82f6',
                backgroundColor: 'rgba(59, 130, 246, 0.1)',
                borderWidth: 3,
                fill: true,
                tension: 0.4
            }]
        },
        options: {
            responsive: true,
            maintainAspectRatio: false,
            plugins: {
                legend: {
                    display: false
                }
            },
            scales: {
                y: {
                    beginAtZero: true,
                    grid: {
                        color: 'rgba(0, 0, 0, 0.1)'
                    }
                },
                x: {
                    grid: {
                        color: 'rgba(0, 0, 0, 0.1)'
                    }
                }
            }
        }
    });
}

function initializeGenderChart() {
    const ctx = document.getElementById('genderChart');
    if (!ctx || !window.genderLabels || !window.genderData) return;
    
    new Chart(ctx, {
        type: 'doughnut',
        data: {
            labels: window.genderLabels,
            datasets: [{
                data: window.genderData,
                backgroundColor: [
                    '#3b82f6',
                    '#ef4444',
                    '#10b981'
                ],
                borderWidth: 2,
                borderColor: '#ffffff'
            }]
        },
        options: {
            responsive: true,
            maintainAspectRatio: false,
            plugins: {
                legend: {
                    position: 'bottom'
                }
            }
        }
    });
}

function initializeMaritalChart() {
    const ctx = document.getElementById('maritalChart');
    if (!ctx || !window.maritalLabels || !window.maritalData) return;
    
    new Chart(ctx, {
        type: 'pie',
        data: {
            labels: window.maritalLabels,
            datasets: [{
                data: window.maritalData,
                backgroundColor: [
                    '#8b5cf6',
                    '#06b6d4',
                    '#f59e0b',
                    '#ef4444'
                ],
                borderWidth: 2,
                borderColor: '#ffffff'
            }]
        },
        options: {
            responsive: true,
            maintainAspectRatio: false,
            plugins: {
                legend: {
                    position: 'bottom'
                }
            }
        }
    });
}

function initializeIncomeChart() {
    const ctx = document.getElementById('incomeChart');
    if (!ctx || !window.incomeLabels || !window.incomeData) return;
    
    new Chart(ctx, {
        type: 'bar',
        data: {
            labels: window.incomeLabels,
            datasets: [{
                label: 'Số lượng mẫu',
                data: window.incomeData,
                backgroundColor: '#10b981',
                borderColor: '#059669',
                borderWidth: 1
            }]
        },
        options: {
            responsive: true,
            maintainAspectRatio: false,
            plugins: {
                legend: {
                    display: false
                }
            },
            scales: {
                y: {
                    beginAtZero: true,
                    grid: {
                        color: 'rgba(0, 0, 0, 0.1)'
                    }
                },
                x: {
                    grid: {
                        color: 'rgba(0, 0, 0, 0.1)'
                    }
                }
            }
        }
    });
}

function initializeCityChart() {
    const ctx = document.getElementById('cityChart');
    if (!ctx || !window.cityLabels || !window.cityData) return;
    
    new Chart(ctx, {
        type: 'bar',
        data: {
            labels: window.cityLabels,
            datasets: [{
                label: 'Số lượng mẫu',
                data: window.cityData,
                backgroundColor: '#f59e0b',
                borderColor: '#d97706',
                borderWidth: 1
            }]
        },
        options: {
            responsive: true,
            maintainAspectRatio: false,
            plugins: {
                legend: {
                    display: false
                }
            },
            scales: {
                y: {
                    beginAtZero: true,
                    grid: {
                        color: 'rgba(0, 0, 0, 0.1)'
                    }
                },
                x: {
                    grid: {
                        color: 'rgba(0, 0, 0, 0.1)'
                    }
                }
            }
        }
    });
}

function initializeDistrictChart() {
    const ctx = document.getElementById('districtChart');
    if (!ctx || !window.districtLabels || !window.districtData) return;
    
    new Chart(ctx, {
        type: 'bar',
        data: {
            labels: window.districtLabels,
            datasets: [{
                label: 'Số lượng mẫu',
                data: window.districtData,
                backgroundColor: '#8b5cf6',
                borderColor: '#7c3aed',
                borderWidth: 1
            }]
        },
        options: {
            responsive: true,
            maintainAspectRatio: false,
            plugins: {
                legend: {
                    display: false
                }
            },
            scales: {
                y: {
                    beginAtZero: true,
                    grid: {
                        color: 'rgba(0, 0, 0, 0.1)'
                    }
                },
                x: {
                    grid: {
                        color: 'rgba(0, 0, 0, 0.1)'
                    }
                }
            }
        }
    });
}

function initializeKhuvucChart() {
    const ctx = document.getElementById('khuvucChart');
    if (!ctx || !window.khuvucLabels || !window.khuvucData) return;
    
    new Chart(ctx, {
        type: 'doughnut',
        data: {
            labels: window.khuvucLabels,
            datasets: [{
                data: window.khuvucData,
                backgroundColor: [
                    '#06b6d4',
                    '#f59e0b',
                    '#ef4444',
                    '#8b5cf6'
                ],
                borderWidth: 2,
                borderColor: '#ffffff'
            }]
        },
        options: {
            responsive: true,
            maintainAspectRatio: false,
            plugins: {
                legend: {
                    position: 'bottom'
                }
            }
        }
    });
}

function initializeProjectChart() {
    const ctx = document.getElementById('projectChart');
    if (!ctx || !window.projectLabels || !window.projectData) return;
    
    new Chart(ctx, {
        type: 'pie',
        data: {
            labels: window.projectLabels,
            datasets: [{
                data: window.projectData,
                backgroundColor: [
                    '#10b981',
                    '#3b82f6',
                    '#f59e0b',
                    '#ef4444',
                    '#8b5cf6'
                ],
                borderWidth: 2,
                borderColor: '#ffffff'
            }]
        },
        options: {
            responsive: true,
            maintainAspectRatio: false,
            plugins: {
                legend: {
                    position: 'bottom'
                }
            }
        }
    });
}

function initializeYearChart() {
    const ctx = document.getElementById('yearChart');
    if (!ctx || !window.yearLabels || !window.yearData) return;
    
    new Chart(ctx, {
        type: 'line',
        data: {
            labels: window.yearLabels,
            datasets: [{
                label: 'Số lượng mẫu',
                data: window.yearData,
                borderColor: '#ef4444',
                backgroundColor: 'rgba(239, 68, 68, 0.1)',
                borderWidth: 3,
                fill: true,
                tension: 0.4
            }]
        },
        options: {
            responsive: true,
            maintainAspectRatio: false,
            plugins: {
                legend: {
                    display: false
                }
            },
            scales: {
                y: {
                    beginAtZero: true,
                    grid: {
                        color: 'rgba(0, 0, 0, 0.1)'
                    }
                },
                x: {
                    grid: {
                        color: 'rgba(0, 0, 0, 0.1)'
                    }
                }
            }
        }
    });
}

function initializeRegionChart() {
    const ctx = document.getElementById('regionChart');
    if (!ctx || !window.regionLabels || !window.regionData) return;

    new Chart(ctx, {
        type: 'doughnut',
        data: {
            labels: window.regionLabels,
            datasets: [{
                data: window.regionData,
                backgroundColor: [
                    '#3b82f6',
                    '#f59e0b',
                    '#ef4444'
                ],
                borderWidth: 2,
                borderColor: '#ffffff'
            }]
        },
        options: {
            responsive: true,
            maintainAspectRatio: false,
            plugins: {
                legend: {
                    position: 'bottom'
                }
            }
        }
    });
}

// Show specific chart groups
function showChartGroup(group) {
    // Remove active class from all buttons
    document.querySelectorAll('.chart-controls .btn').forEach(btn => btn.classList.remove('active'));
    
    // Add active class to clicked button
    event.target.classList.add('active');
    
    // Hide all chart groups
    document.querySelectorAll('.chart-group').forEach(chart => {
        chart.style.display = 'none';
    });
    
    // Show selected group or all
    if (group === 'all') {
        document.querySelectorAll('.chart-group').forEach(chart => {
            chart.style.display = 'block';
        });
    } else {
        document.querySelectorAll(`.chart-group.${group}`).forEach(chart => {
            chart.style.display = 'block';
        });
    }
}

// Toggle data table
function toggleDataTable() {
    const container = document.getElementById('dataTableContainer');
    const text = document.getElementById('tableToggleText');
    
    if (container.style.display === 'none') {
        container.style.display = 'block';
        text.textContent = 'Ẩn dữ liệu';
    } else {
        container.style.display = 'none';
        text.textContent = 'Xem dữ liệu';
    }
}

// Reset filters
function resetFilters() {
    window.location.href = '/Manhinhchinh';
}

// Load all data
function loadAllData() {
    // This function can be implemented to load all data via AJAX
    alert('Chức năng này sẽ được phát triển trong tương lai');
}

// Export to Excel function based on DN logic
function exportToExcel(buttonElement) {
    console.log('📊 Exporting to Excel...');
    
    const form = document.querySelector('.filter-form');

    if (!form) {
        alert('❌ Không tìm thấy form filters. Vui lòng thử lại.');
        return;
    }
    
    const formData = new FormData(form);

    // Convert to URL parameters
    const params = new URLSearchParams();

    // Add all form data
    for (let [key, value] of formData.entries()) {
        params.append(key, value);
    }

    // Collect checkbox values (for multiple selections)
    const checkboxes = document.querySelectorAll('input[type="checkbox"]:checked');
    checkboxes.forEach(checkbox => {
        if (checkbox.name && checkbox.value) {
            params.append(checkbox.name, checkbox.value);
        }
    });

    console.log('📊 Export parameters:', params.toString());

    // Create download link
    const exportUrl = '/Manhinhchinh/ExportToExcel?' + params.toString();
    
    // Show loading message
    const originalText = buttonElement.innerHTML;
    buttonElement.innerHTML = '<i class="fas fa-spinner fa-spin me-1"></i>Đang xuất...';
    buttonElement.disabled = true;

    // Use fetch to check for errors before downloading
    fetch(exportUrl)
        .then(response => {
            // Check if response is JSON (error response)
            const contentType = response.headers.get('content-type');
            if (contentType && contentType.includes('application/json')) {
                // This is an error response, parse it
                return response.json().then(errorData => {
                    throw new Error(errorData.message || 'Lỗi khi export dữ liệu');
                });
            }
            
            // Check if response is OK for file download
            if (response.ok) {
                // If response is OK, trigger download
                const link = document.createElement('a');
                link.href = exportUrl;
                link.style.display = 'none';
                document.body.appendChild(link);
                link.click();
                document.body.removeChild(link);

                // Show success message
                const alertDiv = document.createElement('div');
                alertDiv.className = 'alert alert-success alert-dismissible fade show mt-3';
                alertDiv.innerHTML = `
                    <i class="fas fa-check-circle me-2"></i>
                    <strong>Thành công!</strong> File Excel đang được tải về.
                    <button type="button" class="btn-close" data-bs-dismiss="alert"></button>
                `;

                // Insert alert before table container
                const tableContainer = document.querySelector('#dataTableContainer');
                if (tableContainer) {
                    tableContainer.parentNode.insertBefore(alertDiv, tableContainer);

                    // Auto remove alert after 5 seconds
                    setTimeout(() => {
                        if (alertDiv.parentNode) {
                            alertDiv.remove();
                        }
                    }, 5000);
                }
            } else {
                throw new Error('Lỗi khi tải file Excel');
            }
        })
        .catch(error => {
            console.error('Export error:', error);
            alert('❌ Lỗi khi export: ' + error.message);
        })
        .finally(() => {
            // Restore button
            buttonElement.innerHTML = originalText;
            buttonElement.disabled = false;
        });
}

// Initialize page
document.addEventListener('DOMContentLoaded', function() {
    console.log('Page loaded, initializing charts...');
    if (typeof initializeCharts === 'function') {
        console.log('Initializing all charts from JavaScript file');
        initializeCharts();
    }

    // Close any open dropdowns when clicking export
    const exportBtn = document.getElementById('exportBtn');
    if (exportBtn) {
        exportBtn.addEventListener('click', function() {
            document.querySelectorAll('.dropdown-options').forEach(dd => dd.style.display = 'none');
        });
    }
});

console.log('=== MANHINHCHINH DASHBOARD JS LOADED SUCCESSFULLY ===');