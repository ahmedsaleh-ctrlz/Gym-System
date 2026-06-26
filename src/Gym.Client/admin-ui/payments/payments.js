const token = localStorage.getItem("accessToken");

if (!token) {
  window.location.href = "../auth/index.html";
}

const tableBody = document.getElementById("paymentsTable");

const searchInput = document.getElementById("searchInput");
const statusFilter = document.getElementById("statusFilter");
const sortByFilter = document.getElementById("sortByFilter");
const sortDirectionFilter = document.getElementById("sortDirectionFilter");

let payments = [];
let currentPage = 1;
const pageSize = 10;
let totalCount = 0;

// Load Payments

async function loadPayments(page = currentPage, search = searchInput.value) {
  const loadingSpinner = document.getElementById("loadingSpinner");

  const emptyState = document.getElementById("emptyState");

  loadingSpinner.classList.remove("d-none");

  try {
    const query = new URLSearchParams();
    query.set("pageNumber", String(page));
    query.set("pageSize", String(pageSize));
    query.set("sortDirection", sortDirectionFilter.value || "desc");

    if (search.trim()) query.set("searchTerm", search.trim());
    if (statusFilter.value) query.set("status", statusFilter.value);
    if (sortByFilter.value) query.set("sortBy", sortByFilter.value);

    const response = await fetch(
      `http://localhost:8080/api/v1/payments?${query.toString()}`,
      {
        headers: {
          Authorization: `Bearer ${token}`,
        },
      },
    );

    const data = await response.json();

    payments = data.items || [];
    currentPage = data.pageNumber || page;
    totalCount = data.totalCount || 0;

    renderPayments();

    updateStats();
    renderPagination();

    if (payments.length === 0) {
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
  loadPayments(page, searchInput.value);
}

// Render

function renderPayments() {
  tableBody.innerHTML = "";

  payments.forEach((payment) => {
    tableBody.innerHTML += `

            <tr>

                <td>

                    ${payment.memberName}

                </td>

                <td>

                    ${payment.planName}

                </td>

                <td>

                    ${payment.amount} EGP

                </td>

                <td>

                    ${payment.paymentMethod || "-"}

                </td>

                <td>

                    <span class="status ${payment.status}">

                        ${payment.status}

                    </span>

                </td>

                <td>

                    ${
                      payment.paidAtUtc
                        ? new Date(payment.paidAtUtc).toLocaleDateString()
                        : "-"
                    }

                </td>

                <td>

                    ${
                      payment.status === "Pending"
                        ? `
                        <div class="payment-actions">
                          <button
                              class="btn-pay"
                              onclick="openPayModal(${payment.paymentId})"
                          >

                              Pay

                          </button>
                          <button
                              class="btn-cancel-payment"
                              onclick="cancelPayment(${payment.paymentId})"
                          >

                              Cancel

                          </button>
                        </div>
                        `
                        : `
                        <span>
                            Done
                        </span>
                        `
                    }

                </td>

            </tr>
        `;
  });
}

// Stats

function updateStats() {
  const totalRevenue = payments
    .filter((p) => p.status === "Paid")
    .reduce((sum, p) => sum + p.amount, 0);

  const paidCount = payments.filter((p) => p.status === "Paid").length;

  const pendingCount = payments.filter((p) => p.status === "Pending").length;

  document.getElementById("paymentsCount").textContent = payments.length;

  document.getElementById("totalRevenue").textContent = `${totalRevenue} EGP`;

  document.getElementById("paidPayments").textContent = paidCount;

  document.getElementById("pendingPayments").textContent = pendingCount;
}

// Pay

function openPayModal(paymentId) {
  document.getElementById("payPaymentId").value = paymentId;
  document.getElementById("paymentMethodSelect").value = "Cash";
  new bootstrap.Modal(document.getElementById("payPaymentModal")).show();
}

async function payPayment(paymentId, paymentMethod) {
  try {
    const body = {
      paymentId: paymentId,
      paymentMethod,
    };

    const response = await fetch("http://localhost:8080/api/v1/payments/Pay", {
      method: "POST",

      headers: {
        "Content-Type": "application/json",

        Authorization: `Bearer ${token}`,
      },

      body: JSON.stringify(body),
    });

    if (!response.ok) {
      await throwApiError(response, "Payment failed");
    }

    bootstrap.Modal.getInstance(
      document.getElementById("payPaymentModal"),
    )?.hide();
    loadPayments();
  } catch (error) {
    console.log(error);

    showToast(error.message, "error");
  }
}

async function cancelPayment(paymentId) {
  const confirmed = await showConfirm(
    "Cancel Payment",
    "Are you sure you want to cancel this pending payment?",
  );

  if (!confirmed) return;

  try {
    const response = await fetch("http://localhost:8080/api/v1/payments/cancel", {
      method: "PUT",
      headers: {
        "Content-Type": "application/json",
        Authorization: `Bearer ${token}`,
      },
      body: JSON.stringify({ PaymentId: paymentId }),
    });

    if (!response.ok) {
      await throwApiError(response, "Payment cancellation failed");
    }

    showToast("Payment cancelled successfully", "success");
    loadPayments();
  } catch (error) {
    showToast(error.message, "error");
  }
}

document.getElementById("confirmPayBtn").addEventListener("click", async () => {
  const paymentId = Number(document.getElementById("payPaymentId").value);
  const paymentMethod = document.getElementById("paymentMethodSelect").value;
  await payPayment(paymentId, paymentMethod);
});

// Search

searchInput.addEventListener("input", () => {
  currentPage = 1;
  loadPayments(1, searchInput.value);
});

statusFilter.addEventListener("change", () => {
  currentPage = 1;
  loadPayments(1, searchInput.value);
});

sortByFilter.addEventListener("change", () => {
  currentPage = 1;
  loadPayments(1, searchInput.value);
});

sortDirectionFilter.addEventListener("change", () => {
  currentPage = 1;
  loadPayments(1, searchInput.value);
});

// Logout

function logout() {
  localStorage.removeItem("accessToken");

  localStorage.removeItem("refreshToken");

  window.location.href = "../auth/index.html";
}

// Init

loadPayments();


