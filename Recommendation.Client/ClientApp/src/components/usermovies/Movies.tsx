import * as React from "react";
import { List, Paper, Typography, Divider, ListItem, CircularProgress } from "@material-ui/core";
import AddMovieButton from "../../components/AddMovieButton";
import UserMovie from "./Movie";
import { CustomEvent } from "../../common/CustomEvent";
import { Utils } from "../../common/Utils";
import * as Model from "../../model/Model";
import * as DB from "../../model/DB";

interface Props {
  submitEvent: CustomEvent;
  onSubmit: (info: Model.UserMovie[]) => Promise<void>;
  userMovies?: Model.UserMovie[];
}

interface State {
  componentIds: number[];
  movies?: DB.Movie[];
}

export class UserMovies extends React.Component<Props, State> {
  private _lastId = 0;
  private _submitEvent = new CustomEvent();
  private _defaultUserMovies = new Map<number, Model.UserMovie>();

  public readonly state: State = {
    componentIds: [],
  };

  private userMovies: Model.UserMovie[] = this.props.userMovies;

  private _onSubmit = (movie: Model.UserMovie) => {
    this.userMovies.push(movie);
  }

  private _onSubmitTriggered = () => {
    this.userMovies = [];

    const result = this._submitEvent.notify(this, undefined);

    this.props.onSubmit(this.userMovies);

    return result;
  }

  private createNewId() {
    return ++this._lastId;
  }

  private async loadMovies() {
    const response = await Utils.fetchBackend("/api/data/movies");

    if (!response.ok)
      return;

    const movies = await response.json() as DB.Movie[];

    this.setState({ movies });
  }

  private _onUserMovieDelete = (id: number) => {
    const ids = this.state.componentIds;
    ids.splice(ids.indexOf(id), 1);
    this.setState({ componentIds: ids });
  }

  private _onAddNewUserMovie = () => {
    const ids = this.state.componentIds;
    ids.push(this.createNewId());

    this.setState({ componentIds: ids });
  }

  public async componentDidMount() {
    await this.loadMovies();

    this.props.submitEvent.register(this._onSubmitTriggered);
  }

  public async componentDidUpdate(prevProps: Props) {
    if (this.props.userMovies && this.props.userMovies !== prevProps.userMovies) {
      const ids = [];
      this._defaultUserMovies.clear();

      for (const movie of this.props.userMovies) {
        const id = this.createNewId();
        this._defaultUserMovies.set(id, movie);
        ids.push(id);
      }

      this.setState({ componentIds: ids });
    }
  }

  public async componentWillUnmount() {
    await this.loadMovies();
    this.props.submitEvent.unregister(this._onSubmitTriggered);
  }

  private renderMovies(state: State) {
    return state.componentIds.map(id => {
      const movie = this._defaultUserMovies.get(id);

      return (
        <React.Fragment key={"user_movie_" + id}>
          <UserMovie
            id={id}
            onSubmit={this._onSubmit}
            submitEvent={this._submitEvent}
            onDelete={this._onUserMovieDelete}
            movies={state.movies}
            userMovie={movie}
          />
          <Divider />
        </React.Fragment>
      );
    });
  }

  public render() {
    return (
      <Paper style={{ margin: "5px 0", padding: 10 }}>
        <Typography variant="h6">Movies</Typography>
        <List>
          {this.state.movies
            ?
            <>
              <AddMovieButton key="movie_button" onClick={this._onAddNewUserMovie} />
              {this.renderMovies(this.state)}
            </>
            :
            <ListItem key="circular_progress">
              <CircularProgress />
            </ListItem>
          }
        </List>
      </Paper>
    );
  }
}
