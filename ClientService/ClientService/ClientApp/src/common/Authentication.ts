import { Utils } from "./Utils";
import * as DB from "../model/DB";

function saveUser(user: DB.SignedInUser) {
  localStorage.setItem("user", JSON.stringify(user));
}

function deleteUser() {
  localStorage.removeItem("user");
}

function getUser(): DB.SignedInUser | undefined {
  const userString = localStorage.getItem("user");

  if (!userString)
    return undefined;

  return JSON.parse(userString);
}

async function signIn(email: string, password: string): Promise<DB.SignedInUser | undefined> {
  const response = await Utils.fetchBackend(
    "/api/authentication/signin", {
      method: "POST",
      headers: { password, email },
    },
  );

  if (!response.ok)
    return undefined;

  try {
    const user: DB.SignedInUser = await response.json();
    saveUser(user);
    return user;
  } catch {
    return undefined;
  }
}

function getSignedInUser(): DB.SignedInUser | undefined {
  return getUser();
}

function signOut() {
  deleteUser();
}

export const Authentication = {
  signIn,
  signOut,
  getSignedInUser,
};