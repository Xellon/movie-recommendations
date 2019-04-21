import * as React from "react";
import { List } from "@material-ui/core";
import { SuggestedMovie } from "./SuggestedMovie";
import * as DB from "../model/DB";
import { BackendQueries } from "../common/BackendQueries";

interface Movie {
  id: number;
  title: string;
  imageUrl: string;
  rating: number;
  tags: string[];
}

export interface Props {
  recommendationId?: number;
}

interface State {
  movies?: Map<number, Movie>;
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
          possibleRating={movie.rating}
          tags={movie.tags}
        />
      );
    });
  }

  public async componentDidMount() {
    const recommendedMovies = await BackendQueries.Movies.queryRecommendedMovies();
    const dbTags = await BackendQueries.Tags.queryTags();
    const tags: string[] = [];
    for (const tag of dbTags) {
      tags[tag.id] = tag.text;
    }

    const movieArray = await BackendQueries.Movies.queryMovies();

    const movies = new Map<number, Movie>();

    for (const dbMovie of movieArray) {
      const movie: Movie = {
        id: dbMovie.id,
        imageUrl: dbMovie.imageUrl,
        rating: dbMovie.averageRating,
        tags: dbMovie.tags.map(t => tags[t.tagId]),
        title: dbMovie.title,
      };
      movies.set(dbMovie.id, movie);
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