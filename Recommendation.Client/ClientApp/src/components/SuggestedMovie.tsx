import * as React from "react";
import { ListItemText, Paper, ListItem, CircularProgress } from "@material-ui/core";
import { Utils } from "../common/Utils";

interface Props {
  title: string;
  imageUrl?: string;
  tags: string[];
  possibleRating: number;
}

export function SuggestedMovie(props: Props) {
  return (
    <Paper style={{ margin: 5 }}>
      <ListItem>
        <img
          src={props.imageUrl
            ? props.imageUrl
            : Utils.DEFAULT_MOVIE_IMAGE_URL}
          height={100}
        />
        <ListItemText secondary={props.tags.reduce((t1, t2) => `${t1}; ${t2}`)}>
          {props.title}
        </ListItemText>
        <ListItemText
          secondary={props.possibleRating}
          style={{ maxWidth: 150 }}
        >
          Possible Rating
        </ListItemText>
      </ListItem>
    </Paper>
  );
}

export function PlaceholderSuggestedMovie() {
  return (
    <Paper style={{ margin: 5 }}>
      <ListItem>
        <div style={{ height: 100, width: 66.66, background: "grey" }} />
        <ListItemText secondary={"Placeholder tags"}>
          Placeholder Title
        </ListItemText>
        <CircularProgress />
        <ListItemText
          secondary={"..."}
          style={{ maxWidth: 150 }}
        >
          Possible Rating
        </ListItemText>
      </ListItem>
    </Paper>
  );
}
