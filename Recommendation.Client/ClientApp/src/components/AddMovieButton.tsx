import * as React from "react";
import { Avatar, Typography, ListItem, ListItemAvatar, ListItemText } from "@material-ui/core";
import AddIcon from "@material-ui/icons/Add";

export interface AddMovieButtonProps {
  onClick: () => void;
}

export default class AddMovieButton extends React.PureComponent<AddMovieButtonProps> {
  public render() {
    return (
      <ListItem button onClick={this.props.onClick}>
        <ListItemText>
          <Typography variant="subtitle1">
            Add new movie
            </Typography>
        </ListItemText>
        <ListItemAvatar>
          <Avatar>
            <AddIcon />
          </Avatar>
        </ListItemAvatar>
      </ListItem>
    );
  }
}