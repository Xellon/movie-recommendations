import * as React from "react";
import { List, CircularProgress } from "@material-ui/core";
import { SuggestedMovie, PlaceholderSuggestedMovie } from "./SuggestedMovie";
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
  isUserNew: boolean;
}

export class RecommendedMovies extends React.PureComponent<Props, State> {
  public readonly state: State = { isUserNew: false };

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

    if (recommendedMovies === undefined) {
      this.setState({ isUserNew: true });
      return;
    } else {
      this.setState({ recommendedMovies });
    }

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

    this.setState({ movies });
  }

  public render() {
    return (
      <>
        {!this.state.recommendedMovies && !this.state.isUserNew ? <CircularProgress /> : undefined}
        {this.state.isUserNew
          ? "There are no recommended movies yet. Pleast request a new recomendation!"
          :
          <RecommendedMovieList
            movies={this.state.movies}
            recommendedMovies={this.state.recommendedMovies}
          />
        }
      </>
    );
  }
}

interface RecommendedMovieListProps {
  movies?: Map<number, Movie>;
  recommendedMovies?: DB.RecommendedMovie[];
}

function RecommendedMovieList(props: RecommendedMovieListProps) {
  if (!props.recommendedMovies)
    return null;

  return (
    <List>
      {props.recommendedMovies.map(recommendedMovie => {
        if (!props.movies) {
          return (
            <PlaceholderSuggestedMovie key={`ph-${recommendedMovie.movieId}`} />
          );
        }

        const movie = props.movies.get(recommendedMovie.movieId);
        return (
          <SuggestedMovie
            key={movie.id}
            title={movie.title}
            imageUrl={movie.imageUrl}
            possibleRating={movie.rating}
            tags={movie.tags}
          />
        );
      })}
    </List>
  );
}