const token = localStorage.getItem("accessToken");
const DEFAULT_AVATAR =
  "https://ui-avatars.com/api/?name=Member&background=2d2d2d&color=ffffff&size=128";

if (!token) {
  window.location.href = "../admin-ui/auth/index.html";
}

let currentMember = null;
let memberPayments = [];

async function apiGet(url, fallback) {
  const response = await fetch(url, {
    headers: {
      Authorization: `Bearer ${token}`,
    },
  });

  if (!response.ok) {
    await throwApiError(response, fallback);
  }

  return await response.json();
}

async function loadMemberPortal() {
  showLoading(true);

  try {
    currentMember = await apiGet(
      "http://localhost:8080/api/v1/members/me",
      "Failed to load member profile",
    );

    renderProfile(currentMember);

    const memberId = currentMember.memberId;
    const [currentSub, subHistory, attendanceHistory, plans, payments] =
      await Promise.all([
        loadCurrentSubscription(memberId),
        apiGet(
          `http://localhost:8080/api/v1/subscriptions/member/${memberId}/history`,
          "Failed to load subscription history",
        ),
        apiGet(
          `http://localhost:8080/api/v1/attendances/${memberId}/history?pageNumber=1&pageSize=8&sortDirection=desc`,
          "Failed to load attendance history",
        ),
        apiGet(
          "http://localhost:8080/api/v1/plans?pageNumber=1&pageSize=20&sortDirection=asc",
          "Failed to load plans",
        ),
        apiGet(
          `http://localhost:8080/api/v1/payments/member/${memberId}`,
          "Failed to load payments",
        ),
      ]);

    memberPayments = Array.isArray(payments) ? payments : [];

    renderCurrentSubscription(currentSub);
    renderSubscriptionHistory(Array.isArray(subHistory) ? subHistory : []);
    renderAttendance(attendanceHistory.items || []);
    renderPlans(plans.items || []);
    renderPayments(memberPayments);
    renderSummary(currentSub, memberPayments);
  } catch (error) {
    showToast(error.message, "error");
  } finally {
    showLoading(false);
  }
}

async function loadCurrentSubscription(memberId) {
  try {
    return await apiGet(
      `http://localhost:8080/api/v1/subscriptions/member/${memberId}`,
      "Failed to load current subscription",
    );
  } catch {
    return null;
  }
}

function renderProfile(member) {
  const fullName =
    `${member.firstName || ""} ${member.lastName || ""}`.trim() || "Member";

  document.getElementById("memberImage").src =
    member.imageUrl || DEFAULT_AVATAR;
  document.getElementById("memberImage").onerror = () => {
    document.getElementById("memberImage").src = DEFAULT_AVATAR;
  };
  document.getElementById("memberName").textContent = fullName;
  document.getElementById("memberEmail").textContent = member.email || "-";
  document.getElementById("memberPhone").textContent =
    member.phoneNumber || "-";
  document.getElementById("memberJoinDate").textContent = member.joinDate
    ? `Joined ${formatDate(member.joinDate)}`
    : "Join date unavailable";
}

function renderSummary(currentSub, payments) {
  const pendingTotal = payments
    .filter((x) => x.status === "Pending")
    .reduce((sum, x) => sum + Number(x.amount || 0), 0);

  document.getElementById("currentPlan").textContent =
    currentSub?.planName || "No active plan";
  document.getElementById("currentStatus").innerHTML = currentSub
    ? statusPill(currentSub.status)
    : "-";
  document.getElementById("currentEndDate").textContent = currentSub?.endDate
    ? formatDate(currentSub.endDate)
    : "-";
  document.getElementById("pendingDue").textContent = `${pendingTotal} EGP`;
}

function renderCurrentSubscription(sub) {
  const container = document.getElementById("currentSubscription");

  if (!sub) {
    container.innerHTML = `<div class="empty-text">No current subscription found.</div>`;
    return;
  }

  container.innerHTML = `
    ${detailRow("Plan", sub.planName || "-")}
    ${detailRow("Status", statusPill(sub.status))}
    ${detailRow("Start Date", formatDate(sub.startDate))}
    ${detailRow("End Date", formatDate(sub.endDate))}
    ${detailRow("Price", `${sub.priceSnapshot ?? "-"} EGP`)}
    ${detailRow("Freeze Usage", `${sub.freezeCountUsed ?? 0} freezes, ${sub.totalFreezeDaysUsed ?? 0} days`)}
  `;
}

function renderPayments(payments) {
  const container = document.getElementById("paymentsList");

  if (!payments.length) {
    container.innerHTML = `<div class="empty-text">No payments yet.</div>`;
    return;
  }

  container.innerHTML = payments
    .map(
      (payment) => `
        <article class="stack-item">
          <div>
            <div class="item-title">${payment.planName || "Payment"}</div>
            <div class="muted">
              ${payment.amount ?? "-"} EGP | ${payment.paymentMethod || "No method"} | ${payment.paidAtUtc ? formatDate(payment.paidAtUtc) : "Not paid yet"}
            </div>
            ${statusPill(payment.status)}
          </div>
          ${
            payment.status === "Pending"
              ? `<button class="visa-btn" onclick="payByVisa(${payment.paymentId})">
                  <i class="bi bi-credit-card"></i> Pay Visa
                </button>`
              : ""
          }
        </article>
      `,
    )
    .join("");
}

function renderPlans(plans) {
  const container = document.getElementById("plansList");

  if (!plans.length) {
    container.innerHTML = `<div class="empty-text">No plans available.</div>`;
    return;
  }

  container.innerHTML = plans
    .map(
      (plan) => `
        <article class="plan-item">
          <div>
            <div class="item-title">${plan.title}</div>
            <div class="muted">${plan.durationInDays} days | ${plan.allowedFreezeCount} freezes | ${plan.maxTotalFreezeDays} max freeze days</div>
            <div class="muted">${plan.description || ""}</div>
          </div>
          <div class="plan-price">${plan.cost} EGP</div>
        </article>
      `,
    )
    .join("");
}

function renderAttendance(items) {
  const container = document.getElementById("attendanceList");

  if (!items.length) {
    container.innerHTML = `<div class="empty-text">No attendance records yet.</div>`;
    return;
  }

  container.innerHTML = items
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
  const container = document.getElementById("subscriptionHistory");

  if (!items.length) {
    container.innerHTML = `<div class="empty-text">No subscription history yet.</div>`;
    return;
  }

  container.innerHTML = `
    <table>
      <thead>
        <tr>
          <th>Plan</th>
          <th>Price</th>
          <th>Start</th>
          <th>End</th>
          <th>Status</th>
          <th>Freeze</th>
        </tr>
      </thead>
      <tbody>
        ${items
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
          .join("")}
      </tbody>
    </table>
  `;
}

async function payByVisa(paymentId) {
  const confirmed = await showConfirm(
    "Pay With Visa",
    "Complete this demo payment using Visa?",
  );

  if (!confirmed) return;

  try {
    const response = await fetch("http://localhost:8080/api/v1/payments/Pay", {
      method: "POST",
      headers: {
        "Content-Type": "application/json",
        Authorization: `Bearer ${token}`,
      },
      body: JSON.stringify({
        paymentId,
        paymentMethod: "Visa",
      }),
    });

    if (!response.ok) {
      await throwApiError(response, "Payment failed");
    }

    showToast("Visa payment completed", "success");
    await loadMemberPortal();
  } catch (error) {
    showToast(error.message, "error");
  }
}

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

function formatTime(value) {
  if (!value) return "-";
  return new Date(value).toLocaleTimeString([], {
    hour: "2-digit",
    minute: "2-digit",
  });
}

function showLoading(show) {
  document.getElementById("loadingOverlay").classList.toggle("d-none", !show);
}

function logout() {
  localStorage.removeItem("accessToken");
  localStorage.removeItem("refreshToken");
  window.location.href = "../admin-ui/auth/index.html";
}

loadMemberPortal();
