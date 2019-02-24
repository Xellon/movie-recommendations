import * as React from "react";
import { createNavigationButton } from "./User";

const MainPageButton = createNavigationButton("Main page", "/");
const AboutPageButton = createNavigationButton("About Us", "/about");

export class GlobalNavigation extends React.Component {
  public render() {
    return (
      <>
        <MainPageButton />
        <AboutPageButton />
      </>
    );
  }
}