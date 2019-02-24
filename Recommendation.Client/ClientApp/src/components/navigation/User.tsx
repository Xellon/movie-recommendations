import * as React from "react";
import * as DB from "../../model/DB";
import { Typography, Divider, Avatar, Button } from "@material-ui/core";
import PersonIcon from "@material-ui/icons/Person";
import { withRouter } from "react-router-dom";
import { Utils } from "../../common/Utils";
import { Authentication } from "../../common/Authentication";

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
const ReceiptsButton = createNavigationButton("Receipts", "/receipts");
const BecomeAMemberButton = createNavigationButton("Become a Member!", "/membership", { background: "gold" });
const MembershipButton = createNavigationButton("Membership", "/membership");

const MovieListButton = createNavigationButton("Movie List", "/movies");
const UsersButton = createNavigationButton("Users", "/users");

interface Props {
  user: DB.SignedInUser;
}

interface State {
  isClientMember?: boolean;
}

export class UserNavigation extends React.Component<Props, State> {
  public readonly state: State = {};

  private getButtons() {
    switch (this.props.user.userType) {
      case DB.UserType.Client:
        return (
          <>
            <ChangeMoviesButton />
            <RequestRecommendationButton />
            <ReceiptsButton />
            {this.state.isClientMember === undefined
              ? <MembershipButton />
              : (this.state.isClientMember ? <MembershipButton /> : <BecomeAMemberButton />)}
          </>);
      case DB.UserType.Admin:
        return (
          <>
            <MovieListButton />
            <UsersButton />
          </>);
      case DB.UserType.Finance:
        return (
          <>
            <ReceiptsButton />
          </>);
    }

  }

  public async componentDidMount() {
    const user = Authentication.getSignedInUser();

    const response = await Utils.fetchBackend(`/api/data/user/isMember?userId=${user.id}`);

    this.setState({ isClientMember: (await response.text()) === "true" });
  }

  public render() {
    return (
      <>
        <Avatar style={{ margin: "auto", border: this.state.isClientMember ? "solid 6px gold" : undefined }}>
          <PersonIcon />
        </Avatar>
        <Typography style={{ marginBottom: 30, marginTop: 20 }}>
          {this.props.user.email}
        </Typography>
        {this.getButtons()}
        <Divider style={{ margin: "20px 10px" }} />
      </>
    );
  }
}