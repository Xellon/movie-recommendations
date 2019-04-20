import * as React from "react";
import { Button, Typography } from "@material-ui/core";
import * as DB from "../../model/DB";
import { Utils } from "../../common/Utils";
import "./RequestMovie.scss";
import { Authentication } from "../../common/Authentication";
import { RecommendedMovies } from "../../components/RecommendedMovies";
import { RecommendationStatusChecker } from "./RecommendationStatusChecker";
import { TagList } from "./TagList";

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
  requestStatus: RequestStatus;
}

export class RequestMovie extends React.Component<{}, State> {
  private _selectedTagIds: number[] = [];

  public readonly state: State = {
    tags: [],
    requestStatus: RequestStatus.NotStarted,
  };

  private async startRecommendationGeneration(userId: string) {
    const response = await Utils.fetchBackend(
      `/api/recommendations/generate?userId=${userId}`, {
        method: "POST",
        headers: {
          "Accept": "application/json",
          "Content-Type": "application/json",
        },
        body: JSON.stringify(this._selectedTagIds),
      });

    if (!response.ok)
      return 0;

    return +response.text();
  }

  private _onClickGenerateRecommendations = async () => {
    const user = Authentication.getLoggedInUser();
    if (!user)
      return;

    this.setState({ requestStatus: RequestStatus.Pending });

    const recommendationId = await this.startRecommendationGeneration(user.id);

    if (!recommendationId) {
      return this.setState({ requestStatus: RequestStatus.Error });
    }

    this.setState({ recommendationId, requestStatus: RequestStatus.Success });
  }

  private async queryTags(): Promise<DB.Tag[]> {
    const response = await Utils.fetchBackend("/api/data/tags");

    if (!response.ok)
      return [];

    return response.json();
  }

  public async componentDidMount() {
    const tags = await this.queryTags();

    this.setState({ tags });
  }

  private _onTagSelected = (tagIds: number[]) => {
    this._selectedTagIds = tagIds;
  }

  private _onRecommendationComplete = (status: RecommendationStatus) => {
    this.setState({ requestStatus: RequestStatus.Success });
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
          <TagList tags={this.state.tags} onTagSelected={this._onTagSelected} />
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
        {this.state.requestStatus !== RequestStatus.NotStarted
          ?
          <RecommendationStatusChecker
            onComplete={this._onRecommendationComplete}
            recommendationId={this.state.recommendationId}
          />
          : undefined}
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