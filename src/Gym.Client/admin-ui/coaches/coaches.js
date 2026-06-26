const token = localStorage.getItem("accessToken");

if (!token) {
  window.location.href = "../auth/index.html";
}

const coachesGrid = document.getElementById("coachesGrid");

const searchInput = document.getElementById("searchInput");
const sortByFilter = document.getElementById("sortByFilter");
const sortDirectionFilter = document.getElementById("sortDirectionFilter");
const DEFAULT_AVATAR =
  "https://ui-avatars.com/api/?name=User&background=2d2d2d&color=ffffff&size=128";

let coaches = [];
let currentPage = 1;
const pageSize = 10;
let totalCount = 0;

// Load Coaches

async function loadCoaches(page = currentPage, search = searchInput.value) {
  const loadingSpinner = document.getElementById("loadingSpinner");

  const emptyState = document.getElementById("emptyState");

  loadingSpinner.classList.remove("d-none");

  coachesGrid.innerHTML = "";

  try {
    const query = new URLSearchParams();
    query.set("pageNumber", String(page));
    query.set("pageSize", String(pageSize));
    query.set("sortDirection", sortDirectionFilter.value || "asc");

    if (search.trim()) query.set("searchTerm", search.trim());
    if (sortByFilter.value) query.set("sortBy", sortByFilter.value);

    const response = await fetch(
      `http://localhost:8080/api/v1/coaches?${query.toString()}`,
      {
        headers: {
          Authorization: `Bearer ${token}`,
        },
      },
    );

    const data = await response.json();

    coaches = data.items || [];
    currentPage = data.pageNumber || page;
    totalCount = data.totalCount || 0;

    renderCoaches();
    renderPagination();

    if (coaches.length === 0) {
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
    coachesGrid.after(container);
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
  loadCoaches(page, searchInput.value);
}

// Render

function renderCoaches() {
  coachesGrid.innerHTML = "";

  coaches.forEach((coach) => {
    const fullName =
      `${coach.firstName || ""} ${coach.lastName || ""}`.trim() || "Unknown";
    const imageSrc = coach.imageUrl || DEFAULT_AVATAR;

    coachesGrid.innerHTML += `

            <div class="coach-card">

                <img
                    src="${imageSrc}"

                    class="coach-image"

                    onerror="
                        this.src='${DEFAULT_AVATAR}'
                    "
                >

                <div class="coach-name">

                    ${fullName}

                </div>

                <div class="coach-phone">

                    ${coach.phoneNumber || "-"}

                </div>

                <div class="card-actions">

                    <button
                        class="btn-action btn-view"
                        onclick="showCoach(${coach.coachId})"
                    >

                        <i class="bi bi-eye-fill"></i>

                    </button>

                    <button
                        class="btn-action btn-delete"
                        onclick="deleteCoach(${coach.coachId})"
                    >

                        <i class="bi bi-trash-fill"></i>

                    </button>

                </div>

            </div>
        `;
  });
}

// Add Coach

document
  .getElementById("addCoachForm")
  .addEventListener("submit", async (e) => {
    e.preventDefault();

    try {
      let imagePath = "";

      const imageFile = document.getElementById("image").files[0];

      if (imageFile) {
        const formData = new FormData();

        formData.append("file", imageFile);

        const uploadResponse = await fetch(
          "http://localhost:8080/api/v1/images/UploadImage",
          {
            method: "POST",
            body: formData,
          },
        );

        const uploadData = await uploadResponse.json();

        imagePath = uploadData.path;
      }

      const body = {
        firstName: document.getElementById("firstName").value,

        lastName: document.getElementById("lastName").value,

        email: document.getElementById("email").value,

        password: document.getElementById("password").value,

        phoneNumber: document.getElementById("phoneNumber").value,

        imageUrl: imagePath,
      };

      const response = await fetch("http://localhost:8080/api/v1/coaches", {
        method: "POST",

        headers: {
          "Content-Type": "application/json",

          Authorization: `Bearer ${token}`,
        },

        body: JSON.stringify(body),
      });

      if (!response.ok) {
        await throwApiError(response, "Create coach failed");
      }

      bootstrap.Modal.getInstance(
        document.getElementById("addCoachModal"),
      ).hide();

      document.getElementById("addCoachForm").reset();

      loadCoaches();
    } catch (error) {
      console.log(error);

      showToast(error.message, "error");
    }
  });

// Show Coach

function showCoach(id) {
  const coach = coaches.find((c) => c.coachId === id);

  document.getElementById("viewImage").src =
    coach.imageUrl || DEFAULT_AVATAR;

  document.getElementById("viewName").textContent =
    `${coach.firstName} ${coach.lastName}`;

  document.getElementById("viewEmail").textContent = coach.email || "-";

  document.getElementById("viewPhone").textContent = coach.phoneNumber || "-";

  new bootstrap.Modal(document.getElementById("viewCoachModal")).show();
}

// Delete

async function deleteCoach(id) {
  const confirmed = await showConfirm("Delete Coach", "Are you sure?");

  if (!confirmed) return;

  try {
    const response = await fetch(
      `http://localhost:8080/api/v1/coaches/${id}`,
      {
        method: "DELETE",

        headers: {
          Authorization: `Bearer ${token}`,
        },
      },
    );

    if (!response.ok) {
      await throwApiError(response, "Delete failed");
    }

    loadCoaches();
  } catch (error) {
    console.log(error);
  }
}

// Search

searchInput.addEventListener("input", () => {
  currentPage = 1;
  loadCoaches(1, searchInput.value);
});

sortByFilter.addEventListener("change", () => {
  currentPage = 1;
  loadCoaches(1, searchInput.value);
});

sortDirectionFilter.addEventListener("change", () => {
  currentPage = 1;
  loadCoaches(1, searchInput.value);
});

// Logout

function logout() {
  localStorage.removeItem("accessToken");

  localStorage.removeItem("refreshToken");

  window.location.href = "../auth/index.html";
}

// Init

loadCoaches();


