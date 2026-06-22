const token = localStorage.getItem("accessToken");

if (!token) {
  window.location.href = "../auth/index.html";
}

const tableBody = document.getElementById("attendanceTable");
const searchInput = document.getElementById("searchInput");
const DEFAULT_AVATAR =
  "https://ui-avatars.com/api/?name=User&background=2d2d2d&color=ffffff&size=96";

let activeMembers = [];
let filteredMembers = [];
let checkedInToday = new Set();
let currentPage = 1;
const pageSize = 10;

async function loadAttendanceBoard() {
  const loadingSpinner = document.getElementById("loadingSpinner");
  const emptyState = document.getElementById("emptyState");
  loadingSpinner.classList.remove("d-none");
  emptyState.classList.add("d-none");

  try {
    await Promise.all([loadActiveMembers(), loadTodayCheckIns()]);
    applySearch();
    updateStats();
  } catch (error) {
    showToast(error.message, "error");
  } finally {
    loadingSpinner.classList.add("d-none");
  }
}

async function loadActiveMembers() {
  const response = await fetch("http://localhost:8080/api/v1/members/active", {
    headers: {
      Authorization: `Bearer ${token}`,
    },
  });

  if (!response.ok) {
    await throwApiError(response, "Failed to load active members");
  }

  activeMembers = await response.json();
}

async function loadTodayCheckIns() {
  const today = new Date().toISOString().split("T")[0];
  const query = new URLSearchParams({
    pageNumber: "1",
    pageSize: "500",
    dateFrom: today,
    dateTo: today,
    sortDirection: "desc",
  });

  const response = await fetch(
    `http://localhost:8080/api/v1/attendances?${query.toString()}`,
    {
      headers: {
        Authorization: `Bearer ${token}`,
      },
    },
  );

  if (!response.ok) {
    await throwApiError(response, "Failed to load today attendance");
  }

  const data = await response.json();
  checkedInToday = new Set((data.items || []).map((x) => x.memberId));
}

function applySearch() {
  const term = searchInput.value.trim().toLowerCase();

  filteredMembers = activeMembers.filter((member) => {
    if (!term) return true;
    const fullName = `${member.firstName || ""} ${member.lastName || ""}`
      .trim()
      .toLowerCase();
    return (
      String(member.memberId).includes(term) ||
      fullName.includes(term) ||
      (member.phoneNumber || "").toLowerCase().includes(term)
    );
  });

  currentPage = 1;
  renderAttendanceBoard();
  renderPagination();
}

function renderAttendanceBoard() {
  const emptyState = document.getElementById("emptyState");
  tableBody.innerHTML = "";

  const start = (currentPage - 1) * pageSize;
  const pageMembers = filteredMembers.slice(start, start + pageSize);

  if (pageMembers.length === 0) {
    emptyState.classList.remove("d-none");
    return;
  }

  emptyState.classList.add("d-none");

  pageMembers.forEach((member) => {
    const fullName =
      `${member.firstName || ""} ${member.lastName || ""}`.trim() || "Unknown";
    const isChecked = checkedInToday.has(member.memberId);

    tableBody.innerHTML += `
      <tr>
        <td>${member.memberId}</td>
        <td>
          <img
            src="${member.imageUrl || DEFAULT_AVATAR}"
            class="member-thumb"
            onerror="this.src='${DEFAULT_AVATAR}'"
          >
        </td>
        <td>${fullName}</td>
        <td>${member.phoneNumber || "-"}</td>
        <td>
          <span class="status ${isChecked ? "CheckedIn" : "PendingCheckIn"}">
            ${isChecked ? "Checked In" : "Not Checked In"}
          </span>
        </td>
        <td>
          <button
            class="btn checkin-btn btn-sm"
            ${isChecked ? "disabled" : ""}
            onclick="checkInMember(${member.memberId})"
          >
            ${isChecked ? "Checked" : "Check In"}
          </button>
        </td>
      </tr>
    `;
  });
}

function ensurePaginationContainer() {
  let container = document.getElementById("paginationContainer");

  if (!container) {
    container = document.createElement("div");
    container.id = "paginationContainer";
    container.className =
      "d-flex justify-content-between align-items-center mt-3";
    document.querySelector(".table-container").after(container);
  }

  return container;
}

function renderPagination() {
  const container = ensurePaginationContainer();
  const totalPages = Math.max(1, Math.ceil(filteredMembers.length / pageSize));
  const disablePrev = currentPage <= 1 ? "disabled" : "";
  const disableNext = currentPage >= totalPages ? "disabled" : "";

  container.innerHTML = `
    <button class="btn btn-outline-light btn-sm" ${disablePrev} onclick="changePage(${currentPage - 1})">Previous</button>
    <span class="text-light">Page ${currentPage} of ${totalPages} (${filteredMembers.length} members)</span>
    <button class="btn btn-outline-light btn-sm" ${disableNext} onclick="changePage(${currentPage + 1})">Next</button>
  `;
}

function changePage(page) {
  const totalPages = Math.max(1, Math.ceil(filteredMembers.length / pageSize));
  if (page < 1 || page > totalPages) return;
  currentPage = page;
  renderAttendanceBoard();
  renderPagination();
}

async function checkInMember(memberId) {
  if (checkedInToday.has(memberId)) return;

  try {
    const response = await fetch(
      `http://localhost:8080/api/v1/attendances/${memberId}/check-in`,
      {
        method: "POST",
        headers: {
          Authorization: `Bearer ${token}`,
        },
      },
    );

    if (!response.ok) {
      await throwApiError(response, "Check in failed");
    }

    checkedInToday.add(memberId);
    showToast("Check in successful", "success");
    renderAttendanceBoard();
    updateStats();
  } catch (error) {
    showToast(error.message, "error");
  }
}

function updateStats() {
  document.getElementById("todayAttendance").textContent = checkedInToday.size;
  document.getElementById("activeMembers").textContent = activeMembers.length;
}

searchInput.addEventListener("input", applySearch);

function logout() {
  localStorage.removeItem("accessToken");
  localStorage.removeItem("refreshToken");
  window.location.href = "../auth/index.html";
}

loadAttendanceBoard();
