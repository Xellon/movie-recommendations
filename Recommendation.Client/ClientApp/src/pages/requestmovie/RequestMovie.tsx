import * as React from "react";
import { Button, Typography, Paper } from "@material-ui/core";
import * as DB from "../../model/DB";
import "./RequestMovie.scss";
import { Authentication } from "../../common/Authentication";
import { RecommendedMovies } from "../../components/RecommendedMovies";
import { RecommendationStatusChecker } from "./RecommendationStatusChecker";
import { TagList } from "./TagList";
import { BackendQueries } from "../../common/BackendQueries";

enum RequestStatus {
  NotStarted,
  Pending,
  Error,
  Success,
}

export enum RecommendationStatus {
  Queued,
  InProgress,
  Finished,
  Stopped,
  Error,
  DoesNotExist,
}

interface State {
  tags: DB.Tag[];
  recommendationId?: number;
  queuedRecommendationId?: number;
  requestStatus: RequestStatus;
}

export class RequestMovie extends React.Component<{}, State> {
  private _selectedTagIds: number[] = [];

  public readonly state: State = {
    tags: [],
    requestStatus: RequestStatus.NotStarted,
  };

  private _onTagSelected = (tagIds: number[]) => {
    this._selectedTagIds = tagIds;
  }

  private _onRecommendationComplete = (status: RecommendationStatus) => {
    if (status === RecommendationStatus.Error || status === RecommendationStatus.DoesNotExist)
      this.setState({ requestStatus: RequestStatus.Error });
    else
      this.setState({ requestStatus: RequestStatus.Success });

    if (status === RecommendationStatus.Finished) {
      BackendQueries.Recommendations.queryRecommendationId(this.state.queuedRecommendationId)
        .then(recommendationId => this.setState({ recommendationId }));
    }
  }

  private _onClickGenerateRecommendations = async () => {
    const user = Authentication.getCachedUser();
    if (!user)
      return;

    this.setState({ requestStatus: RequestStatus.Pending });

    const queuedRecommendationId =
      await BackendQueries.Recommendations.startRecommendationGeneration(user.id, this._selectedTagIds);

    if (!queuedRecommendationId)
      return this.setState({ requestStatus: RequestStatus.Error });

    this.setState({ queuedRecommendationId });
  }

  private _onClickStopRecommendation = async () => {
    this.setState({ requestStatus: RequestStatus.NotStarted });
    BackendQueries.Recommendations.stopRecommendationGeneration(this.state.queuedRecommendationId);
  }

  public async componentDidMount() {
    const tags = await BackendQueries.Tags.queryTags();

    this.setState({ tags });
  }

  public render() {
    return (
      <main>
        <Paper className="requestmovie-block">
          <Typography
            className="requestmovie-title"
            variant="h5"
          >
            Select movie genres
          </Typography>
          <div className="requestmovie-chipcontainer">
            <TagList tags={this.state.tags} onTagSelected={this._onTagSelected} />
          </div>
          {this.state.requestStatus !== RequestStatus.Pending
            ?
            <Button
              className="requestmovie-generatebutton"
              variant="contained"
              color="primary"
              onClick={this._onClickGenerateRecommendations}
            >
              Generate recommendations
            </Button>
            :
            <Button
              className="requestmovie-stopbutton"
              variant="contained"
              color="secondary"
              onClick={this._onClickStopRecommendation}
            >
              Stop
            </Button>}
        </Paper>
        {this.state.requestStatus !== RequestStatus.NotStarted
          ?
          <Paper className="requestmovie-block">
            <RecommendationStatusChecker
              onComplete={this._onRecommendationComplete}
              queuedRecommendationId={this.state.queuedRecommendationId}
            />
          </Paper>
          : undefined}
        {this.state.requestStatus === RequestStatus.Success
          ?
          <Paper className="requestmovie-block">
            <Typography variant="h5">Generated movies</Typography>
            <RecommendedMovies recommendationId={this.state.recommendationId} />
          </Paper>
          : undefined}
      </main>
    );
  }
}