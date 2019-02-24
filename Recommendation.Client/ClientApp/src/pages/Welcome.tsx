import * as React from "react";
import { Paper, Typography } from "@material-ui/core";
import "./main/Main.scss";

export const Welcome: React.FunctionComponent = () => {
  return (
    <main>
      <Paper style={{ padding: "20px" }}>
        <Typography
          variant="h4"
          style={{ margin: "30px auto", textAlign: "center" }}
        >
          Welcome to Movie Recommendation service
        </Typography>
        <Typography>
          Register, fill in your most favorite movies and get the appropriate recommendations.
          </Typography>
      </Paper>
    </main>
  );
};