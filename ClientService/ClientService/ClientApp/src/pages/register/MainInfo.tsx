import * as React from "react";
import { TextField } from "@material-ui/core";
import { CustomEvent } from "../../common/CustomEvent";

export interface MainInfoForm {
  email: string;
  password: string;
}

export interface Props {
  submitEvent: CustomEvent;
  onSubmit: (mainInfo: MainInfoForm) => void;
}

export interface State extends MainInfoForm {
  emailError?: string;
  passwordError?: string;
}

export class MainInfo extends React.Component<Props, State> {
  public readonly state: State = {
    email: "",
    password: "",
  };

  constructor(props: Props) {
    super(props);
    props.submitEvent.register(this._onSubmit);
  }

  private handleChange = (type: keyof MainInfoForm) => {
    return (e: React.ChangeEvent<HTMLInputElement>) => {
      const value = this.readValue(type, e);
      this.setState({
        [type]: value,
        [type + "Error"]: undefined,
      } as Pick<MainInfoForm, keyof MainInfoForm>);
    };
  }

  private readValue(type: keyof MainInfoForm, e: React.ChangeEvent<HTMLInputElement>) {
    let value = e.target.value;
    if (type === "password") {
      const hiddenLetters = value.lastIndexOf("*");
      value = this.state[type].slice(0, hiddenLetters + 1) +
        value.slice(hiddenLetters + 1);
    }
    return value;
  }

  private validate(type: keyof MainInfoForm) {
    let error: string | undefined;
    switch (type) {
      case "email":
        error = this.validateEmail(this.state.email);
        break;
      case "password":
        error = this.validatePassword(this.state.password);
        break;
    }
    this.setState({ [type + "Error"]: error } as Pick<MainInfoForm, keyof MainInfoForm>);
    return !error;
  }

  private validateEmail(email: string) {
    if (!email.toLowerCase().match(/.*@.*\.[a-z]{2,6}$/g))
      return "Email is wrong!";
    return undefined;
  }

  private validatePassword(password: string) {
    if (password.length < 5)
      return "Password is shorter than 6 letters!";
    if (password.length > 40)
      return "Password is too long!";
    if (!password.match(/[0-9]/g))
      return "Password does not have a number!";
    if (!password.match(/[A-Z]/g))
      return "Password does not have an Uppercase letter!";
    return undefined;
  }

  private _onSubmit = () => {
    if (!this.validate("email") || !this.validate("password"))
      return false;

    this.props.onSubmit({
      email: this.state.email,
      password: this.state.password,
    });

    return true;
  }

  public componentDidMount() {
    this.props.submitEvent.register(this._onSubmit);
  }

  public componentWillUnmount() {
    this.props.submitEvent.unregister(this._onSubmit);
  }

  public render() {
    return (
      <>
        <div>
          <TextField
            label="Email"
            required
            margin="normal"
            value={this.state.email}
            onChange={this.handleChange("email")}
            helperText={this.state.emailError}
            error={!!this.state.emailError}
          />
        </div>
        <div>
          <TextField
            label="Password"
            required
            margin="normal"
            value={this.state.password.replace(/./g, "*")}
            onChange={this.handleChange("password")}
            helperText={this.state.passwordError}
            error={!!this.state.passwordError}
          />
        </div>
      </>
    );
  }
}