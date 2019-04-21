import * as React from "react";
import { ListItemText, Paper, ListItem } from "@material-ui/core";
import { Utils } from "../common/Utils";

interface Props {
  title: string;
  imageUrl?: string;
  tags: string[];
  possibleRating: number;
}

export class SuggestedMovie extends React.PureComponent<Props> {
  public render() {
    return (
      <Paper style={{ margin: 5 }}>
        <ListItem>
          <img
            src={this.props.imageUrl
              ? this.props.imageUrl
              : Utils.DEFAULT_MOVIE_IMAGE_URL}
            height={100}
          />
          <ListItemText secondary={this.props.tags.reduce((t1, t2) => `${t1}; ${t2}`)}>
            {this.props.title}
          </ListItemText>
          <ListItemText
            secondary={this.props.possibleRating}
            style={{ maxWidth: 150 }}
          >
            Possible Rating
          </ListItemText>
        </ListItem>
      </Paper>
    );
  }
}