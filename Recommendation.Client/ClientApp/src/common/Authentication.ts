import { Utils } from "./Utils";
import * as DB from "../model/DB";

function saveUser(user: DB.SignedInUser) {
  localStorage.setItem("user", JSON.stringify(user));
}

function saveUserId(userId: string) {
  localStorage.setItem("userId", userId);
}

function deleteUser() {
  localStorage.removeItem("user");
}

function deleteUserId() {
  localStorage.removeItem("userId");
}

function getUser(): DB.SignedInUser | undefined {
  const userString = localStorage.getItem("user");

  if (!userString)
    return undefined;

  return JSON.parse(userString);
}

async function isSessionAuthorized() {
  const response = await Utils.fetchBackend("/api/account/verifyAuthorization");

  if (response.ok)
    return true;

  return false;
}

async function logIn(email: string, password: string): Promise<DB.SignedInUser | undefined> {
  const response = await Utils.fetchBackend(
    "/api/account/login", {
      method: "POST",
      headers: {
        "Accept": "application/json",
        "Content-Type": "application/json",
      },
      body: JSON.stringify({ password, email }),
    },
  );

  if (!response.ok)
    return undefined;

  try {
    const userId = await response.json();
    saveUserId(userId);
    const user: DB.SignedInUser = { id: userId, email, userType: DB.UserType.Client };
    saveUser(user);
    return user;
  } catch {
    return undefined;
  }
}

async function verifyLoggedInUser() {
  if (await isSessionAuthorized())
    return;

  deleteUser();
}

function getCachedUser(): DB.SignedInUser | undefined {
  return getUser();
}

async function logOut() {
  const user = await getCachedUser();
  await Utils.fetchBackend(
    `/api/account/logout?userId=${user.id}`, {
      method: "POST",
    },
  );

  deleteUser();
  deleteUserId();
}

export const Authentication = {
  logIn,
  logOut,
  verifyLoggedInUser,
  getCachedUser,
};