import * as React from "react";
import { Authentication } from "../../common/Authentication";
import { GlobalNavigation } from "./Global";
import { UserNavigation } from "./User";

import "./Navigation.scss";

export function Navigation() {
  const user = Authentication.getCachedUser();

  return (
    <nav className="left-nav">
      {user ? <UserNavigation user={user} /> : undefined}
      <GlobalNavigation />
    </nav>
  );
}