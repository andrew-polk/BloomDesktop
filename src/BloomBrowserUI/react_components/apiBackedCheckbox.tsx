import * as React from "react";
import * as ReactDOM from "react-dom";
import { ILocalizationProps } from "./l10n";
import axios from "axios";
import { Checkbox } from "./checkbox";

// Use this component when you have a one-to-one correspodence between a checkbox and an api endpoint

interface IProps extends ILocalizationProps {
    apiPath: string;
}
interface IState {
    checked: boolean;
}
export class ApiBackedCheckbox extends React.Component<IProps, IState> {
    constructor(props) {
        super(props);
        this.state = { checked: false };
    }
    public componentDidMount() {
        axios.get(this.props.apiPath).then(result => {
            const c = result.data as boolean;
            this.setState({ checked: c });
        });
    }
    public render() {
        return (
            <Checkbox
                className={this.props.className}
                checked={this.state.checked}
                l10nKey={this.props.l10nKey}
                onCheckChanged={c => {
                    this.setState({ checked: c });
                    axios.post(this.props.apiPath, c, {
                        headers: { "Content-Type": "application/json" }
                    });
                }}
            >
                {this.props.children}
            </Checkbox>
        );
    }
}