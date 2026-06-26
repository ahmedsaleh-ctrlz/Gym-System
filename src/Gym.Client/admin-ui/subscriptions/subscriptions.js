const token = localStorage.getItem("accessToken");

if (!token) {
  window.location.href = "../auth/index.html";
}

const tableBody = document.getElementById("subscriptionsTable");

const searchInput = document.getElementById("searchInput");

const statusFilter = document.getElementById("statusFilter");
const planNameFilter = document.getElementById("planNameFilter");
const startDateFromFilter = document.getElementById("startDateFromFilter");
const startDateToFilter = document.getElementById("startDateToFilter");
const endDateFromFilter = document.getElementById("endDateFromFilter");
const endDateToFilter = document.getElementById("endDateToFilter");
const sortByFilter = document.getElementById("sortByFilter");
const sortDirectionFilter = document.getElementById("sortDirectionFilter");
const freezeSubscriptionForm = document.getElementById("freezeSubscriptionForm");

let subscriptions = [];
let currentPage = 1;
const pageSize = 10;
let totalCount = 0;

// Load Subscriptions

async function loadSubscriptions(page = currentPage) {
  const loadingSpinner = document.getElementById("loadingSpinner");

  loadingSpinner.classList.remove("d-none");

  try {
    const query = new URLSearchParams();
    query.set("pageNumber", String(page));
    query.set("pageSize", String(pageSize));
    query.set("sortDirection", sortDirectionFilter.value || "asc");

    if (searchInput.value.trim()) query.set("searchTerm", searchInput.value.trim());
    if (statusFilter.value) query.set("status", statusFilter.value);
    if (planNameFilter.value.trim()) query.set("planName", planNameFilter.value.trim());
    if (startDateFromFilter.value) query.set("startDateFrom", startDateFromFilter.value);
    if (startDateToFilter.value) query.set("startDateTo", startDateToFilter.value);
    if (endDateFromFilter.value) query.set("endDateFrom", endDateFromFilter.value);
    if (endDateToFilter.value) query.set("endDateTo", endDateToFilter.value);
    if (sortByFilter.value) query.set("sortBy", sortByFilter.value);

    const response = await fetch(
      `http://localhost:8080/api/v1/subscriptions?${query.toString()}`,
      {
        headers: {
          Authorization: `Bearer ${token}`,
        },
      },
    );

    const data = await response.json();

    subscriptions = data.items || [];
    currentPage = data.pageNumber || page;
    totalCount = data.totalCount || 0;

    renderSubscriptions();
    renderPagination();
  } catch (error) {
    console.log(error);
  } finally {
    loadingSpinner.classList.add("d-none");
  }
}

function ensurePaginationContainer() {
  let container = document.getElementById("paginationContainer");

  if (!container) {
    container = document.createElement("div");
    container.id = "paginationContainer";
    container.className = "d-flex justify-content-between align-items-center mt-3";
    document.querySelector(".table-container").after(container);
  }

  return container;
}

function renderPagination() {
  const container = ensurePaginationContainer();
  const totalPages = Math.max(1, Math.ceil(totalCount / pageSize));
  const disablePrev = currentPage <= 1 ? "disabled" : "";
  const disableNext = currentPage >= totalPages ? "disabled" : "";

  container.innerHTML = `
    <button class="btn btn-outline-light btn-sm" ${disablePrev} onclick="changePage(${currentPage - 1})">Previous</button>
    <span class="text-light">Page ${currentPage} of ${totalPages} (${totalCount} items)</span>
    <button class="btn btn-outline-light btn-sm" ${disableNext} onclick="changePage(${currentPage + 1})">Next</button>
  `;
}

function changePage(page) {
  const totalPages = Math.max(1, Math.ceil(totalCount / pageSize));
  if (page < 1 || page > totalPages) return;
  loadSubscriptions(page);
}

// Render

function renderSubscriptions() {
  tableBody.innerHTML = "";

  subscriptions.forEach((sub) => {
    tableBody.innerHTML += `

            <tr>

                <td>
                    ${sub.memberName}
                </td>

                <td>
                    ${sub.planName}
                </td>

                <td>
                    ${sub.priceSnapshot} EGP
                </td>

                <td>
                    ${sub.startDate}
                </td>

                <td>
                    ${sub.endDate}
                </td>

                <td>

                    <span class="status ${sub.status}">
                        ${sub.status}
                    </span>

                </td>

                <td>

                    <div class="action-buttons">

                        <button
                            class="btn-action btn-freeze"
                            onclick="freezeSubscription(${sub.subscriptionId})"
                        >

                            <i class="bi bi-snow"></i>

                        </button>

                        <button
                            class="btn-action btn-cancel"
                            onclick="cancelSubscription(${sub.subscriptionId})"
                        >

                            <i class="bi bi-x-lg"></i>

                        </button>

                    </div>

                </td>

            </tr>
        `;
  });
}

// Freeze

async function freezeSubscription(id) {
  document.getElementById("freezeSubscriptionId").value = id;
  document.getElementById("freezeDaysInput").value = "";
  new bootstrap.Modal(document.getElementById("freezeSubscriptionModal")).show();
}

freezeSubscriptionForm.addEventListener("submit", async (e) => {
  e.preventDefault();

  const subscriptionId = Number(
    document.getElementById("freezeSubscriptionId").value,
  );
  const freezeDays = Number(document.getElementById("freezeDaysInput").value);

  if (!freezeDays || freezeDays < 1) {
    showToast("Freeze days must be at least 1", "error");
    return;
  }

  try {
    const response = await fetch(
      `http://localhost:8080/api/v1/subscriptions/${subscriptionId}/freeze`,
      {
        method: "PUT",

        headers: {
          "Content-Type": "application/json",

          Authorization: `Bearer ${token}`,
        },

        body: JSON.stringify({
          freezeDays: Number(freezeDays),
        }),
      },
    );

    if (!response.ok) {
      await throwApiError(response, "Freeze failed");
    }

    bootstrap.Modal.getInstance(
      document.getElementById("freezeSubscriptionModal"),
    )?.hide();
    showToast("Subscription frozen successfully", "success");
    loadSubscriptions();
  } catch (error) {
    showToast(error.message, "error");
  }
});

// Cancel

async function cancelSubscription(id) {
  const confirmed = await showConfirm("Cancel Subscription", "Are you sure?");

  if (!confirmed) return;

  if (!confirmed) return;

  try {
    const response = await fetch(
      `http://localhost:8080/api/v1/subscriptions/${id}/cancel`,
      {
        method: "PUT",

        headers: {
          Authorization: `Bearer ${token}`,
        },
      },
    );

    if (!response.ok) {
      await throwApiError(response, "Cancel failed");
    }

    loadSubscriptions();
  } catch (error) {
    console.log(error);
  }
}

// Filters

searchInput.addEventListener("input", () => {
  currentPage = 1;
  loadSubscriptions(1);
});

statusFilter.addEventListener("change", () => {
  currentPage = 1;
  loadSubscriptions(1);
});

planNameFilter.addEventListener("input", () => {
  currentPage = 1;
  loadSubscriptions(1);
});

startDateFromFilter.addEventListener("change", () => {
  currentPage = 1;
  loadSubscriptions(1);
});

startDateToFilter.addEventListener("change", () => {
  currentPage = 1;
  loadSubscriptions(1);
});

endDateFromFilter.addEventListener("change", () => {
  currentPage = 1;
  loadSubscriptions(1);
});

endDateToFilter.addEventListener("change", () => {
  currentPage = 1;
  loadSubscriptions(1);
});

sortByFilter.addEventListener("change", () => {
  currentPage = 1;
  loadSubscriptions(1);
});

sortDirectionFilter.addEventListener("change", () => {
  currentPage = 1;
  loadSubscriptions(1);
});

// Logout

function logout() {
  localStorage.removeItem("accessToken");

  localStorage.removeItem("refreshToken");

  window.location.href = "../auth/index.html";
}

// Init

loadSubscriptions();




