import * as React from "react";
import { UserMovies as Movies } from "../../components/usermovies/Movies";
import { Typography, Button, CircularProgress } from "@material-ui/core";
import { CustomEvent } from "../../common/CustomEvent";
import * as Model from "../../model/Model";
import * as DB from "../../model/DB";
import { Utils } from "../../common/Utils";
import { Authentication } from "../../common/Authentication";
import * as _ from "lodash";

import "./UserMovies.scss";
import { StatusSnackbar, StatusSnackbarType } from "../../components/StatusSnackbar";
import { BackendQueries } from "../../common/BackendQueries";

interface Message {
  text: string;
  isError: boolean;
}

interface State {
  userMovies?: Model.UserMovie[];
  isSaving: boolean;
  message?: Message;
}

export class UserMovies extends React.PureComponent<{}, State> {
  public readonly state: State = {
    isSaving: false,
  };

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

  private getAddedMovies(movies: Model.UserMovie[]) {
    if (this.state.userMovies)
      return movies.filter(m => !this.state.userMovies.find(userMovie => userMovie.movieId === m.movieId));
    return movies;
  }

  private _onSave = async (movies: Model.UserMovie[]) => {
    const addedMovies = this.getAddedMovies(movies);
    let isUpdateSuccessfull = true;

    this.setState({ isSaving: true, message: undefined });

    if (addedMovies && addedMovies.length)
      isUpdateSuccessfull = isUpdateSuccessfull && (await BackendQueries.UserMovies.requestAddition(addedMovies));

    const deletedMovies = this.getDeletedMovies(movies);

    if (deletedMovies && deletedMovies.length)
      isUpdateSuccessfull = isUpdateSuccessfull && (await BackendQueries.UserMovies.requestDeletion(deletedMovies));

    if (!isUpdateSuccessfull)
      this.setState({ isSaving: false, message: { text: "Update failed, please try again later!", isError: true } });

    this.setState({ isSaving: false, message: { text: "Update successfull!", isError: false } });

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
    await Authentication.verifyLoggedInUser();
    const user = Authentication.getCachedUser();

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
          className="usermovies-savebutton"
          onClick={this._onClick}
          variant="contained"
          color="primary"
          disabled={this.state.isSaving}
        >
          {this.state.isSaving
            ?
            <CircularProgress />
            :
            <Typography color="textPrimary">Save</Typography>
          }
        </Button>
        {this.state.message
          ?
          <StatusSnackbar
            message={this.state.message.text}
            type={this.state.message.isError
              ?
              StatusSnackbarType.Error
              :
              StatusSnackbarType.Success
            }
          />
          :
          undefined
        }

      </main>
    );
  }
}