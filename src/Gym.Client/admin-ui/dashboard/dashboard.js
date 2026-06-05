const token = localStorage.getItem("accessToken");

if (!token) {
  window.location.href = "../auth/index.html";
}

async function loadDashboardStats() {
  try {
    const response = await fetch(
      "https://localhost:7022/api/v1/dashboard/stats",
      {
        headers: {
          Authorization: `Bearer ${token}`,
        },
      },
    );

    if (!response.ok) {
      throw new Error("Failed to load dashboard");
    }

    const data = await response.json();

    // Cards

    document.getElementById("totalMembers").textContent = data.totalMembers;

    document.getElementById("activeSubscriptions").textContent =
      data.activeSubscriptions;

    document.getElementById("todayRevenue").textContent =
      data.todayRevenue + " EGP";

    document.getElementById("todayAttendance").textContent =
      data.todayAttendanceCount;

    // Subscription Stats

    document.getElementById("frozenSubscriptions").textContent =
      data.frozenSubscriptions;

    document.getElementById("scheduledSubscriptions").textContent =
      data.scheduledSubscriptions;

    document.getElementById("expiredSubscriptions").textContent =
      data.expiredSubscriptions;

    // Payments

    document.getElementById("paidPayments").textContent =
      data.paidPaymentsCount;

    document.getElementById("pendingPayments").textContent =
      data.pendingPaymentsCount;

    // Popular Plan

    document.getElementById("popularPlan").textContent = data.mostPopularPlan;

    document.getElementById("popularPlanCount").textContent =
      `${data.mostPopularPlanSubscriptionsCount} subscriptions`;

    loadCharts(data);
  } catch (error) {
    console.log(error);
  }
}

function loadCharts(data) {
  // Revenue Chart

  new Chart(document.getElementById("revenueChart"), {
    type: "bar",

    data: {
      labels: ["Today", "This Month", "Total"],

      datasets: [
        {
          label: "Revenue",

          data: [data.todayRevenue, data.thisMonthRevenue, data.totalRevenue],
        },
      ],
    },
  });


  // Attendance Chart

  new Chart(document.getElementById("attendanceChart"), {
    type: "doughnut",

    data: {
      labels: ["Today", "This Week"],

      datasets: [
        {
          data: [data.todayAttendanceCount, data.thisWeekAttendanceCount],
        },
      ],
    },
  });
}

function logout() {
  localStorage.removeItem("accessToken");
  localStorage.removeItem("refreshToken");

  window.location.href = "../auth/index.html";
}

loadDashboardStats();

