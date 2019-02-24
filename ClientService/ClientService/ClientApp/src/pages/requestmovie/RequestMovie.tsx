import * as React from "react";
import { Button, Chip, CircularProgress, Typography } from "@material-ui/core";
import * as DB from "../../model/DB";
import { Utils } from "../../common/Utils";
import "./RequestMovie.scss";
import { Authentication } from "../../common/Authentication";
import { RecommendedMovies } from "../../components/RecommendedMovies";

enum RequestStatus {
  NotStarted,
  Pending,
  Error,
  Success,
}

interface State {
  tags: DB.Tag[];
  recommendationId?: number;
  requestStatus: RequestStatus;
  [index: number]: boolean;
}

export class RequestMovie extends React.Component<{}, State> {
  public readonly state: State = {
    tags: [],
    requestStatus: RequestStatus.NotStarted,
  };

  private renderTags(state: State) {
    return state.tags.map(tag => {
      return (
        <Chip
          key={tag.id}
          className={"requestmovie-chip" + (state[tag.id] ? " selected" : "")}
          label={tag.text}
          variant="outlined"
          // tslint:disable-next-line:jsx-no-lambda
          onClick={() => this.setState(prevState => ({ [tag.id]: !prevState[tag.id] }))}
        />);
    });
  }

  private _onClickGenerateRecommendations = async () => {
    const user = Authentication.getSignedInUser();
    if (!user)
      return;

    this.setState({ requestStatus: RequestStatus.Pending });

    const response = await Utils.fetchBackend(
      `/api/data/generaterecommendations?userId=${user.id}`, {
        method: "POST",
        headers: {
          "Accept": "application/json",
          "Content-Type": "application/json",
        },
        body: JSON.stringify(
          Object.keys(this.state)
            .filter(key => !isNaN(+key) && this.state[+key])),
      });

    if (!response.ok) {
      this.setState({ requestStatus: RequestStatus.Error });
      return;
    }

    this.setState({ recommendationId: +(await response.text()), requestStatus: RequestStatus.Success });
  }

  public async componentDidMount() {
    const response = await Utils.fetchBackend("/api/data/tags");

    if (!response.ok)
      return;

    const tags: DB.Tag[] = await response.json();

    this.setState({ tags });
  }

  public render() {
    return (
      <main>
        <Typography
          style={{
            margin: "30px auto",
            textAlign: "center",
          }}
          variant="h5"
        >
          Select movie genres
        </Typography>
        <div className="requestmovie-chipcontainer">
          {this.renderTags(this.state)}
        </div>
        <Button
          className="requestmovie-generatebutton"
          variant="contained"
          color="primary"
          onClick={this._onClickGenerateRecommendations}
          disabled={this.state.requestStatus === RequestStatus.Pending}
        >
          Generate recommendations
        </Button>
        {this.state.requestStatus === RequestStatus.Pending ? <CircularProgress /> : undefined}
        {this.state.requestStatus === RequestStatus.Error ? "Error generating recommended movies" : undefined}
        {this.state.requestStatus === RequestStatus.Success
          ?
          <>
            <Typography variant="h5">Generated movies</Typography>
            <RecommendedMovies recommendationId={this.state.recommendationId} />
          </>
          : undefined}
      </main>
    );
  }
}