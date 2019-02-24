import * as React from "react";
import { List } from "@material-ui/core";
import { SuggestedMovie } from "./SuggestedMovie";
import * as DB from "../model/DB";
import { Utils } from "../common/Utils";
import { Authentication } from "../common/Authentication";

interface Props {
  recommendationId?: number;
}

interface State {
  movies?: Map<number, DB.Movie>;
  recommendedMovies?: DB.RecommendedMovie[];
}

export class RecommendedMovies extends React.PureComponent<Props, State> {
  public readonly state: State = {};

  public renderMovies() {
    if (!this.state.movies || !this.state.recommendedMovies)
      return;

    return this.state.recommendedMovies.map(recommendedMovie => {
      const movie = this.state.movies.get(recommendedMovie.movieId);
      return (
        <SuggestedMovie
          key={movie.id}
          title={movie.title}
          imageUrl={movie.imageUrl}
          possibleRating={recommendedMovie.possibleRating}
        />
      );
    });
  }

  private async getMovies(): Promise<DB.Movie[] | undefined> {
    const response = await Utils.fetchBackend("/api/data/movies");

    if (!response.ok)
      return undefined;

    return response.json();
  }

  private async getRecommendedMovies(recommendationId?: number): Promise<DB.RecommendedMovie[] | undefined> {
    const user = Authentication.getSignedInUser();

    let response: Response;

    if (recommendationId)
      response = await Utils.fetchBackend(
        `/api/data/recommendedmovies/${recommendationId}?userId=${user.id}`);
    else
      response = await Utils.fetchBackend(
        `/api/data/recommendedmovies/latest?userId=${user.id}`);

    if (!response.ok)
      return undefined;

    return response.json();
  }

  public async componentDidMount() {
    const recommendedMovies = await this.getRecommendedMovies();

    const movieArray = await this.getMovies();

    const movies = new Map<number, DB.Movie>();

    for (const movie of movieArray) {
      movies.set(movie.id, movie);
    }

    this.setState({ recommendedMovies, movies });
  }

  public render() {
    return (
      <List>
        {this.renderMovies()}
      </List>
    );
  }
}