import * as React from "react";
import * as DB from "../../model/DB";
import { Typography, Divider, Avatar, Button } from "@material-ui/core";
import PersonIcon from "@material-ui/icons/Person";
import { withRouter } from "react-router-dom";
import { Utils } from "../../common/Utils";

export function createNavigationButton(text: string, path: string, style?: React.CSSProperties) {
  return withRouter(({ history }) => (
    <Button
      variant="contained"
      fullWidth
      style={{ margin: "0 auto 20px auto", display: "block", ...style }}
      onClick={Utils.createOnNavigationClick(history, path)}
    >
      {text}
    </Button>
  ));
}

const ChangeMoviesButton = createNavigationButton("Change movies", "/usermovies");
const RequestRecommendationButton = createNavigationButton("Request a Recommendation", "/requestmovie");

interface Props {
  user: DB.SignedInUser;
}

export function UserNavigation(props: Props) {
  return (
    <>
      <Avatar style={{ margin: "auto" }}>
        <PersonIcon />
      </Avatar>
      <Typography style={{ marginBottom: 30, marginTop: 20 }}>
        {props.user.email}
      </Typography>
      <UserNavigationButtons />
      <Divider style={{ margin: "20px 10px" }} />
    </>
  );
}

function UserNavigationButtons() {
  return (
    <>
      <ChangeMoviesButton />
      <RequestRecommendationButton />
    </>);
}