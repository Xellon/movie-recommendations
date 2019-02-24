import * as React from "react";
import { UserMovies as Movies } from "../../components/usermovies/Movies";
import { Typography, Button } from "@material-ui/core";
import { CustomEvent } from "../../common/CustomEvent";
import * as Model from "../../model/Model";
import * as DB from "../../model/DB";
import { Utils } from "../../common/Utils";
import { Authentication } from "../../common/Authentication";
import * as _ from "lodash";

interface State {
  userMovies?: Model.UserMovie[];
}

export class UserMovies extends React.PureComponent<{}, State> {
  public readonly state: State = {};

  private _saveEvent = new CustomEvent();

  private getDeletedMovies(movies: Model.UserMovie[]) {
    if (!this.state.userMovies)
      return undefined;

    const moviesToDelete: Model.UserMovie[] = [];

    for (const movie of this.state.userMovies) {
      if (!movies.find(m => m.movieId === movie.movieId))
        moviesToDelete.push(movie);
    }

    return moviesToDelete;
  }

  private async requestDeletion(movies: Model.UserMovie[]) {
    const user = Authentication.getSignedInUser();

    await Utils.fetchBackend(`/api/data/user/movies?userId=${user.id}`, {
      method: "DELETE",
      body: JSON.stringify(movies),
      headers: {
        "Accept": "application/json",
        "Content-Type": "application/json",
      },
    });
  }

  private getAddedMovies(movies: Model.UserMovie[]) {
    if (this.state.userMovies)
      return movies.filter(m => !this.state.userMovies.find(userMovie => userMovie.movieId === m.movieId));
    return movies;
  }

  private async requestAddition(movies: Model.UserMovie[]) {
    const user = Authentication.getSignedInUser();

    await Utils.fetchBackend(`/api/data/user/movies?userId=${user.id}`, {
      method: "POST",
      body: JSON.stringify(movies),
      headers: {
        "Accept": "application/json",
        "Content-Type": "application/json",
      },
    });
  }

  private _onSave = async (movies: Model.UserMovie[]) => {
    const addedMovies = this.getAddedMovies(movies);

    if (addedMovies && addedMovies.length)
      await this.requestAddition(addedMovies);

    const deletedMovies = this.getDeletedMovies(movies);

    if (deletedMovies && deletedMovies.length)
      await this.requestDeletion(deletedMovies);

    let userMovies = addedMovies;
    if (this.state.userMovies)
      userMovies = userMovies.concat(
        this.state.userMovies.filter(m => !deletedMovies.find(deletedMovie => deletedMovie.movieId === m.movieId)));
    this.setState({ userMovies });
  }

  private _onClick = () => {
    this._saveEvent.notify(this, undefined);
  }

  public async componentDidMount() {
    const user = Authentication.getSignedInUser();

    const response = await Utils.fetchBackend(`/api/data/user/movies?userId=${user.id}`);

    if (!response.ok)
      return;

    const userMovies = await response.json() as DB.UserMovie[];
    this.setState({ userMovies: userMovies.map(m => ({ movieId: m.movieId, rating: m.rating })) });
  }

  public render() {
    return (
      <main>
        <Typography variant="h5">User movies</Typography>
        <Movies onSubmit={this._onSave} submitEvent={this._saveEvent} userMovies={this.state.userMovies} />
        <Button
          style={{
            margin: "50px auto",
            display: "block",
            width: "80%",
            height: "60px",
          }}
          onClick={this._onClick}
          variant="contained"
          color="primary"
        >
          <Typography color="textPrimary">Save</Typography>
        </Button>
      </main>
    );
  }
}