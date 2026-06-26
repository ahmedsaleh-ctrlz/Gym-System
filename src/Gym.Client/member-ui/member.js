const token = localStorage.getItem("accessToken");
const API_BASE = "http://localhost:8080/api/v1";
const DEFAULT_AVATAR =
  "https://ui-avatars.com/api/?name=Member&background=2d2d2d&color=ffffff&size=128";
const STRIPE_PUBLISHABLE_KEY =
  "pk_test_51Tkz731Ib3k691E77uYykLRchBuXqRO3LgZVLPGDzHSVVxh5iNy2rxCDcbpwLjhn53S5sqZRzXK41UDhxNL4LrZr005oiwDqIQ";

if (!token) {
  window.location.href = "../admin-ui/auth/index.html";
}

const stripe = window.Stripe ? Stripe(STRIPE_PUBLISHABLE_KEY) : null;

const els = {
  memberImage: document.getElementById("memberImage"),
  memberName: document.getElementById("memberName"),
  memberEmail: document.getElementById("memberEmail"),
  memberPhone: document.getElementById("memberPhone"),
  memberJoinDate: document.getElementById("memberJoinDate"),
  currentPlan: document.getElementById("currentPlan"),
  currentStatus: document.getElementById("currentStatus"),
  currentEndDate: document.getElementById("currentEndDate"),
  pendingDue: document.getElementById("pendingDue"),
  currentSubscription: document.getElementById("currentSubscription"),
  pendingPaymentsList: document.getElementById("pendingPaymentsList"),
  plansGrid: document.getElementById("plansGrid"),
  paymentHistoryTable: document.getElementById("paymentHistoryTable"),
  attendanceList: document.getElementById("attendanceList"),
  subscriptionHistoryTable: document.getElementById("subscriptionHistoryTable"),
  loadingOverlay: document.getElementById("loadingOverlay"),
  subscriptionActionTitle: document.getElementById("subscriptionActionTitle"),
  subscriptionActionDescription: document.getElementById(
    "subscriptionActionDescription",
  ),
  subscriptionActionForm: document.getElementById("subscriptionActionForm"),
  subscriptionPlanSelect: document.getElementById("subscriptionPlanSelect"),
  subscriptionStartDateWrap: document.getElementById("subscriptionStartDateWrap"),
  subscriptionStartDate: document.getElementById("subscriptionStartDate"),
  subscriptionSubmitBtn: document.getElementById("subscriptionSubmitBtn"),
  freezeSubscriptionModal: document.getElementById("freezeSubscriptionModal"),
  freezeSubscriptionForm: document.getElementById("freezeSubscriptionForm"),
  freezeDaysInput: document.getElementById("freezeDaysInput"),
  confirmFreezeBtn: document.getElementById("confirmFreezeBtn"),
  stripePaymentModal: document.getElementById("stripePaymentModal"),
  stripePaymentSummary: document.getElementById("stripePaymentSummary"),
  stripeCardElement: document.getElementById("stripeCardElement"),
  confirmStripePayBtn: document.getElementById("confirmStripePayBtn"),
};

const stripePaymentModal = new bootstrap.Modal(els.stripePaymentModal);
const freezeSubscriptionModal = new bootstrap.Modal(
  els.freezeSubscriptionModal,
);

let currentMember = null;
let currentSubscription = null;
let plans = [];
let allPayments = [];
let pendingPayments = [];
let attendanceHistory = [];
let subscriptionHistory = [];
let selectedSubscriptionMode = "subscribe";
let selectedStripePayment = null;
let stripeElements = null;
let stripeCard = null;

function apiHeaders(extra = {}) {
  return {
    Authorization: `Bearer ${token}`,
    ...extra,
  };
}

async function apiGet(path, fallback) {
  const response = await fetch(`${API_BASE}${path}`, {
    headers: apiHeaders(),
  });

  if (!response.ok) {
    await throwApiError(response, fallback);
  }

  return response.json();
}

async function apiPost(path, body, fallback) {
  const response = await fetch(`${API_BASE}${path}`, {
    method: "POST",
    headers: apiHeaders({
      "Content-Type": "application/json",
    }),
    body: JSON.stringify(body),
  });

  if (!response.ok) {
    await throwApiError(response, fallback);
  }

  if (response.status === 204) return null;
  const text = await response.text();
  return text ? JSON.parse(text) : null;
}

async function apiPut(path, body, fallback) {
  const response = await fetch(`${API_BASE}${path}`, {
    method: "PUT",
    headers: apiHeaders({
      "Content-Type": "application/json",
    }),
    body: body ? JSON.stringify(body) : undefined,
  });

  if (!response.ok) {
    await throwApiError(response, fallback);
  }

  if (response.status === 204) return null;
  const text = await response.text();
  return text ? JSON.parse(text) : null;
}

async function apiDelete(path, fallback) {
  const response = await fetch(`${API_BASE}${path}`, {
    method: "DELETE",
    headers: apiHeaders(),
  });

  if (!response.ok) {
    await throwApiError(response, fallback);
  }

  return null;
}

async function loadMemberPortal() {
  showLoading(true);

  try {
    currentMember = await apiGet("/members/me", "Failed to load member profile");
    renderProfile(currentMember);

    const memberId = currentMember.memberId;

    const [currentSubResp, historyResp, attendanceResp, plansResp, paymentsResp] =
      await Promise.all([
        loadCurrentSubscription(memberId),
        apiGet(
          `/subscriptions/member/${memberId}/history`,
          "Failed to load subscription history",
        ),
        apiGet(
          `/attendances/${memberId}/history?pageNumber=1&pageSize=8&sortDirection=desc`,
          "Failed to load attendance history",
        ),
        apiGet("/plans?pageNumber=1&pageSize=20&sortDirection=asc", "Failed to load plans"),
        apiGet(`/payments/member/${memberId}`, "Failed to load payments"),
      ]);

    currentSubscription = currentSubResp;
    subscriptionHistory = Array.isArray(historyResp) ? historyResp : [];
    attendanceHistory = attendanceResp?.items || [];
    plans = plansResp?.items || [];
    allPayments = Array.isArray(paymentsResp) ? paymentsResp : [];
    pendingPayments = allPayments.filter((payment) => payment.status === "Pending");

    renderPlans(plans);
    renderProfileSections();
    renderCurrentSubscription(currentSubscription);
    renderPendingPayments(pendingPayments);
    renderPaymentHistory(allPayments);
    renderAttendance(attendanceHistory);
    renderSubscriptionHistory(subscriptionHistory);
    fillPlanSelect(plans);
    configureSubscriptionAction();
  } catch (error) {
    showToast(error.message, "error");
  } finally {
    showLoading(false);
  }
}

async function loadCurrentSubscription(memberId) {
  const response = await fetch(`${API_BASE}/subscriptions/member/${memberId}`, {
    headers: apiHeaders(),
  });

  if (response.status === 404) {
    return null;
  }

  if (!response.ok) {
    await throwApiError(response, "Failed to load current subscription");
  }

  return response.json();
}

function renderProfile(member) {
  const fullName =
    `${member.firstName || ""} ${member.lastName || ""}`.trim() || "Member";

  els.memberImage.src = member.imageUrl || DEFAULT_AVATAR;
  els.memberImage.onerror = () => {
    els.memberImage.src = DEFAULT_AVATAR;
  };
  els.memberName.textContent = fullName;
  els.memberEmail.textContent = member.email || "-";
  els.memberPhone.textContent = member.phoneNumber || "-";
  els.memberJoinDate.textContent = member.joinDate
    ? `Joined ${formatDate(member.joinDate)}`
    : "Join date unavailable";
}

function renderProfileSections() {
  const pendingTotal = pendingPayments.reduce(
    (sum, item) => sum + Number(item.amount || 0),
    0,
  );

  els.currentPlan.textContent = currentSubscription?.planName || "No active plan";
  els.currentStatus.innerHTML = currentSubscription
    ? statusPill(currentSubscription.status)
    : `<span class="empty-text-inline">-</span>`;
  els.currentEndDate.textContent = currentSubscription?.endDate
    ? formatDate(currentSubscription.endDate)
    : "-";
  els.pendingDue.textContent = `${pendingTotal} EGP`;
}

function renderCurrentSubscription(sub) {
  if (!sub) {
    els.currentSubscription.innerHTML =
      '<div class="empty-text">No current subscription found.</div>';
    return;
  }

  const details = [
    detailRow("Plan", sub.planName || "-"),
    detailRow("Status", statusPill(sub.status)),
    detailRow("Start Date", formatDate(sub.startDate)),
    detailRow("End Date", formatDate(sub.endDate)),
    detailRow("Price", `${sub.priceSnapshot ?? "-"} EGP`),
    detailRow(
      "Freeze Usage",
      `${sub.freezeCountUsed ?? 0} freezes, ${sub.totalFreezeDaysUsed ?? 0} days`,
    ),
  ];

  if (String(sub.status).toLowerCase() === "active") {
    details.push(`
      <button type="button" class="freeze-subscription-btn" onclick="openFreezeSubscription()">
        <i class="bi bi-snow2"></i>
        Freeze Subscription
      </button>
    `);
  }

  els.currentSubscription.innerHTML = details.join("");
}

function renderPendingPayments(payments) {
  if (!payments.length) {
    els.pendingPaymentsList.innerHTML =
      '<div class="empty-text">No pending payments right now.</div>';
    return;
  }

  els.pendingPaymentsList.innerHTML = payments
    .map(
      (payment) => `
        <article class="stack-item pending-payment-card">
          <div>
            <div class="item-title">${payment.planName || "Subscription Payment"}</div>
            <div class="muted">
              Payment #${payment.paymentId} | ${payment.amount ?? "-"} EGP
            </div>
            <div class="muted">
              Status: ${payment.status || "-"} | Subscription #${payment.subscriptionId || "-"}
            </div>
            ${statusPill(payment.status)}
          </div>
          <div class="pending-payment-actions">
            <button class="visa-btn" onclick="openStripePayment(${payment.paymentId})">
              <i class="bi bi-credit-card"></i>
              Pay Visa
            </button>
            <button class="cancel-payment-btn" onclick="cancelPendingPayment(${payment.paymentId})">
              <i class="bi bi-x-lg"></i>
              Cancel
            </button>
          </div>
        </article>
      `,
    )
    .join("");
}

async function cancelPendingPayment(paymentId) {
  const confirmed = await showConfirm(
    "Cancel Pending Payment",
    "Are you sure you want to cancel this payment?",
  );

  if (!confirmed) return;

  try {
    await apiPut(
      `/payments/${paymentId}/cancel`,
      null,
      "Failed to cancel payment",
    );
    showToast("Pending payment cancelled successfully", "success");
    await loadMemberPortal();
  } catch (error) {
    showToast(error.message, "error");
  }
}

function openFreezeSubscription() {
  if (!currentSubscription?.subscriptionId) {
    showToast("Current subscription was not found", "error");
    return;
  }

  els.freezeDaysInput.value = "";
  freezeSubscriptionModal.show();
}

els.freezeSubscriptionForm.addEventListener("submit", async (event) => {
  event.preventDefault();

  const freezeDays = Number(els.freezeDaysInput.value);
  if (!Number.isInteger(freezeDays) || freezeDays < 1) {
    showToast("Freeze days must be a whole number of at least 1", "error");
    return;
  }

  setButtonLoading(els.confirmFreezeBtn, true);

  try {
    await apiPut(
      `/subscriptions/${currentSubscription.subscriptionId}/freeze`,
      { freezeDays },
      "Failed to freeze subscription",
    );
    freezeSubscriptionModal.hide();
    showToast(`Subscription frozen for ${freezeDays} days`, "success");
    await loadMemberPortal();
  } catch (error) {
    showToast(error.message, "error");
  } finally {
    setButtonLoading(els.confirmFreezeBtn, false);
  }
});

function renderPaymentHistory(payments) {
  const completed = payments.filter((payment) => payment.status !== "Pending");

  if (!completed.length) {
    els.paymentHistoryTable.innerHTML =
      '<tr><td colspan="5"><div class="empty-text">No payment history yet.</div></td></tr>';
    return;
  }

  els.paymentHistoryTable.innerHTML = completed
    .map(
      (payment) => `
        <tr>
          <td>${payment.planName || "-"}</td>
          <td>${payment.amount ?? "-"} EGP</td>
          <td>${payment.paymentMethod || "-"}</td>
          <td>${statusPill(payment.status)}</td>
          <td>${payment.paidAtUtc ? formatDateTime(payment.paidAtUtc) : "-"}</td>
        </tr>
      `,
    )
    .join("");
}

function renderPlans(planList) {
  if (!planList.length) {
    els.plansGrid.innerHTML = '<div class="empty-text">No plans available.</div>';
    return;
  }

  els.plansGrid.innerHTML = planList
    .map((plan) => {
      const isCurrentPlan =
        currentSubscription?.planName &&
        currentSubscription.planName.toLowerCase() === plan.title.toLowerCase();

      return `
        <article class="plan-card ${isCurrentPlan ? "plan-card-current" : ""}">
          <div class="plan-card-head">
            <div>
              <h3>${plan.title}</h3>
              <div class="muted">${plan.description || "No description provided."}</div>
            </div>
            <div class="plan-price">${plan.cost} EGP</div>
          </div>

          <div class="plan-badges">
            <span class="plan-badge">${plan.durationInDays} days</span>
            <span class="plan-badge">${plan.allowedFreezeCount} freezes</span>
            <span class="plan-badge">${plan.maxTotalFreezeDays} max freeze days</span>
            ${isCurrentPlan ? '<span class="plan-badge">Current plan</span>' : ""}
          </div>
        </article>
      `;
    })
    .join("");
}

function renderAttendance(items) {
  if (!items.length) {
    els.attendanceList.innerHTML =
      '<div class="empty-text">No attendance records yet.</div>';
    return;
  }

  els.attendanceList.innerHTML = items
    .map(
      (item) => `
        <article class="timeline-item">
          <div>
            <div class="item-title">${formatDate(item.checkInAtUtc)}</div>
            <div class="muted">${formatTime(item.checkInAtUtc)}</div>
          </div>
          ${statusPill("CheckedIn", "Checked In")}
        </article>
      `,
    )
    .join("");
}

function renderSubscriptionHistory(items) {
  if (!items.length) {
    els.subscriptionHistoryTable.innerHTML =
      '<tr><td colspan="6"><div class="empty-text">No subscription history yet.</div></td></tr>';
    return;
  }

  els.subscriptionHistoryTable.innerHTML = items
    .map(
      (item) => `
        <tr>
          <td>${item.planName || "-"}</td>
          <td>${item.priceSnapshot ?? "-"} EGP</td>
          <td>${formatDate(item.startDate)}</td>
          <td>${formatDate(item.endDate)}</td>
          <td>${statusPill(item.status)}</td>
          <td>${item.freezeCountUsed ?? 0} / ${item.totalFreezeDaysUsed ?? 0} days</td>
        </tr>
      `,
    )
    .join("");
}

function fillPlanSelect(planList) {
  els.subscriptionPlanSelect.innerHTML = planList
    .map(
      (plan) =>
        `<option value="${plan.planId}">${plan.title} - ${plan.cost} EGP</option>`,
    )
    .join("");

  els.subscriptionPlanSelect.disabled = !planList.length;
  els.subscriptionSubmitBtn.disabled = !planList.length;
}

function configureSubscriptionAction() {
  selectedSubscriptionMode = currentSubscription ? "renew" : "subscribe";

  if (selectedSubscriptionMode === "renew") {
    els.subscriptionActionTitle.textContent = "Renew Your Subscription";
    els.subscriptionActionDescription.textContent =
      "Choose the plan you want for your next subscription period.";
    els.subscriptionSubmitBtn.textContent = "Renew Subscription";
    els.subscriptionStartDateWrap.classList.add("d-none");
    els.subscriptionStartDate.required = false;
  } else {
    els.subscriptionActionTitle.textContent = "Subscribe to a Plan";
    els.subscriptionActionDescription.textContent =
      "Choose a plan to create your first subscription.";
    els.subscriptionSubmitBtn.textContent = "Subscribe Now";
    els.subscriptionStartDateWrap.classList.remove("d-none");
    els.subscriptionStartDate.required = true;
    if (!els.subscriptionStartDate.value) {
      els.subscriptionStartDate.value = new Date().toISOString().split("T")[0];
    }
  }
}

els.subscriptionActionForm.addEventListener("submit", async (event) => {
  event.preventDefault();

  const planId = Number(els.subscriptionPlanSelect.value);
  const startDate = els.subscriptionStartDate.value;

  try {
    const fallback =
      selectedSubscriptionMode === "renew"
        ? "Renew subscription failed"
        : "Create subscription failed";

    await apiPost(
      selectedSubscriptionMode === "renew"
        ? "/subscriptions/renew"
        : "/subscriptions",
      selectedSubscriptionMode === "renew"
        ? {
            memberId: currentMember.memberId,
            planId,
          }
        : {
            memberId: currentMember.memberId,
            planId,
            startDate,
          },
      fallback,
    );

    showToast(
      selectedSubscriptionMode === "renew"
        ? "Subscription renewed. Pending payment created."
        : "Subscription created. Pending payment created.",
      "success",
    );
    await loadMemberPortal();
  } catch (error) {
    showToast(error.message, "error");
  }
});

function ensureStripeCardMounted() {
  if (!stripe) {
    throw new Error("Stripe checkout is unavailable.");
  }

  if (!stripeElements) {
    stripeElements = stripe.elements({
      appearance: {
        theme: "night",
      },
    });
  }

  if (!stripeCard) {
    stripeCard = stripeElements.create("card", {
      hidePostalCode: true,
    });
    stripeCard.mount("#stripeCardElement");
  }
}

function openStripePayment(paymentId) {
  selectedStripePayment = allPayments.find(
    (payment) => payment.paymentId === paymentId,
  );

  if (!selectedStripePayment) {
    showToast("Payment not found", "error");
    return;
  }

  els.stripePaymentSummary.innerHTML = `
    <div class="payment-summary-row">
      <span>Plan</span>
      <strong>${selectedStripePayment.planName || "-"}</strong>
    </div>
    <div class="payment-summary-row">
      <span>Amount</span>
      <strong>${selectedStripePayment.amount ?? "-"} EGP</strong>
    </div>
    <div class="payment-summary-row">
      <span>Payment ID</span>
      <strong>#${selectedStripePayment.paymentId}</strong>
    </div>
  `;

  try {
    ensureStripeCardMounted();
  } catch (error) {
    showToast(error.message, "error");
    return;
  }

  stripePaymentModal.show();
}

async function confirmStripePayment() {
  if (!selectedStripePayment) return;

  setButtonLoading(els.confirmStripePayBtn, true);

  try {
    const response = await fetch(
      `${API_BASE}/payments/${selectedStripePayment.paymentId}/stripe-intent`,
      {
        method: "POST",
        headers: apiHeaders(),
      },
    );

    if (!response.ok) {
      await throwApiError(response, "Failed to initialize Stripe payment");
    }

    const intent = await response.json();

    const result = await stripe.confirmCardPayment(intent.clientSecret, {
      payment_method: {
        card: stripeCard,
        billing_details: {
          name:
            `${currentMember.firstName || ""} ${currentMember.lastName || ""}`.trim() ||
            "Member",
          email: currentMember.email || undefined,
        },
      },
    });

    if (result.error) {
      throw new Error(result.error.message || "Stripe payment failed");
    }

    if (result.paymentIntent?.status !== "succeeded") {
      throw new Error("Payment was not completed");
    }

    stripePaymentModal.hide();
    showToast("Payment completed successfully", "success");
    await sleep(1200);
    await loadMemberPortal();
  } catch (error) {
    showToast(error.message, "error");
  } finally {
    setButtonLoading(els.confirmStripePayBtn, false);
  }
}

els.confirmStripePayBtn.addEventListener("click", confirmStripePayment);

function detailRow(label, value) {
  return `
    <div class="detail-row">
      <span>${label}</span>
      <strong>${value}</strong>
    </div>
  `;
}

function statusPill(status, label = status || "-") {
  return `<span class="status-pill status-${status || "Unknown"}">${label}</span>`;
}

function formatDate(value) {
  if (!value) return "-";
  return new Date(value).toLocaleDateString();
}

function formatDateTime(value) {
  if (!value) return "-";
  return new Date(value).toLocaleString();
}

function formatTime(value) {
  if (!value) return "-";
  return new Date(value).toLocaleTimeString([], {
    hour: "2-digit",
    minute: "2-digit",
  });
}

function showLoading(show) {
  els.loadingOverlay.classList.toggle("d-none", !show);
}

function logout() {
  localStorage.removeItem("accessToken");
  localStorage.removeItem("refreshToken");
  window.location.href = "../admin-ui/auth/index.html";
}

function sleep(ms) {
  return new Promise((resolve) => setTimeout(resolve, ms));
}

loadMemberPortal();
