import * as React from "react";
import { AppBar, Toolbar, IconButton, Button, Typography } from "@material-ui/core";
import { Authentication } from "../common/Authentication";
import { Utils } from "../common/Utils";
import { withRouter } from "react-router-dom";
import MenuIcon from "@material-ui/icons/Menu";

const styles = {
  grow: {
    flexGrow: 1,
  },
  menuButton: {
    marginLeft: -12,
    marginRight: 20,
  },
};

const MainPageButton = withRouter(({ history }) => (
  <Button
    color="inherit"
    style={styles.grow}
    onClick={Utils.createOnNavigationClick(history, "/")}
  >
    <Typography variant="h6" color="inherit">
      Movie Recommendation Service
    </Typography>
  </Button>
));

const LoginPageButton = withRouter(({ history }) => (
  <Button
    color="inherit"
    style={styles.menuButton}
    onClick={Utils.createOnNavigationClick(history, "/login")}
  >
    Login
  </Button>
));

const RegisterPageButton = withRouter(({ history }) => (
  <Button
    color="inherit"
    style={styles.menuButton}
    onClick={Utils.createOnNavigationClick(history, "/register")}
  >
    Register
  </Button>
));

interface Props {
  onNavigationClick: () => void;
}

export class HeaderBar extends React.Component<Props> {

  private onSignOut = () => {
    Authentication.signOut();
    location.reload();
  }

  private getAppBarButtonsForUser() {
    const user = Authentication.getSignedInUser();

    if (!user)
      return (
        <>
          <LoginPageButton />
          <RegisterPageButton />
        </>
      );

    return (
      <Button
        color="inherit"
        style={styles.menuButton}
        onClick={this.onSignOut}
      >
        Sign Out
      </Button>
    );
  }

  public render() {
    return (
      <>
        <AppBar position="static">
          <Toolbar>
            <IconButton
              color="inherit"
              aria-label="Menu"
              onClick={this.props.onNavigationClick}
            >
              <MenuIcon />
            </IconButton>
            <MainPageButton />
            {this.getAppBarButtonsForUser()}
          </Toolbar>
        </AppBar>
      </>
    );
  }
}