import * as React from "react";
import { Route } from "react-router-dom";
import { CssBaseline, MuiThemeProvider, createMuiTheme } from "@material-ui/core";
import { Main } from "./pages/main/Main";
import { Login } from "./pages/login/Login";
import { Movies } from "./pages/movies/Movies";
import { Register } from "./pages/register/Register";
import { RequestMovie } from "./pages/requestmovie/RequestMovie";
import { Welcome } from "./pages/Welcome";
import { Authentication } from "./common/Authentication";
import * as DB from "./model/DB";
import { Navigation } from "./components/navigation/Navigation";
import { AboutPage } from "./pages/about/About";
import { HeaderBar } from "./components/HeaderBar";
import { UserMovies } from "./pages/usermovies/UserMovies";
import { lime, pink } from "@material-ui/core/colors";

import "./App.scss";

// function errorPage() {
//   return (
//     <main>Page does not exist</main>
//   );
// }

const appTheme = createMuiTheme({
  palette: {
    primary: lime,
    secondary: pink,
  },
  typography: {
    useNextVariants: true,
  },
});

interface State {
  showNavigation: boolean;
}

export default class App extends React.Component<{}, State> {
  public readonly state: State = { showNavigation: false };

  public async componentDidMount() {
    await Authentication.verifyLoggedInUser();
  }

  private _onNavigationClick = () => {
    this.setState(prevState => ({ showNavigation: !prevState.showNavigation }));
  }

  public render() {
    return (
      <MuiThemeProvider theme={appTheme}>
        <CssBaseline />
        <header>
          <HeaderBar onNavigationClick={this._onNavigationClick} />
        </header>
        <div
          className="content"
          style={{ gridTemplateColumns: this.state.showNavigation ? "200px auto" : "auto" }}
        >
          {/* Side menu */
            this.state.showNavigation ? <Navigation /> : undefined}

          <UserRoutes />
        </div>
      </MuiThemeProvider>
    );
  }
}

function SharedRoutes() {
  return (
    <>
      <Route path="/login" component={Login} />
      <Route path="/register" component={Register} />
      <Route path="/about" component={AboutPage} />
      {/* <Route path="*" component={errorPage} /> */}
    </>
  );
}

function UserRoutes() {
  const user = Authentication.getCachedUser();

  let routes = <SharedRoutes />;

  if (!user)
    return (<><Route exact path="/" component={Welcome} />{routes}</>);

  routes = (<><Route exact path="/" component={Main} />{routes}</>);

  switch (user.userType) {
    case DB.UserType.Client:
      routes = (
        <>
          <Route path="/requestmovie" component={RequestMovie} />
          <Route path="/usermovies" component={UserMovies} />
          {routes}
        </>);
      break;
    case DB.UserType.Admin:
      routes = (
        <>
          <Route path="/movies" component={Movies} />
          {routes}
        </>);
      break;
    case DB.UserType.Finance:
      break;
  }

  return routes;
}