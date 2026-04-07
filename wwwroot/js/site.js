// site.js - Main JavaScript file for Alfaneyah Project Dashboard

// Wait for DOM and jQuery to be ready
$(document).ready(function () {
    console.log("Alfaneyah Dashboard loaded successfully!");

    // ============================================
    // 1. SIDEBAR TOGGLE FUNCTIONALITY (BURGER BUTTON)
    // ============================================
    // Make sure the sidebar toggle button exists before attaching event
    if ($('#sidebarToggle').length) {
        $('#sidebarToggle').off('click').on('click', function (e) {
            e.preventDefault();
            toggleSidebar();
        });
    }

    // Close sidebar button
    if ($('#closeSidebarBtn').length) {
        $('#closeSidebarBtn').off('click').on('click', function () {
            if ($(window).width() <= 992) {
                $('#sidebar').addClass('collapsed');
                $('#mainContent').addClass('expanded');
                localStorage.setItem('sidebarCollapsed', true);
            }
        });
    }

    // Load saved sidebar state
    loadSidebarState();

    // ============================================
    // 2. LOGIN PAGE FUNCTIONALITY
    // ============================================
    if ($('.login-container').length) {
        initializeLoginPage();
    }

    // ============================================
    // 3. LOAD PROJECTS FOR SIDEBAR (if needed)
    // ============================================
    if ($('#projectListContainer').length) {
        loadProjectList();
    }

    // ============================================
    // 4. AUTO-HIDE ALERTS
    // ============================================
    setTimeout(function () {
        $('.alert-success, .alert-danger').fadeOut('slow');
    }, 5000);

    // ============================================
    // 5. CONFIRMATION DIALOGS
    // ============================================
    $('.btn-danger[asp-action="Delete"]').on('click', function (e) {
        if (!confirm('Are you sure you want to delete this item? This action cannot be undone.')) {
            e.preventDefault();
        }
    });

    // ============================================
    // 6. TABLE SORTING INDICATORS
    // ============================================
    $('.table-custom thead th.sortable').hover(
        function () { $(this).find('i').css('opacity', '0.8'); },
        function () {
            if (!$(this).hasClass('asc') && !$(this).hasClass('desc')) {
                $(this).find('i').css('opacity', '0.3');
            }
        }
    );

    // ============================================
    // 7. PAGE-SPECIFIC INITIALIZATION
    // ============================================
    initializePageSpecificFunctions();

    // ============================================
    // 8. MOBILE VIEW CHECK
    // ============================================
    checkMobileView();
    $(window).on('resize', function () {
        checkMobileView();
    });
});

// ============================================
// SIDEBAR TOGGLE FUNCTIONS (BURGER BUTTON)
// ============================================
function toggleSidebar() {
    var sidebar = $('#sidebar');
    var mainContent = $('#mainContent');

    sidebar.toggleClass('collapsed');
    mainContent.toggleClass('expanded');

    // Save preference to localStorage
    var isCollapsed = sidebar.hasClass('collapsed');
    localStorage.setItem('sidebarCollapsed', isCollapsed);

    // Trigger resize event for charts if any
    $(window).trigger('resize');

    console.log("Sidebar toggled, collapsed: " + isCollapsed);
}

function loadSidebarState() {
    var savedState = localStorage.getItem('sidebarCollapsed');
    var sidebar = $('#sidebar');
    var mainContent = $('#mainContent');

    if (savedState === 'true') {
        sidebar.addClass('collapsed');
        mainContent.addClass('expanded');
    } else if (savedState === 'false') {
        sidebar.removeClass('collapsed');
        mainContent.removeClass('expanded');
    } else {
        // Default: show sidebar on desktop
        sidebar.removeClass('collapsed');
        mainContent.removeClass('expanded');
    }
}

function checkMobileView() {
    var sidebar = $('#sidebar');
    var mainContent = $('#mainContent');

    if ($(window).width() <= 768) {
        // On mobile, always ensure sidebar is collapsed/hidden
        if (!sidebar.hasClass('collapsed')) {
            sidebar.addClass('collapsed');
            mainContent.addClass('expanded');
        }
    }
}

// ============================================
// LOGIN PAGE FUNCTIONS
// ============================================
function initializeLoginPage() {
    // Add focus effects
    $('.form-control').focus(function () {
        $(this).css('border-color', '#c3272c');
    }).blur(function () {
        $(this).css('border-color', '#e9ecef');
    });

    // Add loading state to form submission
    $('form').on('submit', function (e) {
        var btn = $(this).find('button[type="submit"]');
        var username = $('input[name="username"]').val().trim();
        var password = $('input[name="password"]').val().trim();

        if (!username || !password) {
            e.preventDefault();
            alert('Please fill in all fields');
            return false;
        }

        btn.html('<i class="fas fa-spinner fa-spin me-2"></i>Signing in...');
        btn.prop('disabled', true);
    });

    // Pre-fill demo credentials on click
    $('.demo-credentials p').click(function () {
        var text = $(this).text();
        if (text.includes('owner')) {
            $('input[name="username"]').val('owner');
            $('input[name="password"]').val('owner123');
        } else if (text.includes('accountant')) {
            $('input[name="username"]').val('accountant');
            $('input[name="password"]').val('acc123');
        } else if (text.includes('staff')) {
            $('input[name="username"]').val('staff');
            $('input[name="password"]').val('staff123');
        }
    });
}

// ============================================
// PROJECT LOADER FOR SIDEBAR
// ============================================
function loadProjectList() {
    $.get('/api/dashboard/projects', function (projects) {
        var html = '';
        projects.forEach(function (project) {
            var isActive = project.id == getUrlParameter('projectId');
            html += `
                <div class="project-item ${isActive ? 'active' : ''}" onclick="selectProject(${project.id})">
                    <div class="project-name">${project.projectName}</div>
                    <div class="project-progress">
                        <span>Progress</span>
                        <span>${project.progressPercentage}%</span>
                    </div>
                    <div class="progress-bar-custom">
                        <div class="progress-fill" style="width: ${project.progressPercentage}%"></div>
                    </div>
                </div>
            `;
        });
        $('#projectListContainer').html(html);
    }).fail(function () {
        console.log("Could not load projects for sidebar");
    });
}

function selectProject(projectId) {
    window.location.href = '/Home/Dashboard?projectId=' + projectId;
}

function getUrlParameter(name) {
    name = name.replace(/[\[]/, '\\[').replace(/[\]]/, '\\]');
    var regex = new RegExp('[\\?&]' + name + '=([^&#]*)');
    var results = regex.exec(location.search);
    return results === null ? '' : decodeURIComponent(results[1].replace(/\+/g, ' '));
}

// ============================================
// PAGE-SPECIFIC FUNCTIONS
// ============================================
function initializePageSpecificFunctions() {
    var currentPage = window.location.pathname.toLowerCase();

    if (currentPage.includes('dashboard')) {
        initializeDashboardPage();
    }
    if (currentPage.includes('projects')) {
        initializeProjectsFilters();
    }
    if (currentPage.includes('paymentclaims')) {
        initializePaymentClaimsFilters();
    }
    if (currentPage.includes('variationorders')) {
        initializeVariationOrdersFilters();
        initializeVariationOrdersPage();
    }
}

// ============================================
// DASHBOARD PAGE FUNCTIONS (Basic)
// ============================================
function initializeDashboardPage() {
    // Dashboard specific initialization
    console.log("Dashboard page initialized");

    // Initialize click handlers for dashboard cards if they exist
    if ($('.clickable-card').length) {
        $('.clickable-card').off('click').on('click', function () {
            var filter = $(this).data('filter');
            if (filter) {
                console.log("Dashboard card clicked: " + filter);
                // This will be handled by the view's script section
            }
        });
    }
}

// ============================================
// PROJECTS FILTERS
// ============================================
function initializeProjectsFilters() {
    $('#searchInput, #statusFilter, #locationFilter, #progressFilter').on('keyup change', function () {
        filterProjectsTable();
        updateActiveFilters();
    });

    $('#clearFilters, #clearFiltersLink').click(function (e) {
        e.preventDefault();
        $('#searchInput').val('');
        $('#statusFilter').val('all');
        $('#locationFilter').val('all');
        $('#progressFilter').val('all');
        filterProjectsTable();
        updateActiveFilters();
    });
}

function filterProjectsTable() {
    var searchTerm = $('#searchInput').val().toLowerCase();
    var statusFilter = $('#statusFilter').val();
    var locationFilter = $('#locationFilter').val();
    var progressFilter = $('#progressFilter').val();

    var visibleCount = 0;

    $('#projectsTable tbody tr').each(function () {
        var showRow = true;

        var projectName = $(this).data('project-name')?.toLowerCase() || '';
        var location = $(this).data('location')?.toLowerCase() || '';
        var scope = $(this).data('scope')?.toLowerCase() || '';
        var rowStatus = $(this).data('status') || '';
        var rowLocation = $(this).data('location') || '';
        var progress = parseFloat($(this).data('progress')) || 0;

        if (searchTerm) {
            if (!projectName.includes(searchTerm) &&
                !location.includes(searchTerm) &&
                !scope.includes(searchTerm)) {
                showRow = false;
            }
        }

        if (showRow && statusFilter !== 'all' && rowStatus !== statusFilter) {
            showRow = false;
        }

        if (showRow && locationFilter !== 'all') {
            if (rowLocation.toLowerCase() !== locationFilter.toLowerCase()) {
                showRow = false;
            }
        }

        if (showRow && progressFilter !== 'all') {
            switch (progressFilter) {
                case '0-25':
                    if (progress < 0 || progress > 25) showRow = false;
                    break;
                case '25-50':
                    if (progress < 25 || progress > 50) showRow = false;
                    break;
                case '50-75':
                    if (progress < 50 || progress > 75) showRow = false;
                    break;
                case '75-99':
                    if (progress < 75 || progress >= 100) showRow = false;
                    break;
                case '100':
                    if (progress < 100) showRow = false;
                    break;
            }
        }

        $(this).toggle(showRow);
        if (showRow) visibleCount++;
    });

    $('#resultCount').text(visibleCount + ' project' + (visibleCount !== 1 ? 's' : ''));
    $('#noResultsMessage').toggle(visibleCount === 0);
    updateActiveFilters();
}

// ============================================
// PAYMENT CLAIMS FILTERS
// ============================================
function initializePaymentClaimsFilters() {
    $('#searchInput, #projectFilter, #yearFilter, #monthFilter, #amountFilter').on('keyup change', function () {
        filterPaymentClaimsTable();
        updateActiveFilters();
    });

    $('#clearFilters, #clearFiltersLink').click(function (e) {
        e.preventDefault();
        $('#searchInput').val('');
        $('#projectFilter').val('all');
        $('#yearFilter').val('all');
        $('#monthFilter').val('all');
        $('#amountFilter').val('all');
        filterPaymentClaimsTable();
        updateActiveFilters();
    });

    $('#viewTable').click(function () {
        $(this).addClass('active');
        $('#viewGrouped').removeClass('active');
        $('#tableView').show();
        $('#groupedView').hide();
    });

    $('#viewGrouped').click(function () {
        $(this).addClass('active');
        $('#viewTable').removeClass('active');
        $('#tableView').hide();
        $('#groupedView').show();
    });
}

function filterPaymentClaimsTable() {
    var searchTerm = $('#searchInput').val().toLowerCase();
    var projectFilter = $('#projectFilter').val();
    var yearFilter = $('#yearFilter').val();
    var monthFilter = $('#monthFilter').val();
    var amountFilter = $('#amountFilter').val();

    var visibleCount = 0;
    var rows = [];

    $('#claimsTable tbody tr').each(function () {
        var showRow = true;

        var projectName = $(this).data('project-name')?.toLowerCase() || '';
        var description = $(this).data('description')?.toLowerCase() || '';
        var amount = parseFloat($(this).data('amount')) || 0;
        var rowProjectId = $(this).data('project-id');
        var rowYear = $(this).data('year');
        var rowMonth = $(this).data('month');

        if (searchTerm) {
            var amountStr = amount.toString();
            if (!projectName.includes(searchTerm) && !description.includes(searchTerm) && !amountStr.includes(searchTerm)) {
                showRow = false;
            }
        }

        if (showRow && projectFilter !== 'all' && rowProjectId != projectFilter) showRow = false;
        if (showRow && yearFilter !== 'all' && rowYear != yearFilter) showRow = false;
        if (showRow && monthFilter !== 'all' && rowMonth != monthFilter) showRow = false;

        $(this).toggle(showRow);
        if (showRow) {
            visibleCount++;
            rows.push($(this));
        }
    });

    if (amountFilter === 'high') {
        rows.sort((a, b) => parseFloat(b.data('amount')) - parseFloat(a.data('amount')));
        $('#claimsTable tbody').empty().append(rows);
    } else if (amountFilter === 'low') {
        rows.sort((a, b) => parseFloat(a.data('amount')) - parseFloat(b.data('amount')));
        $('#claimsTable tbody').empty().append(rows);
    }

    $('#resultCount').text(visibleCount + ' item' + (visibleCount !== 1 ? 's' : ''));
    $('#noResultsMessage').toggle(visibleCount === 0);
}

// ============================================
// VARIATION ORDERS FILTERS
// ============================================
function initializeVariationOrdersFilters() {
    $('#searchInput, #projectFilter, #voTypeFilter, #yearFilter, #monthFilter, #amountFilter').on('keyup change', function () {
        filterVariationOrdersTable();
        updateActiveFilters();
    });

    $('#clearFilters, #clearFiltersLink').click(function (e) {
        e.preventDefault();
        $('#searchInput').val('');
        $('#projectFilter').val('all');
        $('#voTypeFilter').val('all');
        $('#yearFilter').val('all');
        $('#monthFilter').val('all');
        $('#amountFilter').val('all');
        filterVariationOrdersTable();
        updateActiveFilters();
    });

    $('#viewTable').click(function () {
        $(this).addClass('active');
        $('#viewGrouped').removeClass('active');
        $('#tableView').show();
        $('#groupedView').hide();
    });

    $('#viewGrouped').click(function () {
        $(this).addClass('active');
        $('#viewTable').removeClass('active');
        $('#tableView').hide();
        $('#groupedView').show();
    });
}

function filterVariationOrdersTable() {
    var searchTerm = $('#searchInput').val().toLowerCase();
    var projectFilter = $('#projectFilter').val();
    var typeFilter = $('#voTypeFilter').val();
    var yearFilter = $('#yearFilter').val();
    var monthFilter = $('#monthFilter').val();
    var amountFilter = $('#amountFilter').val();

    var visibleCount = 0;
    var rows = [];

    $('#voTable tbody tr').each(function () {
        var showRow = true;

        var projectName = $(this).data('project-name')?.toLowerCase() || '';
        var scope = $(this).data('scope')?.toLowerCase() || '';
        var amount = parseFloat($(this).data('amount')) || 0;
        var rowProjectId = $(this).data('project-id');
        var rowYear = $(this).data('year');
        var rowMonth = $(this).data('month');

        if (searchTerm) {
            var amountStr = Math.abs(amount).toString();
            if (!projectName.includes(searchTerm) && !scope.includes(searchTerm) && !amountStr.includes(searchTerm)) {
                showRow = false;
            }
        }

        if (showRow && projectFilter !== 'all' && rowProjectId != projectFilter) showRow = false;

        if (showRow && typeFilter !== 'all') {
            if (typeFilter === 'positive' && amount <= 0) showRow = false;
            if (typeFilter === 'negative' && amount >= 0) showRow = false;
            if (typeFilter === 'zero' && amount !== 0) showRow = false;
        }

        if (showRow && yearFilter !== 'all' && rowYear != yearFilter) showRow = false;
        if (showRow && monthFilter !== 'all' && rowMonth != monthFilter) showRow = false;

        $(this).toggle(showRow);
        if (showRow) {
            visibleCount++;
            rows.push($(this));
        }
    });

    if (amountFilter === 'high') {
        rows.sort((a, b) => parseFloat(b.data('amount')) - parseFloat(a.data('amount')));
        $('#voTable tbody').empty().append(rows);
    } else if (amountFilter === 'low') {
        rows.sort((a, b) => parseFloat(a.data('amount')) - parseFloat(b.data('amount')));
        $('#voTable tbody').empty().append(rows);
    }

    $('#resultCount').text(visibleCount + ' item' + (visibleCount !== 1 ? 's' : ''));
    $('#noResultsMessage').toggle(visibleCount === 0);
}

// ============================================
// VARIATION ORDERS PAGE FUNCTIONS
// ============================================
function initializeVariationOrdersPage() {
    // Initialize click handlers for cards
    $('.clickable-card').off('click').on('click', function () {
        var filter = $(this).data('vo-filter');
        var title = '';
        var voType = '';

        switch (filter) {
            case 'positive':
                title = '✅ Positive Variation Orders (+ Value)';
                voType = 'positive';
                break;
            case 'negative':
                title = '⚠️ Negative Variation Orders (- Value)';
                voType = 'negative';
                break;
            case 'zero':
                title = '⏸️ Zero Value Variation Orders';
                voType = 'zero';
                break;
            case 'all':
                title = '📊 All Variation Orders';
                voType = 'all';
                break;
        }

        showVOModal(title, voType);
    });

    // Initialize project header clicks for grouped view
    $('.clickable-project').off('click').on('click', function () {
        var projectId = $(this).data('project-id');
        var projectName = $(this).data('project-name');
        if (projectId) {
            $('#projectFilter').val(projectId).trigger('change');
            $('#viewTable').click();
            $('#searchInput').val('');
            filterVariationOrdersTable();
            updateActiveFilters();
            showToast('Showing VOs for: ' + projectName, 'info');
        }
    });
}

function showVOModal(title, voType) {
    $('#modalTitle').text(title);

    var filteredVOs = [];

    if (voType === 'all') {
        filteredVOs = $('#voTable tbody tr');
    } else {
        filteredVOs = $('#voTable tbody tr[data-vo-type="' + voType + '"]');
    }

    if (filteredVOs.length === 0) {
        $('#modalVOList').html('<div class="text-center py-5"><i class="fas fa-folder-open fa-4x text-muted mb-3"></i><h5 class="text-muted">No variation orders found</h5></div>');
        $('#voDetailsModal').modal('show');
        return;
    }

    var totalFilteredAmount = 0;
    filteredVOs.each(function () {
        totalFilteredAmount += parseFloat($(this).data('amount')) || 0;
    });

    var vosHtml = '<div class="row"><div class="col-12 mb-4"><div class="alert alert-info">Found <strong>' + filteredVOs.length + '</strong> variation order(s)<span class="ms-3"><strong>Total Amount:</strong> <span>' + (totalFilteredAmount > 0 ? '+' : '') + totalFilteredAmount.toLocaleString() + ' SAR</span></span></div></div></div><div class="row vo-cards-container">';

    filteredVOs.each(function () {
        var projectName = $(this).find('td:eq(0) a').text().trim();
        var scope = $(this).find('td:eq(1) span').attr('title') || $(this).find('td:eq(1)').text().trim();
        var amount = parseFloat($(this).data('amount')) || 0;
        var approvedDate = $(this).find('td:eq(3)').text().trim();
        var editLink = $(this).find('td:eq(4) .btn-group a:first').attr('href');
        var amountClass = amount > 0 ? 'positive' : amount < 0 ? 'negative' : 'zero';
        var amountIcon = amount > 0 ? 'fa-arrow-up' : amount < 0 ? 'fa-arrow-down' : 'fa-minus';
        var amountColor = amount > 0 ? '#10b981' : amount < 0 ? '#ef4444' : '#6b7280';

        vosHtml += '<div class="col-md-6 col-lg-4 mb-4"><div class="vo-card" onclick="window.location.href=\'' + editLink + '\'"><div class="vo-card-header"><div class="vo-amount-badge ' + amountClass + '"><i class="fas ' + amountIcon + ' me-1"></i>' + Math.abs(amount).toLocaleString() + ' SAR</div><h4 class="vo-card-title">' + escapeHtml(projectName) + '</h4><p class="vo-date"><i class="fas fa-calendar-alt me-1"></i>' + approvedDate + '</p></div><div class="vo-card-body"><p class="vo-scope">' + escapeHtml(scope.length > 100 ? scope.substring(0, 100) + '...' : scope) + '</p><div class="vo-impact"><div class="impact-label"><span>Financial Impact</span><span class="impact-percentage">' + (amount > 0 ? 'Increase' : amount < 0 ? 'Decrease' : 'Neutral') + '</span></div><div class="progress" style="height: 8px;"><div class="progress-bar" style="width: ' + Math.min(Math.abs(amount) / 1000000 * 100, 100) + '%; background: ' + amountColor + ';"></div></div></div></div><div class="vo-card-footer"><span class="view-details">Click to edit details <i class="fas fa-arrow-right ms-1"></i></span></div></div></div>';
    });

    vosHtml += '</div>';
    $('#modalVOList').html(vosHtml);
    $('#voDetailsModal').modal('show');
}

// ============================================
// UPDATE ACTIVE FILTERS DISPLAY
// ============================================
function updateActiveFilters() {
    var activeFilters = [];
    var searchTerm = $('#searchInput').val();

    if (searchTerm) {
        activeFilters.push('<span class="badge bg-primary me-1">Search: ' + searchTerm + '</span>');
    }

    if ($('#projectFilter').length && $('#projectFilter').val() !== 'all') {
        activeFilters.push('<span class="badge bg-primary me-1">Project: ' + $('#projectFilter option:selected').text() + '</span>');
    }

    if ($('#statusFilter').length && $('#statusFilter').val() !== 'all') {
        activeFilters.push('<span class="badge bg-primary me-1">Status: ' + $('#statusFilter option:selected').text() + '</span>');
    }

    if ($('#locationFilter').length && $('#locationFilter').val() !== 'all') {
        activeFilters.push('<span class="badge bg-primary me-1">Location: ' + $('#locationFilter option:selected').text() + '</span>');
    }

    if ($('#progressFilter').length && $('#progressFilter').val() !== 'all') {
        activeFilters.push('<span class="badge bg-primary me-1">Progress: ' + $('#progressFilter option:selected').text() + '</span>');
    }

    if ($('#voTypeFilter').length && $('#voTypeFilter').val() !== 'all') {
        activeFilters.push('<span class="badge bg-primary me-1">Type: ' + $('#voTypeFilter option:selected').text() + '</span>');
    }

    if ($('#yearFilter').length && $('#yearFilter').val() !== 'all') {
        activeFilters.push('<span class="badge bg-primary me-1">Year: ' + $('#yearFilter option:selected').text() + '</span>');
    }

    if ($('#monthFilter').length && $('#monthFilter').val() !== 'all') {
        activeFilters.push('<span class="badge bg-primary me-1">Month: ' + $('#monthFilter option:selected').text() + '</span>');
    }

    if ($('#amountFilter').length && $('#amountFilter').val() !== 'all') {
        activeFilters.push('<span class="badge bg-primary me-1">Sort: ' + $('#amountFilter option:selected').text() + '</span>');
    }

    if (activeFilters.length > 0) {
        $('#activeFilters').html('<i class="fas fa-filter text-muted me-2"></i>' + activeFilters.join(''));
    } else {
        $('#activeFilters').html('');
    }
}

// ============================================
// HELPER FUNCTIONS
// ============================================
function escapeHtml(text) {
    if (!text) return '';
    return text.replace(/&/g, '&amp;').replace(/</g, '&lt;').replace(/>/g, '&gt;').replace(/"/g, '&quot;').replace(/'/g, '&#39;');
}

function showToast(message, type) {
    type = type || 'success';
    var toast = $('<div class="toast-notification ' + type + '"><i class="fas fa-' + (type === 'success' ? 'check-circle' : type === 'info' ? 'info-circle' : 'exclamation-triangle') + ' me-2"></i>' + message + '</div>');
    $('body').append(toast);
    setTimeout(function () { toast.fadeOut('slow', function () { toast.remove(); }); }, 3000);
}

function formatCurrency(amount) {
    return new Intl.NumberFormat('en-SA', {
        style: 'currency',
        currency: 'SAR',
        minimumFractionDigits: 0,
        maximumFractionDigits: 0
    }).format(amount);
}

function formatDate(dateString) {
    var date = new Date(dateString);
    return date.toLocaleDateString('en-GB', {
        day: '2-digit',
        month: '2-digit',
        year: 'numeric'
    }).split('/').join('/');
}

// Export functions to global scope
window.Alfaneyah = {
    formatCurrency: formatCurrency,
    formatDate: formatDate,
    toggleSidebar: toggleSidebar,
    showToast: showToast,
    escapeHtml: escapeHtml
};
