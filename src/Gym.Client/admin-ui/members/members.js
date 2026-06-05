const token = localStorage.getItem("accessToken");

if (!token) {
  window.location.href = "../auth/index.html";
}

const tableBody = document.getElementById("membersTable");

const searchInput = document.getElementById("searchInput");
const sortByFilter = document.getElementById("sortByFilter");
const sortDirectionFilter = document.getElementById("sortDirectionFilter");
const memberSubActionForm = document.getElementById("memberSubActionForm");
const imageInput = document.getElementById("image");
const imageFileName = document.getElementById("imageFileName");
const imagePreview = document.getElementById("imagePreview");

const addMemberForm = document.getElementById("addMemberForm");
const DEFAULT_AVATAR =
  "https://ui-avatars.com/api/?name=User&background=2d2d2d&color=ffffff&size=128";

let members = [];
let currentPage = 1;
const pageSize = 10;
let totalCount = 0;
let currentMemberSubscriptionId = null;
let subscriptionPlans = [];

function bindImageUploadUI() {
  if (!imageInput || !imageFileName || !imagePreview) return;

  imageInput.addEventListener("change", () => {
    const file = imageInput.files?.[0];
    if (!file) {
      imageFileName.textContent = "No file chosen";
      imagePreview.classList.add("d-none");
      imagePreview.removeAttribute("src");
      return;
    }

    imageFileName.textContent = file.name;
    imagePreview.src = URL.createObjectURL(file);
    imagePreview.classList.remove("d-none");
  });
}

function resetImageUploadUI() {
  if (!imageFileName || !imagePreview) return;
  imageFileName.textContent = "No file chosen";
  imagePreview.classList.add("d-none");
  imagePreview.removeAttribute("src");
}

function formatDate(value) {
  if (!value) return "-";
  const date = new Date(value);
  if (Number.isNaN(date.getTime())) return value;
  return date.toLocaleDateString();
}

function renderSubscriptionHistory(historyItems = []) {
  const table = document.getElementById("subscriptionHistoryTable");
  const empty = document.getElementById("subscriptionHistoryEmpty");
  table.innerHTML = "";

  if (!historyItems.length) {
    empty.classList.remove("d-none");
    return;
  }

  empty.classList.add("d-none");
  historyItems.forEach((item) => {
    table.innerHTML += `
      <tr>
        <td>${item.planName || "-"}</td>
        <td>${item.status || "-"}</td>
        <td>${formatDate(item.startDate)}</td>
        <td>${formatDate(item.endDate)}</td>
        <td>${item.priceSnapshot != null ? `${item.priceSnapshot} EGP` : "-"}</td>
      </tr>
    `;
  });
}

function renderPaymentsHistory(payments = []) {
  const table = document.getElementById("memberPaymentsTable");
  const empty = document.getElementById("memberPaymentsEmpty");
  table.innerHTML = "";

  if (!payments.length) {
    empty.classList.remove("d-none");
    return;
  }

  empty.classList.add("d-none");
  payments.forEach((item) => {
    table.innerHTML += `
      <tr>
        <td>${item.planName || "-"}</td>
        <td>${item.amount != null ? `${item.amount} EGP` : "-"}</td>
        <td>${item.paymentMethod || "-"}</td>
        <td>${item.status || "-"}</td>
        <td>${formatDate(item.paidAtUtc)}</td>
      </tr>
    `;
  });
}

// Load Members

async function loadMembers(page = currentPage, search = searchInput.value) {
  const loadingSpinner = document.getElementById("loadingSpinner");

  const emptyState = document.getElementById("emptyState");

  const tableContainer = document.getElementById("tableContainer");

  loadingSpinner.classList.remove("d-none");

  tableContainer.classList.add("d-none");

  emptyState.classList.add("d-none");

  try {
    const query = new URLSearchParams();
    query.set("pageNumber", String(page));
    query.set("pageSize", String(pageSize));
    query.set("sortDirection", sortDirectionFilter.value || "asc");

    if (search.trim()) query.set("searchTerm", search.trim());
    if (sortByFilter.value) query.set("sortBy", sortByFilter.value);

    const response = await fetch(
      `https://localhost:7022/api/v1/members?${query.toString()}`,
      {
        headers: {
          Authorization: `Bearer ${token}`,
        },
      },
    );

    if (!response.ok) {
      throw new Error("Failed to load members");
    }

    const data = await response.json();

    members = data.items || [];
    currentPage = data.pageNumber || page;
    totalCount = data.totalCount || 0;

    renderMembers();
    renderPagination();

    if (members.length === 0) {
      emptyState.classList.remove("d-none");
    } else {
      tableContainer.classList.remove("d-none");
    }
  } catch (error) {
    console.log(error);
  } finally {
    loadingSpinner.classList.add("d-none");
  }
}

// Render Members

function renderMembers() {
  tableBody.innerHTML = "";

  members.forEach((member) => {
    const fullName =
      `${member.firstName || ""} ${member.lastName || ""}`.trim() || "Unknown";
    const imageSrc = member.imageUrl || DEFAULT_AVATAR;

    tableBody.innerHTML += `

            <tr>

                <td>

                    <img
                        src="${imageSrc}"

                        class="member-image"

                        onerror="
                            this.src='${DEFAULT_AVATAR}'
                        "
                    >

                </td>

                <td>
                    ${fullName}
                </td>

                <td>
                    ${member.phoneNumber || "-"}
                </td>

                <td>
                    ${
                      member.joinDate
                        ? new Date(member.joinDate).toLocaleDateString()
                        : "-"
                    }
                </td>

                <td class="actions-col">
                    <div class="action-buttons">
                        <button
                            class="btn-action btn-sub-add"
                            onclick="openMemberSubscriptionAction(${member.memberId}, 'add')"
                        >
                            <i class="bi bi-plus-lg"></i>
                        </button>

                        <button
                            class="btn-action btn-sub-renew"
                            onclick="openMemberSubscriptionAction(${member.memberId}, 'renew')"
                        >
                            <i class="bi bi-arrow-repeat"></i>
                        </button>
                    </div>
                </td>

                <td class="actions-col">

                    <div class="action-buttons">

                        <button
                            class="btn-action btn-view"
                            onclick="showMemberDetails(${member.memberId})"
                        >

                            <i class="bi bi-eye-fill"></i>

                        </button>

                        <button
                            class="btn-action btn-edit"
                            onclick="openEditModal(${member.memberId})"
                        >

                            <i class="bi bi-pencil-fill"></i>

                        </button>

                        <button
                            class="btn-action btn-subscription"
                            onclick="showCurrentSubscription(${member.memberId})"
                        >

                            <i class="bi bi-calendar-check-fill"></i>

                        </button>

                        <button
                            class="btn-action btn-delete"
                            onclick="deleteMember(${member.memberId})"
                        >

                            <i class="bi bi-trash-fill"></i>

                        </button>

                    </div>

                </td>

            </tr>
        `;
  });
}

async function loadSubscriptionPlans() {
  try {
    const response = await fetch(
      "https://localhost:7022/api/v1/plans?pageNumber=1&pageSize=100",
      {
        headers: {
          Authorization: `Bearer ${token}`,
        },
      },
    );

    if (!response.ok) {
      await throwApiError(response, "Failed to load plans");
    }

    const data = await response.json();
    subscriptionPlans = data.items || [];

    const planSelect = document.getElementById("memberSubPlanId");
    planSelect.innerHTML = "";
    subscriptionPlans.forEach((plan) => {
      planSelect.innerHTML += `<option value="${plan.planId}">${plan.title}</option>`;
    });
  } catch (error) {
    showToast(error.message, "error");
  }
}

function openMemberSubscriptionAction(memberId, mode) {
  document.getElementById("memberSubActionMemberId").value = memberId;
  document.getElementById("memberSubActionMode").value = mode;

  const title = document.getElementById("memberSubActionTitle");
  const submit = document.getElementById("memberSubActionSubmit");
  const startDateWrap = document.getElementById("memberSubStartDateWrap");
  const startDateInput = document.getElementById("memberSubStartDate");

  if (mode === "renew") {
    title.textContent = "Renew Subscription";
    submit.textContent = "Renew Subscription";
    startDateWrap.classList.add("d-none");
    startDateInput.required = false;
  } else {
    title.textContent = "Add Subscription";
    submit.textContent = "Create Subscription";
    startDateWrap.classList.remove("d-none");
    startDateInput.required = true;
    if (!startDateInput.value) {
      startDateInput.value = new Date().toISOString().split("T")[0];
    }
  }

  new bootstrap.Modal(document.getElementById("memberSubActionModal")).show();
}

function ensurePaginationContainer() {
  let container = document.getElementById("paginationContainer");

  if (!container) {
    container = document.createElement("div");
    container.id = "paginationContainer";
    container.className = "d-flex justify-content-between align-items-center mt-3";
    document.getElementById("tableContainer").after(container);
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
  loadMembers(page, searchInput.value);
}

// Add Member

addMemberForm.addEventListener("submit", async (e) => {
  e.preventDefault();

  try {
    let imagePath = "";

    // Upload image

    const imageFile = document.getElementById("image").files[0];

    if (imageFile) {
      const formData = new FormData();

      formData.append("file", imageFile);

      const uploadResponse = await fetch(
        "https://localhost:7022/api/v1/images/UploadImage",
        {
          method: "POST",
          body: formData,
        },
      );

      const uploadData = await uploadResponse.json();

      imagePath = uploadData.path;
    }

    // Create member

    const body = {
      firstName: document.getElementById("firstName").value,

      lastName: document.getElementById("lastName").value,

      email: document.getElementById("email").value,

      password: document.getElementById("password").value,

      phoneNumber: document.getElementById("phoneNumber").value,

      dateOfBirth: document.getElementById("dateOfBirth").value
        ? document.getElementById("dateOfBirth").value + "T00:00:00Z"
        : null,

      joinDate: document.getElementById("joinDate").value
        ? document.getElementById("joinDate").value + "T00:00:00Z"
        : null,

      notes: document.getElementById("notes").value,

      imageUrl: imagePath,
    };

    const response = await fetch("https://localhost:7022/api/v1/members", {
      method: "POST",

      headers: {
        "Content-Type": "application/json",

        Authorization: `Bearer ${token}`,
      },

      body: JSON.stringify(body),
    });

    if (!response.ok) {
      await throwApiError(response, "Create member failed");
    }

    bootstrap.Modal.getInstance(
      document.getElementById("addMemberModal"),
    ).hide();

    addMemberForm.reset();
    resetImageUploadUI();

    loadMembers();
  } catch (error) {
    console.log(error);

    showToast(error.message, "error");
  }
});

// Delete Member

async function deleteMember(id) {
  const confirmed = await showConfirm("Delete Member", "Are you sure?");

  if (!confirmed) return;

  try {
    const response = await fetch(
      `https://localhost:7022/api/v1/members/${id}`,
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

    loadMembers();
  } catch (error) {
    console.log(error);
  }
}

async function showCurrentSubscription(memberId) {
  try {
    const headers = { Authorization: `Bearer ${token}` };
    const [currentRes, historyRes, paymentsRes] = await Promise.all([
      fetch(`https://localhost:7022/api/v1/subscriptions/member/${memberId}`, {
        headers,
      }),
      fetch(
        `https://localhost:7022/api/v1/subscriptions/member/${memberId}/history`,
        { headers },
      ),
      fetch(`https://localhost:7022/api/v1/payments/member/${memberId}`, {
        headers,
      }),
    ]);

    if (!currentRes.ok) {
      await throwApiError(currentRes, "No current subscription found");
    }

    const sub = await currentRes.json();
    const history = historyRes.ok ? await historyRes.json() : [];
    const payments = paymentsRes.ok ? await paymentsRes.json() : [];

    renderSubscriptionHistory(Array.isArray(history) ? history : []);
    renderPaymentsHistory(Array.isArray(payments) ? payments : []);

    currentMemberSubscriptionId = sub.subscriptionId || null;

    document.getElementById("subPlanName").textContent = sub.planName || "-";
    document.getElementById("subStatus").textContent = sub.status || "-";
    document.getElementById("subStartDate").textContent = formatDate(sub.startDate);
    document.getElementById("subEndDate").textContent = formatDate(sub.endDate);
    document.getElementById("subPrice").textContent =
      sub.priceSnapshot != null ? `${sub.priceSnapshot} EGP` : "-";

    const cancelBtn = document.getElementById("cancelSubBtn");
    const cancellableStatuses = ["Pending", "Active", "Scheduled", "Frozen"];
    if (
      currentMemberSubscriptionId &&
      cancellableStatuses.includes(sub.status || "")
    ) {
      cancelBtn.classList.remove("d-none");
    } else {
      cancelBtn.classList.add("d-none");
    }

    new bootstrap.Modal(document.getElementById("memberSubscriptionModal")).show();
  } catch (error) {
    showToast(error.message, "error");
  }
}

document.getElementById("cancelSubBtn").addEventListener("click", async () => {
  if (!currentMemberSubscriptionId) return;

  const confirmed = await showConfirm(
    "Cancel Subscription",
    "Are you sure you want to cancel this subscription?",
  );
  if (!confirmed) return;

  try {
    const response = await fetch(
      `https://localhost:7022/api/v1/subscriptions/${currentMemberSubscriptionId}/cancel`,
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

    bootstrap.Modal.getInstance(
      document.getElementById("memberSubscriptionModal"),
    )?.hide();
    showToast("Subscription cancelled", "success");
  } catch (error) {
    showToast(error.message, "error");
  }
});

memberSubActionForm.addEventListener("submit", async (e) => {
  e.preventDefault();

  const memberId = Number(
    document.getElementById("memberSubActionMemberId").value,
  );
  const planId = Number(document.getElementById("memberSubPlanId").value);
  const mode = document.getElementById("memberSubActionMode").value;
  const startDate = document.getElementById("memberSubStartDate").value;

  const endpoint =
    mode === "renew"
      ? "https://localhost:7022/api/v1/subscriptions/renew"
      : "https://localhost:7022/api/v1/subscriptions";

  const body =
    mode === "renew"
      ? {
          memberId,
          planId,
        }
      : {
          memberId,
          planId,
          startDate,
        };

  try {
    const response = await fetch(endpoint, {
      method: "POST",
      headers: {
        "Content-Type": "application/json",
        Authorization: `Bearer ${token}`,
      },
      body: JSON.stringify(body),
    });

    if (!response.ok) {
      await throwApiError(
        response,
        mode === "renew" ? "Renew subscription failed" : "Create subscription failed",
      );
    }

    bootstrap.Modal.getInstance(
      document.getElementById("memberSubActionModal"),
    )?.hide();
    showToast(
      mode === "renew" ? "Subscription renewed" : "Subscription created",
      "success",
    );
  } catch (error) {
    showToast(error.message, "error");
  }
});

// Show Details

async function showMemberDetails(id) {
  try {
    const response = await fetch(
      `https://localhost:7022/api/v1/members/${id}`,
      {
        headers: {
          Authorization: `Bearer ${token}`,
        },
      },
    );

    if (!response.ok) {
      await throwApiError(response, "Failed to load member");
    }

    const member = await response.json();

    document.getElementById("viewImage").src = member.imageUrl || DEFAULT_AVATAR;
    document.getElementById("viewImage").onerror = function imageFallback() {
      this.src = DEFAULT_AVATAR;
    };

    document.getElementById("viewName").textContent =
      `${member.firstName} ${member.lastName}`;

    document.getElementById("viewEmail").textContent = member.email || "-";

    document.getElementById("viewPhone").textContent = member.phoneNumber;

    document.getElementById("viewDob").textContent = member.dateOfBirth
      ? new Date(member.dateOfBirth).toLocaleDateString()
      : "-";

    document.getElementById("viewJoinDate").textContent = member.joinDate
      ? new Date(member.joinDate).toLocaleDateString()
      : "-";

    document.getElementById("viewNotes").textContent = member.notes || "-";

    new bootstrap.Modal(document.getElementById("viewMemberModal")).show();
  } catch (error) {
    console.log(error);
  }
}

// Open Edit Modal

async function openEditModal(id) {
  try {
    const response = await fetch(
      `https://localhost:7022/api/v1/members/${id}`,
      {
        headers: {
          Authorization: `Bearer ${token}`,
        },
      },
    );

    if (!response.ok) {
      await throwApiError(response, "Failed to load member");
    }

    const member = await response.json();

    document.getElementById("editMemberId").value = member.memberId;

    document.getElementById("editFirstName").value = member.firstName;

    document.getElementById("editLastName").value = member.lastName;

    document.getElementById("editPhoneNumber").value = member.phoneNumber;

    document.getElementById("editDateOfBirth").value =
      member.dateOfBirth?.split("T")[0] || "";

    document.getElementById("editJoinDate").value =
      member.joinDate?.split("T")[0] || "";

    document.getElementById("editNotes").value = member.notes || "";

    new bootstrap.Modal(document.getElementById("editMemberModal")).show();
  } catch (error) {
    console.log(error);
  }
}

// Edit Submit

document
  .getElementById("editMemberForm")
  .addEventListener("submit", async (e) => {
    e.preventDefault();

    try {
      const id = document.getElementById("editMemberId").value;

      const body = {
        firstName: document.getElementById("editFirstName").value,

        lastName: document.getElementById("editLastName").value,

        phoneNumber: document.getElementById("editPhoneNumber").value,

        dateOfBirth: document.getElementById("editDateOfBirth").value
          ? document.getElementById("editDateOfBirth").value + "T00:00:00Z"
          : null,

        joinDate: document.getElementById("editJoinDate").value
          ? document.getElementById("editJoinDate").value + "T00:00:00Z"
          : null,

        notes: document.getElementById("editNotes").value,
      };

      const response = await fetch(
        `https://localhost:7022/api/v1/members/${id}`,
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
        document.getElementById("editMemberModal"),
      ).hide();

      loadMembers();
    } catch (error) {
      console.log(error);

      showToast(error.message, "error");
    }
  });

// Search

searchInput.addEventListener("input", () => {
  currentPage = 1;
  loadMembers(1, searchInput.value);
});

sortByFilter.addEventListener("change", () => {
  currentPage = 1;
  loadMembers(1, searchInput.value);
});

sortDirectionFilter.addEventListener("change", () => {
  currentPage = 1;
  loadMembers(1, searchInput.value);
});

// Logout

function logout() {
  localStorage.removeItem("accessToken");

  localStorage.removeItem("refreshToken");

  window.location.href = "../auth/index.html";
}

// Init

loadMembers();
loadSubscriptionPlans();
bindImageUploadUI();

