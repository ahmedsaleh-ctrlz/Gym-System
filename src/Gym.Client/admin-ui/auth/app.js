const loginTab = document.getElementById("loginTab");

const registerTab = document.getElementById("registerTab");

const loginSection = document.getElementById("loginSection");

const registerSection = document.getElementById("registerSection");
const imageInput = document.getElementById("image");
const imageFileName = document.getElementById("imageFileName");
const imagePreview = document.getElementById("imagePreview");

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

// Tabs

loginTab.addEventListener("click", () => {
  loginTab.classList.add("active");

  registerTab.classList.remove("active");

  loginSection.classList.remove("d-none");

  registerSection.classList.add("d-none");
});

registerTab.addEventListener("click", () => {
  registerTab.classList.add("active");

  loginTab.classList.remove("active");

  registerSection.classList.remove("d-none");

  loginSection.classList.add("d-none");
});

// Toggle Password

function togglePassword(id, btn) {
  const input = document.getElementById(id);

  if (input.type === "password") {
    input.type = "text";

    btn.textContent = "Hide";
  } else {
    input.type = "password";

    btn.textContent = "Show";
  }
}

function getRoleFromToken(token) {
  try {
    const payload = JSON.parse(atob(token.split(".")[1]));
    const role =
      payload.role ||
      payload.roles ||
      payload[
        "http://schemas.microsoft.com/ws/2008/06/identity/claims/role"
      ];

    return Array.isArray(role) ? role[0] : role;
  } catch {
    return null;
  }
}

function redirectByRole(accessToken) {
  const role = (getRoleFromToken(accessToken) || "").toLowerCase();

  if (role === "member") {
    window.location.href = "../../member-ui/member.html";
    return;
  }

  window.location.href = "../dashboard/dashboard.html";
}

// Login

document.getElementById("loginForm").addEventListener("submit", async (e) => {
  e.preventDefault();

  const loginBtn = document.getElementById("loginBtn");

  const loginError = document.getElementById("loginError");

  loginError.classList.add("d-none");

  loginBtn.disabled = true;

  loginBtn.textContent = "Loading...";

  try {
    const response = await fetch(
      "https://localhost:7022/api/v1/Identity/token/generate",
      {
        method: "POST",

        headers: {
          "Content-Type": "application/json",
        },

        body: JSON.stringify({
          email: document.getElementById("loginEmail").value,

          password: document.getElementById("loginPassword").value,
        }),
      },
    );

    if (!response.ok) {
      if (response.status === 401) {
        throw new Error("Invalid email or password");
      }
      await throwApiError(response, "Login failed");
    }

    const data = await response.json();

    localStorage.setItem("accessToken", data.accessToken);

    localStorage.setItem("refreshToken", data.refreshToken);

    redirectByRole(data.accessToken);
  } catch (error) {
    loginError.classList.remove("d-none");
    loginError.textContent = error.message || "Login failed";
  } finally {
    loginBtn.disabled = false;

    loginBtn.textContent = "Login";
  }
});

// Register

document
  .getElementById("registerForm")
  .addEventListener("submit", async (e) => {
    e.preventDefault();

    const registerBtn = document.getElementById("registerBtn");

    const registerError = document.getElementById("registerError");

    registerError.classList.add("d-none");

    registerBtn.disabled = true;

    registerBtn.textContent = "Creating...";

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

      // Register

      const body = {
        firstName: document.getElementById("firstName").value,

        lastName: document.getElementById("lastName").value,

        email: document.getElementById("registerEmail").value,

        password: document.getElementById("registerPassword").value,

        phoneNumber: document.getElementById("phoneNumber").value,

        dateOfBirth:
          document.getElementById("dateOfBirth").value + "T00:00:00Z",

        joinDate: document.getElementById("joinDate").value + "T00:00:00Z",

        notes: document.getElementById("notes").value,

        imageUrl: imagePath,
      };

      const response = await fetch("https://localhost:7022/api/v1/Identity", {
        method: "POST",

        headers: {
          "Content-Type": "application/json",
        },

        body: JSON.stringify(body),
      });

      if (!response.ok) {
        await throwApiError(response, "Create account failed");
      }

      showToast("Success");

      registerTab.classList.remove("active");

      loginTab.classList.add("active");

      registerSection.classList.add("d-none");

      loginSection.classList.remove("d-none");

      document.getElementById("registerForm").reset();
      resetImageUploadUI();
    } catch (error) {
      console.log(error);

      registerError.classList.remove("d-none");
      registerError.textContent = error.message || "Create account failed";
    } finally {
      registerBtn.disabled = false;

      registerBtn.textContent = "Create Account";
    }
  });

bindImageUploadUI();

