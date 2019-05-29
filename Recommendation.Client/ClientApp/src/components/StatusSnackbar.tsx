import * as React from "react";
import { SnackbarContent, Snackbar } from "@material-ui/core";
import ErrorIcon from "@material-ui/icons/Error";
import SuccessIcon from "@material-ui/icons/CheckCircle";

import "./StatusSnackbar.scss";

export enum StatusSnackbarType {
  Success,
  Error,
}

export interface Props {
  message: string;
  type: StatusSnackbarType;
}

export function StatusSnackbar(props: Props) {
  const [isOpen, setIsOpen] = React.useState(true);
  const Icon = props.type === StatusSnackbarType.Success ? SuccessIcon : ErrorIcon;

  const typeClassName = props.type === StatusSnackbarType.Success ? "success" : "error";

  const onClose = () => setIsOpen(false);

  return (
    <Snackbar
      anchorOrigin={{
        vertical: "bottom",
        horizontal: "center",
      }}
      open={isOpen}
      autoHideDuration={3000}
      onClose={onClose}
    >
      <SnackbarContent
        className={"statussnackbar " + typeClassName}
        message={
          <span className="statussnackbar-message">
            <Icon className="statussnackbar-message-icon" />
            {props.message}
          </span>
        }
      />
    </Snackbar>
  );
}