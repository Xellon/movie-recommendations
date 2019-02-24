import * as React from "react";
import {
  TextField,
  ListItemText,
  ListItem,
  IconButton,
} from "@material-ui/core";
import { CustomEvent } from "../../common/CustomEvent";
import DeleteIcon from "@material-ui/icons/Delete";
import Autocomplete, { AutocompleteItem } from "../Autocomplete";
import * as DB from "../../model/DB";
import * as Model from "../../model/Model";
import { Utils } from "../../common/Utils";

class MovieImage extends React.PureComponent<{ imageUrl?: string }> {
  public render() {
    return (
      <img
        height="120px"
        src={this.props.imageUrl ? this.props.imageUrl : Utils.DEFAULT_MOVIE_IMAGE_URL}
      />
    );
  }
}

interface Props {
  id: number;
  onDelete: (id: number) => void;
  submitEvent: CustomEvent;
  onSubmit: (userMovie: Model.UserMovie) => void;
  movies: DB.Movie[];
  userMovie?: Model.UserMovie;
}

interface State {
  movieId?: number;
  rating?: number;
}

export default class UserMovie extends React.PureComponent<Props, State> {
  public readonly state: State = { ...this.props.userMovie };

  private _onSubmit = () => {
    if (!this.state.movieId || !this.state.rating)
      return false;

    this.props.onSubmit({
      movieId: this.state.movieId,
      rating: this.state.rating,
    });
    return true;
  }

  private _onDelete = () => {
    this.props.onDelete(this.props.id);
  }

  private _onTitleChange = (selectedMovie: AutocompleteItem) => {
    const movie = this.props.movies.find(m => m.title === selectedMovie.value);
    this.setState({ movieId: movie.id });
  }

  private _onRatingChange = (e: React.ChangeEvent<HTMLInputElement>) => {
    if (e.target.value === "") {
      this.setState({ rating: undefined });
      return;
    }
    const rating = e.target.valueAsNumber;
    if (isNaN(rating) || rating < 1 || rating > 10) {
      this.setState({ rating: 1 });
      return;
    }

    this.setState({ rating });
  }

  public componentDidMount() {
    this.props.submitEvent.register(this._onSubmit);
  }

  public componentWillUnmount() {
    this.props.submitEvent.unregister(this._onSubmit);
  }

  public render() {
    const setMovie = this.props.movies.find(m => m.id === this.state.movieId);

    return (
      <ListItem>
        <MovieImage
          imageUrl={setMovie !== undefined
            ? setMovie.imageUrl
            : undefined}
        />
        <ListItemText>
          <Autocomplete
            items={this.props.movies.map((movie, index) => ({ value: movie.title, index }))}
            textFieldProps={{
              fullWidth: true,
              label: "Title",
              margin: "normal",
              error: !this.state.movieId,
            }}
            defaultValue={setMovie ? setMovie.title : undefined}
            onSelectionChanged={this._onTitleChange}
          />
          <TextField
            style={{ width: "48px", display: "block" }}
            label="Rating"
            value={this.state.rating ? this.state.rating : ""}
            onChange={this._onRatingChange}
            type="number"
            margin="normal"
            error={!this.state.rating}
          />
        </ListItemText>
        <IconButton onClick={this._onDelete}>
          <DeleteIcon />
        </IconButton>
      </ListItem>
    );
  }
}