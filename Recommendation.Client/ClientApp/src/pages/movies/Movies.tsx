import * as React from "react";
// import List from "@material-ui/core/List";
import ListItem from "@material-ui/core/ListItem";
import Avatar from "@material-ui/core/Avatar";
import DeleteIcon from "@material-ui/icons/Delete";
import { Paper, ListItemText, IconButton, Divider } from "@material-ui/core";
import * as DB from "../../model/DB";
import { Utils } from "../../common/Utils";
import { List as VirtualizedList, ListRowProps } from "react-virtualized";

interface State {
  movies?: DB.Movie[];
}

export class Movies extends React.PureComponent<{}, State> {
  public readonly state: State = {};
  private _mainRef = React.createRef<HTMLMainElement>();

  public async componentDidMount() {
    const response = await Utils.fetchBackend("/api/data/movies");

    if (!response.ok)
      return;

    const movies = await response.json() as DB.Movie[];
    this.setState({ movies });
  }

  public _renderMovie = (props: ListRowProps) => {
    const movie = this.state.movies[props.index];
    return (
      <div key={props.key} style={{ ...props.style, background: "white" }}>
        <ListItem>
          <ListItemText secondary={movie.title}>
            Title
            </ListItemText>
          <ListItemText style={{ flexGrow: 0 }} secondary={movie.averageRating}>
            Rating
            </ListItemText>
          <Avatar src={movie.imageUrl} />
          <IconButton style={{ marginLeft: 30 }}>
            <DeleteIcon />
          </IconButton>
        </ListItem>
        <Divider />
      </div>
    );
  }

  public render() {
    let dimensions: { width: number, height: number } | undefined;

    if (this._mainRef.current) {
      const box = this._mainRef.current.getBoundingClientRect();
      dimensions = { width: box.width, height: box.height };
    }

    // const items = this.createListItems(this.state);

    return (
      <main ref={this._mainRef}>
        <Paper>
          {dimensions && this.state.movies ?
            <VirtualizedList
              rowRenderer={this._renderMovie}
              rowHeight={71}
              rowCount={this.state.movies.length}
              {...dimensions}
            />
            : undefined}
        </Paper>
      </main>
    );
  }
}