// BookWise - Online Booking System JS Orchestrator
// Designed and implemented by Ahmed Mohamed Hosni

let bookingState = {
    currentStep: 1,
    categoryId: null,
    serviceId: null,
    serviceName: "",
    servicePrice: 0,
    serviceDuration: 0,
    providerId: null,
    providerName: "",
    selectedDate: null, // format: YYYY-MM-DD
    timeSlot: null,
    
    // Calendar variables
    currentCalendarYear: new Date().getFullYear(),
    currentCalendarMonth: new Date().getMonth() // 0-indexed
};

$(document).ready(function () {
    // Initialize booking workflow if booking form exists
    if ($("#bookingForm").length > 0) {
        initBookingWizard();
    }
});

function initBookingWizard() {
    // Check if query params pre-select any values
    const urlParams = new URLSearchParams(window.location.search);
    const preCategoryId = urlParams.get('categoryId');
    const preServiceId = urlParams.get('serviceId');
    const preProviderId = urlParams.get('providerId');

    // Register Category Select Event Listeners
    $(".cat-select-card").on("click", function () {
        const catId = $(this).data("category-id");
        selectCategory(catId);
    });

    // Back Buttons
    $(".prev-step-btn").on("click", function () {
        navigateStep(bookingState.currentStep - 1);
    });

    // Calendar Navigation
    $("#prevMonthBtn").on("click", function () {
        changeMonth(-1);
    });
    $("#nextMonthBtn").on("click", function () {
        changeMonth(1);
    });

    // Step indicators header navigation (only for already completed/active steps)
    $(".booking-step").on("click", function () {
        const targetStep = parseInt($(this).attr("id").replace("step", "").replace("-btn", ""));
        if (targetStep < bookingState.currentStep) {
            navigateStep(targetStep);
        }
    });

    // Handle initial state if params passed
    if (preCategoryId) {
        selectCategory(parseInt(preCategoryId));
    }
}

// -------------------------------------------------------------
// Core Navigation & State Management
// -------------------------------------------------------------
function navigateStep(stepNumber) {
    if (stepNumber < 1 || stepNumber > 5) return;
    
    // Hide current panel
    $(`#panel-step${bookingState.currentStep}`).addClass("d-none");
    $(`#step${bookingState.currentStep}-btn`).removeClass("active");
    if (bookingState.currentStep > stepNumber) {
        $(`#step${bookingState.currentStep}-btn`).removeClass("completed");
    } else {
        $(`#step${bookingState.currentStep}-btn`).addClass("completed");
    }

    // Show target panel
    $(`#panel-step${stepNumber}`).removeClass("d-none").addClass("animate-fade-in");
    $(`#step${stepNumber}-btn`).addClass("active");

    // Update state
    bookingState.currentStep = stepNumber;

    // Update Progress Line width
    const percentage = ((stepNumber - 1) / 4) * 100;
    $("#stepProgress").css("width", `${percentage}%`);

    // Special steps setups
    if (stepNumber === 4) {
        renderCalendar();
    } else if (stepNumber === 5) {
        updateBookingSummary();
    }
}

// -------------------------------------------------------------
// Step 1: Category Selection -> Load Services
// -------------------------------------------------------------
function selectCategory(catId) {
    bookingState.categoryId = catId;
    navigateStep(2);
    
    // Fetch Services under this Category via AJAX
    $("#servicesGrid").html(`
        <div class="col-12 text-center text-secondary py-5">
            <div class="spinner-border text-primary" role="status"></div>
            <p class="small mt-2">Fetching services...</p>
        </div>
    `);

    $.getJSON(`/Booking/GetServicesByCategory?categoryId=${catId}`, function (data) {
        if (!data || data.length === 0) {
            $("#servicesGrid").html(`
                <div class="col-12 text-center text-muted py-5">
                    <i class="bi bi-box-fill display-6 mb-3"></i>
                    <p class="small">No services available in this category at the moment.</p>
                </div>
            `);
            return;
        }

        let html = '';
        data.forEach(srv => {
            html += `
                <div class="col-md-6 animate-fade-in">
                    <div class="glass-card service-select-card" data-service-id="${srv.id}" data-service-name="${srv.name}" data-service-price="${srv.price}" data-service-duration="${srv.durationMinutes}" style="cursor:pointer;">
                        <div class="d-flex justify-content-between align-items-start mb-3">
                            <h4 class="h6 text-white mb-0">${srv.name}</h4>
                            <span class="fw-bold text-accent">$${srv.price.toFixed(2)}</span>
                        </div>
                        <p class="text-secondary small mb-3">${srv.description}</p>
                        <div class="d-flex justify-content-between align-items-center small text-muted pt-2 border-top" style="border-color: var(--border-color);">
                            <span><i class="bi bi-clock me-1"></i> ${srv.durationMinutes} mins</span>
                            <span class="text-primary fw-semibold">Select <i class="bi bi-chevron-right"></i></span>
                        </div>
                    </div>
                </div>
            `;
        });
        
        $("#servicesGrid").html(html);

        // Bind click events on service cards
        $(".service-select-card").on("click", function () {
            const serviceId = $(this).data("service-id");
            bookingState.serviceId = serviceId;
            bookingState.serviceName = $(this).data("service-name");
            bookingState.servicePrice = parseFloat($(this).data("service-price"));
            bookingState.serviceDuration = parseInt($(this).data("service-duration"));
            $("#serviceId").val(serviceId);
            
            selectService(serviceId);
        });
    });
}

// -------------------------------------------------------------
// Step 2: Service Selection -> Load Providers
// -------------------------------------------------------------
function selectService(serviceId) {
    navigateStep(3);

    // Fetch Providers for this Service via AJAX
    $("#providersGrid").html(`
        <div class="col-12 text-center text-secondary py-5">
            <div class="spinner-border text-primary" role="status"></div>
            <p class="small mt-2">Finding available specialists...</p>
        </div>
    `);

    $.getJSON(`/Booking/GetProvidersByService?serviceId=${serviceId}`, function (data) {
        if (!data || data.length === 0) {
            $("#providersGrid").html(`
                <div class="col-12 text-center text-muted py-5">
                    <i class="bi bi-people-fill display-6 mb-3"></i>
                    <p class="small">No specialists available for this service currently.</p>
                </div>
            `);
            return;
        }

        let html = '';
        data.forEach(prov => {
            // Generate simple initials avatar for mockups
            const names = prov.name.split(' ');
            const initials = names.length > 1 ? names[0][0] + names[1][0] : names[0][0];

            html += `
                <div class="col-md-4 animate-fade-in">
                    <div class="glass-card provider-select-card text-center d-flex flex-column align-items-center" data-provider-id="${prov.id}" data-provider-name="${prov.name}" style="cursor:pointer;">
                        <div class="rounded-circle bg-secondary p-1 mb-3" style="background: linear-gradient(135deg, var(--primary), var(--secondary)); width: 80px; height: 80px;">
                            <div class="rounded-circle bg-dark d-flex align-items-center justify-content-center" style="width: 72px; height: 72px;">
                                <span class="fs-4 fw-bold text-white">${initials}</span>
                            </div>
                        </div>
                        <h4 class="h6 text-white mb-1">${prov.name}</h4>
                        <span class="badge-custom badge-custom-primary mb-3" style="font-size: 0.7rem;">${prov.title}</span>
                        <div class="rating-stars mb-3">
                            ${Array(5).fill(0).map((_, i) => i < Math.round(prov.rating) ? '<i class="bi bi-star-fill"></i>' : '<i class="bi bi-star"></i>').join('')}
                            <span class="text-muted ms-1 small">(${prov.rating.toFixed(1)})</span>
                        </div>
                        <p class="text-secondary small mb-0 px-2" style="height: 48px; overflow: hidden; display: -webkit-box; -webkit-line-clamp: 2; -webkit-box-orient: vertical;">
                            ${prov.bio}
                        </p>
                    </div>
                </div>
            `;
        });

        $("#providersGrid").html(html);

        // Bind click events on provider cards
        $(".provider-select-card").on("click", function () {
            const providerId = $(this).data("provider-id");
            bookingState.providerId = providerId;
            bookingState.providerName = $(this).data("provider-name");
            $("#providerId").val(providerId);
            
            navigateStep(4);
        });
    });
}

// -------------------------------------------------------------
// Step 3: Schedule Calendar & Slots Selection
// -------------------------------------------------------------
function renderCalendar() {
    const year = bookingState.currentCalendarYear;
    const month = bookingState.currentCalendarMonth;
    
    // Month Names
    const monthNames = ["January", "February", "March", "April", "May", "June", "July", "August", "September", "October", "November", "December"];
    $("#calendarMonthYear").text(`${monthNames[month]} ${year}`);

    // Get first day of month and total days
    const firstDayIndex = new Date(year, month, 1).getDay();
    const totalDays = new Date(year, month + 1, 0).getDate();
    
    // Get last month days for padding
    const prevMonthTotalDays = new Date(year, month, 0).getDate();

    let daysHtml = '';
    const today = new Date();
    today.setHours(0,0,0,0);

    // Padding days from previous month
    for (let i = firstDayIndex - 1; i >= 0; i--) {
        daysHtml += `<div class="calendar-day disabled">${prevMonthTotalDays - i}</div>`;
    }

    // Days of current month
    for (let day = 1; day <= totalDays; day++) {
        const currentDate = new Date(year, month, day);
        let classes = 'calendar-day';
        let isPast = currentDate < today;
        let isToday = currentDate.getTime() === today.getTime();

        if (isPast) {
            classes += ' disabled';
        }
        if (isToday) {
            classes += ' today';
        }

        // Format Date string: YYYY-MM-DD
        const formattedMonth = String(month + 1).padStart(2, '0');
        const formattedDay = String(day).padStart(2, '0');
        const dateString = `${year}-${formattedMonth}-${formattedDay}`;

        if (bookingState.selectedDate === dateString) {
            classes += ' selected';
        }

        if (isPast) {
            daysHtml += `<div class="${classes}">${day}</div>`;
        } else {
            daysHtml += `<div class="${classes}" data-date="${dateString}">${day}</div>`;
        }
    }

    $("#calendarDays").html(daysHtml);

    // Day Click Listener
    $(".calendar-day:not(.disabled)").on("click", function () {
        $(".calendar-day").removeClass("selected");
        $(this).addClass("selected");
        
        const dateStr = $(this).data("date");
        bookingState.selectedDate = dateStr;
        $("#appointmentDate").val(dateStr);
        
        loadAvailableTimeSlots(dateStr);
    });
}

function changeMonth(direction) {
    bookingState.currentCalendarMonth += direction;
    if (bookingState.currentCalendarMonth < 0) {
        bookingState.currentCalendarMonth = 11;
        bookingState.currentCalendarYear--;
    } else if (bookingState.currentCalendarMonth > 11) {
        bookingState.currentCalendarMonth = 0;
        bookingState.currentCalendarYear++;
    }
    renderCalendar();
}

function loadAvailableTimeSlots(dateStr) {
    $("#slotsContainer").addClass("d-none");
    $("#slotsLoader").removeClass("d-none");

    const providerId = bookingState.providerId;
    const serviceId = bookingState.serviceId;

    $.getJSON(`/Booking/GetAvailableSlots?providerId=${providerId}&date=${dateStr}&serviceId=${serviceId}`, function (data) {
        $("#slotsLoader").addClass("d-none");
        $("#slotsContainer").removeClass("d-none");

        if (!data || data.length === 0) {
            $("#slotsContainer").html(`
                <div class="text-center text-muted w-100 py-5">
                    <i class="bi bi-emoji-frown text-warning display-6 mb-3"></i>
                    <p class="small mb-0">No time slots are available for the selected specialist on this date. Please try another day.</p>
                </div>
            `);
            return;
        }

        let html = '';
        data.forEach(slot => {
            const isAvailable = slot.available;
            const disabledClass = isAvailable ? '' : 'disabled';
            const selectedClass = (bookingState.timeSlot === slot.time && isAvailable) ? 'selected' : '';

            // Format time slot nicely (e.g. converting 09:00 to 09:00 AM)
            const hour = parseInt(slot.time.split(':')[0]);
            const minutes = slot.time.split(':')[1];
            const ampm = hour >= 12 ? 'PM' : 'AM';
            const displayHour = hour > 12 ? hour - 12 : (hour === 0 ? 12 : hour);
            const formattedTimeLabel = `${String(displayHour).padStart(2, '0')}:${minutes} ${ampm}`;

            html += `
                <div class="time-slot-button ${disabledClass} ${selectedClass}" data-time-slot="${slot.time}">
                    ${formattedTimeLabel}
                </div>
            `;
        });

        $("#slotsContainer").html(html);

        // Bind click events on time slots
        $(".time-slot-button:not(.disabled)").on("click", function () {
            $(".time-slot-button").removeClass("selected");
            $(this).addClass("selected");

            const timeStr = $(this).data("time-slot");
            bookingState.timeSlot = timeStr;
            $("#timeSlot").val(timeStr);

            // Navigate to step 5 automatically for a fast UX
            setTimeout(() => {
                navigateStep(5);
            }, 300);
        });
    });
}

// -------------------------------------------------------------
// Step 4: Summary Mapping
// -------------------------------------------------------------
function updateBookingSummary() {
    $("#summaryService").text(bookingState.serviceName);
    $("#summaryProvider").text(bookingState.providerName);
    
    // Format Date nicely
    if (bookingState.selectedDate) {
        const parts = bookingState.selectedDate.split('-');
        const dateObj = new Date(parts[0], parts[1] - 1, parts[2]);
        const formatOptions = { weekday: 'long', year: 'numeric', month: 'long', day: 'numeric' };
        $("#summaryDate").text(dateObj.toLocaleDateString('en-US', formatOptions));
    }
    
    // Format Time nicely
    if (bookingState.timeSlot) {
        const hour = parseInt(bookingState.timeSlot.split(':')[0]);
        const minutes = bookingState.timeSlot.split(':')[1];
        const ampm = hour >= 12 ? 'PM' : 'AM';
        const displayHour = hour > 12 ? hour - 12 : (hour === 0 ? 12 : hour);
        $("#summaryTime").text(`${String(displayHour).padStart(2, '0')}:${minutes} ${ampm}`);
    }

    $("#summaryPrice").text(`$${bookingState.servicePrice.toFixed(2)}`);
}
