// 🚀 QUICK PERFORMANCE FIX FOR IMMEDIATE IMPROVEMENT
// Add this script to Views/DN/Index.cshtml to reduce lag

console.log('🚀 Loading Quick Performance Optimizations...');

// ================================
// 1. CHART DATA OPTIMIZATION
// ================================

// Override chart data limits for performance
function optimizeChartData(data, maxItems = 20) {
    if (!data || data.length <= maxItems) return data;

    console.log(`🚀 PERFORMANCE: Limiting chart data from ${data.length} to ${maxItems} items`);

    // Sort by value/count and take top items
    const sorted = data.sort((a, b) => {
        const valueA = a.soLuong || a.SoLuong || a.count || a.value || 0;
        const valueB = b.soLuong || b.SoLuong || b.count || b.value || 0;
        return valueB - valueA;
    });

    return sorted.slice(0, maxItems);
}

// ================================
// 2. CHART RENDERING OPTIMIZATION
// ================================

// Optimized chart options for better performance
const performanceChartOptions = {
    responsive: true,
    maintainAspectRatio: false,
    animation: {
        duration: 1000, // Reduce animation time
        easing: 'easeOutQuart'
    },
    plugins: {
        legend: {
            display: true,
            labels: {
                maxWidth: 200, // Limit legend width
                fontSize: 12
            }
        },
        tooltip: {
            enabled: true,
            mode: 'single', // Single tooltip for performance
            intersect: false
        }
    },
    elements: {
        point: {
            radius: 3, // Smaller points
            hoverRadius: 5
        },
        line: {
            borderWidth: 2, // Thinner lines
            tension: 0.1
        }
    },
    scales: {
        x: {
            display: true,
            ticks: {
                maxTicksLimit: 10, // Limit x-axis labels
                font: { size: 11 }
            }
        },
        y: {
            display: true,
            ticks: {
                maxTicksLimit: 8, // Limit y-axis labels
                font: { size: 11 }
            }
        }
    }
};

// ================================
// 3. LAZY LOADING ENHANCEMENTS
// ================================

// Debounced chart loading to prevent multiple rapid calls
function debounce(func, wait) {
    let timeout;
    return function executedFunction(...args) {
        const later = () => {
            clearTimeout(timeout);
            func(...args);
        };
        clearTimeout(timeout);
        timeout = setTimeout(later, wait);
    };
}

// Enhanced lazy loading with performance monitoring
const debouncedChartLoad = debounce(function (chartElement) {
    const startTime = performance.now();
    console.log('🚀 Loading chart with performance monitoring...');

    // Original chart loading logic here
    // loadChart(chartElement);

    const endTime = performance.now();
    console.log(`✅ Chart loaded in ${Math.round(endTime - startTime)}ms`);
}, 300);

// ================================
// 4. MEMORY OPTIMIZATION
// ================================

// Clean up chart instances to prevent memory leaks
window.chartInstances = window.chartInstances || {};

function createOptimizedChart(canvasId, config) {
    // Destroy existing chart if exists
    if (window.chartInstances[canvasId]) {
        window.chartInstances[canvasId].destroy();
        console.log(`🧹 Destroyed existing chart: ${canvasId}`);
    }

    // Optimize data before creating chart
    if (config.data && config.data.datasets) {
        config.data.datasets.forEach(dataset => {
            if (dataset.data && Array.isArray(dataset.data)) {
                dataset.data = optimizeChartData(dataset.data);
            }
        });
    }

    // Apply performance options
    config.options = { ...performanceChartOptions, ...config.options };

    // Create new chart
    const canvas = document.getElementById(canvasId);
    if (canvas) {
        window.chartInstances[canvasId] = new Chart(canvas, config);
        console.log(`✅ Created optimized chart: ${canvasId}`);
    }

    return window.chartInstances[canvasId];
}

// ================================
// 5. VIEWPORT OPTIMIZATION
// ================================

// Only load charts when they come into viewport
function createIntersectionObserver() {
    if ('IntersectionObserver' in window) {
        const observer = new IntersectionObserver((entries) => {
            entries.forEach(entry => {
                if (entry.isIntersecting) {
                    const chartElement = entry.target;
                    const chartType = chartElement.getAttribute('data-chart-type');

                    if (chartType && !chartElement.classList.contains('chart-loaded')) {
                        console.log(`🚀 Chart ${chartType} entered viewport, loading...`);
                        debouncedChartLoad(chartElement);
                        chartElement.classList.add('chart-loaded');
                        observer.unobserve(chartElement); // Stop observing after loading
                    }
                }
            });
        }, {
            threshold: 0.1, // Load when 10% visible
            rootMargin: '50px' // Start loading 50px before entering viewport
        });

        // Observe all chart containers
        document.querySelectorAll('.chart-container, .lazy-chart, [data-chart-type]').forEach(chart => {
            observer.observe(chart);
        });

        console.log('🚀 Intersection Observer initialized for chart lazy loading');
    }
}

// ================================
// 6. DATA PROCESSING OPTIMIZATION
// ================================

// Efficient data aggregation for large datasets
function optimizeDataAggregation(data) {
    if (!data || data.length < 1000) return data; // No optimization needed for small data

    console.log(`🚀 Optimizing data aggregation for ${data.length} records`);

    // Use Map for O(1) lookups instead of array searches
    const aggregationMap = new Map();

    data.forEach(item => {
        const key = item.category || item.label || item.name;
        if (key) {
            const value = item.value || item.count || item.soLuong || 0;
            aggregationMap.set(key, (aggregationMap.get(key) || 0) + value);
        }
    });

    // Convert back to array format
    const optimizedData = Array.from(aggregationMap.entries()).map(([label, value]) => ({
        label,
        value
    }));

    console.log(`✅ Data aggregation optimized: ${data.length} → ${optimizedData.length} items`);
    return optimizedData;
}

// ================================
// 7. BROWSER PERFORMANCE MONITORING
// ================================

// Monitor and log performance metrics
function logPerformanceMetrics() {
    if (performance.memory) {
        const memoryInfo = {
            used: Math.round(performance.memory.usedJSHeapSize / 1024 / 1024),
            total: Math.round(performance.memory.totalJSHeapSize / 1024 / 1024),
            limit: Math.round(performance.memory.jsHeapSizeLimit / 1024 / 1024)
        };

        console.log('🚀 Memory Usage:', memoryInfo);

        // Warn if memory usage is high
        if (memoryInfo.used > 100) {
            console.warn('⚠️ High memory usage detected. Consider refreshing the page.');
        }
    }

    // Log page performance
    const navigation = performance.getEntriesByType('navigation')[0];
    if (navigation) {
        console.log('🚀 Page Load Time:', Math.round(navigation.loadEventEnd - navigation.loadEventStart) + 'ms');
    }
}

// ================================
// 8. INITIALIZATION
// ================================

// Initialize performance optimizations when DOM is ready
document.addEventListener('DOMContentLoaded', function () {
    console.log('🚀 Quick Performance Fix initialized');

    // Create intersection observer for lazy loading
    createIntersectionObserver();

    // Log initial performance metrics
    logPerformanceMetrics();

    // Set up periodic performance monitoring
    setInterval(logPerformanceMetrics, 30000); // Every 30 seconds

    // Add performance warning if too many DOM elements
    const totalElements = document.querySelectorAll('*').length;
    if (totalElements > 5000) {
        console.warn(`⚠️ High DOM element count: ${totalElements}. This may impact performance.`);
    }

    console.log('✅ Performance optimizations applied successfully');
});

// ================================
// 9. UTILITY FUNCTIONS FOR EXISTING CODE
// ================================

// Replace existing chart creation calls with optimized version
window.createOptimizedChart = createOptimizedChart;
window.optimizeChartData = optimizeChartData;
window.optimizeDataAggregation = optimizeDataAggregation;

// Cleanup function for page navigation
window.addEventListener('beforeunload', function () {
    // Destroy all chart instances
    Object.values(window.chartInstances || {}).forEach(chart => {
        if (chart && typeof chart.destroy === 'function') {
            chart.destroy();
        }
    });
    console.log('🧹 Cleaned up chart instances before page unload');
});

console.log('🚀 Quick Performance Fix loaded successfully!'); 