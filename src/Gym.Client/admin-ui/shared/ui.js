// Toast Container

const toastContainer = document.createElement("div");

toastContainer.className = "toast-container-custom";

document.body.appendChild(toastContainer);

// Show Toast

function showToast(message, type = "success") {
  const toast = document.createElement("div");

  toast.className = `custom-toast ${type}`;

  toast.innerHTML = `

        <div class="toast-content">

            <i class="bi ${
              type === "success"
                ? "bi-check-circle-fill"
                : type === "error"
                  ? "bi-x-circle-fill"
                  : "bi-info-circle-fill"
            }"></i>

            <span>${message}</span>

        </div>
    `;

  toastContainer.appendChild(toast);

  setTimeout(() => {
    toast.classList.add("show");
  }, 100);

  setTimeout(() => {
    toast.classList.remove("show");

    setTimeout(() => {
      toast.remove();
    }, 300);
  }, 3000);
}

// Confirm Dialog

function showConfirm(title, message) {
  return new Promise((resolve) => {
    const overlay = document.createElement("div");

    overlay.className = "confirm-overlay";

    overlay.innerHTML = `

            <div class="confirm-box">

                <h3>${title}</h3>

                <p>${message}</p>

                <div class="confirm-actions">

                    <button
                        class="confirm-cancel"
                    >
                        Cancel
                    </button>

                    <button
                        class="confirm-ok"
                    >
                        Confirm
                    </button>

                </div>

            </div>
        `;

    document.body.appendChild(overlay);

    overlay.querySelector(".confirm-cancel").onclick = () => {
      overlay.remove();

      resolve(false);
    };

    overlay.querySelector(".confirm-ok").onclick = () => {
      overlay.remove();

      resolve(true);
    };
  });
}

// Loading Button

function setButtonLoading(button, loading) {
  if (loading) {
    button.disabled = true;

    button.dataset.originalText = button.innerHTML;

    button.innerHTML = `

            <span
                class="spinner-border spinner-border-sm"
            ></span>

            Loading...
        `;
  } else {
    button.disabled = false;

    button.innerHTML = button.dataset.originalText;
  }
}

// API Error Helpers

async function getApiErrorMessage(response, fallback = "Request failed") {
  try {
    const raw = await response.text();
    if (!raw) return fallback;

    let data = null;
    try {
      data = JSON.parse(raw);
    } catch {
      return raw;
    }

    if (data?.errors && typeof data.errors === "object") {
      const messages = Object.values(data.errors).flat().filter(Boolean);
      if (messages.length) return messages.join(" | ");
    }

    if (data?.title) return data.title;
    if (data?.message) return data.message;
    if (data?.detail) return data.detail;

    return fallback;
  } catch {
    return fallback;
  }
}

async function throwApiError(response, fallback = "Request failed") {
  const message = await getApiErrorMessage(response, fallback);
  throw new Error(message);
}

// Silent Token Refresh

const nativeFetch = window.fetch.bind(window);
let refreshInFlight = null;

function isRefreshEndpoint(url) {
  return url.includes("/api/v1/Identity/token/refresh-token");
}

function isLoginEndpoint(url) {
  return url.includes("/api/v1/Identity/token/generate");
}

async function refreshAccessToken() {
  const refreshToken = localStorage.getItem("refreshToken");
  const expiredAccessToken = localStorage.getItem("accessToken");

  if (!refreshToken || !expiredAccessToken) {
    throw new Error("Missing refresh credentials");
  }

  const response = await nativeFetch(
    "https://localhost:7022/api/v1/Identity/token/refresh-token",
    {
      method: "POST",
      headers: {
        "Content-Type": "application/json",
      },
      body: JSON.stringify({
        refreshToken,
        expiredAccessToken,
      }),
    },
  );

  if (!response.ok) {
    throw new Error("Refresh token failed");
  }

  const data = await response.json();
  localStorage.setItem("accessToken", data.accessToken);
  localStorage.setItem("refreshToken", data.refreshToken);
  return data.accessToken;
}

window.fetch = async (input, init = {}) => {
  const requestUrl = typeof input === "string" ? input : input.url;

  const firstResponse = await nativeFetch(input, init);

  const shouldTryRefresh =
    firstResponse.status === 401 &&
    !isRefreshEndpoint(requestUrl) &&
    !isLoginEndpoint(requestUrl) &&
    !!localStorage.getItem("refreshToken");

  if (!shouldTryRefresh) {
    return firstResponse;
  }

  try {
    if (!refreshInFlight) {
      refreshInFlight = refreshAccessToken().finally(() => {
        refreshInFlight = null;
      });
    }

    const newAccessToken = await refreshInFlight;

    const retryHeaders = new Headers(init.headers || {});
    retryHeaders.set("Authorization", `Bearer ${newAccessToken}`);

    return await nativeFetch(input, {
      ...init,
      headers: retryHeaders,
    });
  } catch {
    localStorage.removeItem("accessToken");
    localStorage.removeItem("refreshToken");
    return firstResponse;
  }
};
