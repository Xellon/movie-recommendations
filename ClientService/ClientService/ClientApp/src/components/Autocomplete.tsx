import * as React from "react";
import Downshift, { ControllerStateAndHelpers } from "downshift";
import { TextField, Paper } from "@material-ui/core";
import { TextFieldProps } from "@material-ui/core/TextField";

import "./Autocomplete.scss";

export interface AutocompleteItem {
  value: string;
  index: number;
}

interface Props {
  items: AutocompleteItem[];
  textFieldProps?: TextFieldProps;
  defaultValue?: string;
  onSelectionChanged: (item: AutocompleteItem) => void;
}

interface State {
  input: string;
  selectedItem: AutocompleteItem;
}

const defaultItem: AutocompleteItem = {
  index: Number.MAX_SAFE_INTEGER,
  value: "",
};

export default class Autocomplete extends React.PureComponent<Props> {
  public readonly state: State = {
    input: "",
    selectedItem: this.props.defaultValue
      ? this.props.items.find(i => i.value === this.props.defaultValue)
      : defaultItem,
  };

  private _onAutocomplete = (selectedItem: AutocompleteItem) => {
    this.setState({ selectedItem });
    this.props.onSelectionChanged(selectedItem);
  }

  private _itemToString = (item: AutocompleteItem) => {
    return item.value;
  }

  private renderSuggestions = (controllerState: ControllerStateAndHelpers<AutocompleteItem>) => {
    return (
      <div style={{ position: "relative" }}>
        <Paper style={{ position: "fixed", zIndex: 100 }}>
          <ul {...controllerState.getMenuProps({ className: "autocomplete-suggestion-list" })}>
            {this.props.items
              .filter(item => !controllerState.inputValue
                || item.value.toLocaleLowerCase().includes(controllerState.inputValue.toLocaleLowerCase()))
              .map((item, index) => (
                <li
                  {...controllerState.getItemProps({
                    key: item.index,
                    index,
                    item,
                    style: {
                      backgroundColor:
                        controllerState.highlightedIndex === index ? "lightgray" : "white",
                      fontWeight: controllerState.selectedItem === item ? "bold" : "normal",
                    },
                  })}
                >
                  {item.value}
                </li>
              ))}
          </ul>
        </Paper>
      </div>
    );
  }

  private renderTextField = (controllerState: ControllerStateAndHelpers<AutocompleteItem>) => {
    return (
      <div>
        <TextField {...controllerState.getInputProps(this.props.textFieldProps)} />
        {controllerState.isOpen ? this.renderSuggestions(controllerState) : undefined}
      </div>
    );
  }

  public render() {
    return (
      <Downshift
        onChange={this._onAutocomplete}
        selectedItem={this.state.selectedItem}
        itemToString={this._itemToString}
      >
        {this.renderTextField}
      </Downshift>
    );
  }
}