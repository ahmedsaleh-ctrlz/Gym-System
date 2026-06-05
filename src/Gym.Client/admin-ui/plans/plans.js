const token = localStorage.getItem("accessToken");

if (!token) {
  window.location.href = "../auth/index.html";
}

const plansGrid = document.getElementById("plansGrid");

const searchInput = document.getElementById("searchInput");
const sortByFilter = document.getElementById("sortByFilter");
const sortDirectionFilter = document.getElementById("sortDirectionFilter");

let plans = [];
let currentPage = 1;
const pageSize = 10;
let totalCount = 0;

// Load Plans

async function loadPlans(page = currentPage, search = searchInput.value) {
  const loadingSpinner = document.getElementById("loadingSpinner");

  const emptyState = document.getElementById("emptyState");

  loadingSpinner.classList.remove("d-none");

  plansGrid.innerHTML = "";

  try {
    const query = new URLSearchParams();
    query.set("pageNumber", String(page));
    query.set("pageSize", String(pageSize));
    query.set("sortDirection", sortDirectionFilter.value || "asc");

    if (search.trim()) query.set("searchTerm", search.trim());
    if (sortByFilter.value) query.set("sortBy", sortByFilter.value);

    const response = await fetch(
      `https://localhost:7022/api/v1/plans?${query.toString()}`,
      {
        headers: {
          Authorization: `Bearer ${token}`,
        },
      },
    );

    const data = await response.json();

    plans = data.items || [];
    currentPage = data.pageNumber || page;
    totalCount = data.totalCount || 0;

    renderPlans();
    renderPagination();

    if (plans.length === 0) {
      emptyState.classList.remove("d-none");
    } else {
      emptyState.classList.add("d-none");
    }
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
    plansGrid.after(container);
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
  loadPlans(page, searchInput.value);
}

// Render

function renderPlans() {
  plansGrid.innerHTML = "";

  plans.forEach((plan) => {
    plansGrid.innerHTML += `

            <div class="plan-card">

                <div class="plan-title">
                    ${plan.title}
                </div>

                <div class="plan-description">
                    ${plan.description || "-"}
                </div>

                <div class="plan-price">
                    ${plan.cost} EGP
                </div>

                <div class="plan-info">

                    <span>Duration</span>

                    <strong>
                        ${plan.durationInDays} Days
                    </strong>

                </div>

                <div class="plan-info">

                    <span>Freeze Count</span>

                    <strong>
                        ${plan.allowedFreezeCount}
                    </strong>

                </div>

                <div class="plan-info">

                    <span>Max Freeze</span>

                    <strong>
                        ${plan.maxTotalFreezeDays} Days
                    </strong>

                </div>

                <div class="plan-actions">

                    <button
                        class="btn-action btn-edit"
                        onclick="openEditModal(${plan.planId})"
                    >

                        <i class="bi bi-pencil-fill"></i>

                    </button>

                    <button
                        class="btn-action btn-delete"
                        onclick="deletePlan(${plan.planId})"
                    >

                        <i class="bi bi-trash-fill"></i>

                    </button>

                </div>

            </div>
        `;
  });
}

// Add Plan

document.getElementById("addPlanForm").addEventListener("submit", async (e) => {
  e.preventDefault();

  try {
    const body = {
      title: document.getElementById("title").value,

      description: document.getElementById("description").value,

      cost: Number(document.getElementById("cost").value),

      durationInDays: Number(document.getElementById("durationInDays").value),

      allowedFreezeCount: Number(
        document.getElementById("allowedFreezeCount").value,
      ),

      maxTotalFreezeDays: Number(
        document.getElementById("maxTotalFreezeDays").value,
      ),
    };

    const response = await fetch("https://localhost:7022/api/v1/plans", {
      method: "POST",

      headers: {
        "Content-Type": "application/json",

        Authorization: `Bearer ${token}`,
      },

      body: JSON.stringify(body),
    });

    if (!response.ok) {
      await throwApiError(response, "Create plan failed");
    }

    bootstrap.Modal.getInstance(document.getElementById("addPlanModal")).hide();

    document.getElementById("addPlanForm").reset();

    loadPlans();
  } catch (error) {
    console.log(error);

    showToast(error.message, "error");
  }
});

// Delete

async function deletePlan(id) {
  const confirmed = await showConfirm("Delete Plan", "Are you sure?");

  if (!confirmed) return;

  try {
    const response = await fetch(`https://localhost:7022/api/v1/plans/${id}`, {
      method: "DELETE",

      headers: {
        Authorization: `Bearer ${token}`,
      },
    });

    if (!response.ok) {
      await throwApiError(response, "Delete failed");
    }

    loadPlans();
  } catch (error) {
    console.log(error);
  }
}

// Open Edit

async function openEditModal(id) {
  const plan = plans.find((p) => p.planId === id);

  document.getElementById("editPlanId").value = plan.planId;

  document.getElementById("editTitle").value = plan.title;

  document.getElementById("editDescription").value = plan.description;

  document.getElementById("editCost").value = plan.cost;

  document.getElementById("editDurationInDays").value = plan.durationInDays;

  document.getElementById("editAllowedFreezeCount").value =
    plan.allowedFreezeCount;

  document.getElementById("editMaxTotalFreezeDays").value =
    plan.maxTotalFreezeDays;

  new bootstrap.Modal(document.getElementById("editPlanModal")).show();
}

// Edit Submit

document
  .getElementById("editPlanForm")
  .addEventListener("submit", async (e) => {
    e.preventDefault();

    try {
      const id = document.getElementById("editPlanId").value;

      const body = {
        title: document.getElementById("editTitle").value,

        description: document.getElementById("editDescription").value,

        cost: Number(document.getElementById("editCost").value),

        durationInDays: Number(
          document.getElementById("editDurationInDays").value,
        ),

        allowedFreezeCount: Number(
          document.getElementById("editAllowedFreezeCount").value,
        ),

        maxTotalFreezeDays: Number(
          document.getElementById("editMaxTotalFreezeDays").value,
        ),
      };

      const response = await fetch(
        `https://localhost:7022/api/v1/plans/${id}`,
        {
          method: "PUT",

          headers: {
            "Content-Type": "application/json",

            Authorization: `Bearer ${token}`,
          },

          body: JSON.stringify(body),
        },
      );

      if (!response.ok) {
        await throwApiError(response, "Update failed");
      }

      bootstrap.Modal.getInstance(
        document.getElementById("editPlanModal"),
      ).hide();

      loadPlans();
    } catch (error) {
      console.log(error);

      showToast(error.message, "error");
    }
  });

// Search

searchInput.addEventListener("input", () => {
  currentPage = 1;
  loadPlans(1, searchInput.value);
});

sortByFilter.addEventListener("change", () => {
  currentPage = 1;
  loadPlans(1, searchInput.value);
});

sortDirectionFilter.addEventListener("change", () => {
  currentPage = 1;
  loadPlans(1, searchInput.value);
});

// Logout

function logout() {
  localStorage.removeItem("accessToken");

  localStorage.removeItem("refreshToken");

  window.location.href = "../auth/index.html";
}

loadPlans();

