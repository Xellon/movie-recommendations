import * as React from "react";
import { Authentication } from "../../common/Authentication";
import { GlobalNavigation } from "./Global";
import { UserNavigation } from "./User";

import "./Navigation.scss";

export class Navigation extends React.Component {
  public render() {
    const user = Authentication.getSignedInUser();

    return (
      <nav className="left-nav">
        {user ? <UserNavigation user={user} /> : undefined}
        <GlobalNavigation />
      </nav>
    );
  }
}