import * as React from "react";
import { AppBar, Toolbar, IconButton, Button, Typography } from "@material-ui/core";
import { Authentication } from "../common/Authentication";
import { Utils } from "../common/Utils";
import { withRouter } from "react-router-dom";
import MenuIcon from "@material-ui/icons/Menu";
import * as DB from "../model/DB";

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

export function HeaderBar(props: Props) {
  const user = Authentication.getCachedUser();

  React.useEffect(() => {
    Authentication.verifyLoggedInUser();
  }, []);

  return (
    <>
      <AppBar position="static">
        <Toolbar>
          <IconButton
            color="inherit"
            aria-label="Menu"
            onClick={props.onNavigationClick}
          >
            <MenuIcon />
          </IconButton>
          <MainPageButton />
          <UserButtons user={user} />
        </Toolbar>
      </AppBar>
    </>
  );
}

interface UserButtonProps {
  user: DB.SignedInUser | undefined;
}

function UserButtons(props: UserButtonProps) {
  const onSignOut = () => {
    Authentication.logOut();
    location.reload();
  };

  if (!props.user)
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
      onClick={onSignOut}
    >
      Sign Out
    </Button>
  );
}