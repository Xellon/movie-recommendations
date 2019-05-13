import * as React from "react";
import { Button } from "@material-ui/core";
import { Utils } from "../../common/Utils";
import { withRouter, RouteComponentProps } from "react-router-dom";
import { Authentication } from "../../common/Authentication";
import * as DB from "../../model/DB";
import { RecommendedMovies } from "../../components/RecommendedMovies";

import "./Main.scss";

const RequestRecommendationButton = withRouter(({ history }) => (
  <Button
    style={{
      width: "50vw",
      height: "60px",
      display: "block",
      margin: "50px auto",
    }}
    variant="contained"
    color="primary"
    onClick={Utils.createOnNavigationClick(history, "/requestmovie")}
  >
    Request a Movie
  </Button>
));

export class Main extends React.PureComponent<RouteComponentProps> {
  public async componentDidMount() {
    await Authentication.verifyLoggedInUser();
    const user = Authentication.getCachedUser();

    switch (user.userType) {
      case DB.UserType.Client:
        break;
      case DB.UserType.Admin:
        this.props.history.push("/movies");
        break;
      case DB.UserType.Finance:
        this.props.history.push("/receipts");
        break;
    }
  }

  public render() {
    return (
      <main>
        <div
          style={{
            height: 200,
            padding: 1,
          }}
        >
          <RequestRecommendationButton />
        </div>
        <div>
          <h3>Last suggested movies</h3>
          <RecommendedMovies />
        </div>
      </main>
    );
  }
}