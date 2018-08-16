import * as React from "react";
import * as ReactModal from "react-modal";
import CloseOnEscape from "react-close-on-escape";
import LicenseSelection, {
    LicenseInfo
} from "../../react_components/licenseSelection";
import { Label } from "../../react_components/l10n";
import BloomButton from "../../react_components/bloomButton";
import { BloomApi } from "../../utils/bloomApi";
import "./IntellectualPropertyDialog.less";

interface IIntellectualPropertyDialogProps {}

interface IIntellectualPropertyDialogState {
    isOpen: boolean;
    showCreator: boolean;
    creator: string;
    copyrightYear: string;
    copyrightHolder: string;
    licenseInfo: LicenseInfo;
}

const InitialState: IIntellectualPropertyDialogState = {
    isOpen: false,
    showCreator: true,
    creator: "",
    copyrightYear: new Date().getFullYear().toString(),
    copyrightHolder: "",
    licenseInfo: new LicenseInfo()
};

export default class IntellectualPropertyDialog extends React.Component<
    IIntellectualPropertyDialogProps,
    IIntellectualPropertyDialogState
> {
    private static singleton: IntellectualPropertyDialog;

    constructor(props) {
        super(props);
        IntellectualPropertyDialog.singleton = this;
    }

    public readonly state = InitialState;

    private onChangeCopyrightYear(newValue) {
        this.setState({
            copyrightYear: newValue
        });
    }

    private onChangeCopyrightHolder(newValue) {
        this.setState({
            copyrightHolder: newValue
        });
    }

    private isCopyrightYearValid(copyrightYear: string) {
        return this.isValidYear(copyrightYear);
    }

    private isValidYear(possibleYear: string) {
        return /^\d\d\d\d$/g.exec(possibleYear) !== null;
    }

    private isCopyrightHolderValid(copyrightHolder: string) {
        return copyrightHolder !== "";
    }

    private handleLicenseSelectionChange(field, newValue) {
        const licenseInfo = { ...this.state.licenseInfo };
        licenseInfo[field] = newValue;
        this.setState({ licenseInfo });
    }

    private handleCreativeCommonsChange(field, newValue) {
        const licenseInfo = { ...this.state.licenseInfo };
        licenseInfo.creativeCommonsInfo[field] = newValue;
        this.setState({ licenseInfo });
    }

    public static show(showCreator: boolean) {
        IntellectualPropertyDialog.singleton.setState({
            isOpen: true,
            showCreator: showCreator
        });
        BloomApi.get("copyrightAndLicense/getBookLicenseMetadata", result => {
            console.log(result);
            this.singleton.setState(result.data);
        });
    }

    private handleCloseModal(save?: boolean) {
        this.setState({ isOpen: false });
        if (save) {
            BloomApi.postData(
                "copyrightAndLicense/saveBookLicenseMetadata",
                this.state
            );
        }
    }

    private getDialogTitle(): string {
        if (this.state.showCreator) return "Credit, Copyright, and License";
        return "Copyright and License";
    }

    private validateInput() {
        return {
            copyrightYear: !this.isCopyrightYearValid(this.state.copyrightYear),
            copyrightHolder: !this.isCopyrightHolderValid(
                this.state.copyrightHolder
            )
        };
    }

    public render() {
        const validationErrors = this.validateInput();
        // When we get GeckoFx 60, this can be simplified to use Object.values
        const isFormValid = Object.keys(validationErrors).every(
            key => !validationErrors[key]
        );
        const modal = (
            <CloseOnEscape
                onEscape={() => {
                    this.handleCloseModal();
                }}
            >
                <ReactModal
                    ariaHideApp={false}
                    isOpen={this.state.isOpen}
                    shouldCloseOnOverlayClick={true}
                    onRequestClose={() => this.handleCloseModal()}
                    className="reactModal intellectualPropertyDialog"
                    portalClassName="reactModalPortal"
                    overlayClassname="reactModalOverlay"
                >
                    <div className="dialogTitle">{this.getDialogTitle()}</div>
                    <div className="dialogContent">
                        <div
                            className={
                                "formRow" +
                                (this.state.showCreator ? "" : " invisible")
                            }
                        >
                            <Label htmlFor="creator" l10nKey="">
                                Creator
                            </Label>
                            <input
                                type="text"
                                name="creator"
                                value={this.state.creator}
                                onChange={event =>
                                    this.setState({
                                        creator: event.target.value
                                    })
                                }
                            />
                        </div>
                        <div className="formRow">
                            <Label
                                htmlFor="copyrightYear"
                                l10nKey=""
                                required={true}
                            >
                                Copyright Year
                            </Label>
                            <input
                                className={
                                    validationErrors.copyrightYear
                                        ? "invalidInput"
                                        : ""
                                }
                                type="text"
                                name="copyrightYear"
                                value={this.state.copyrightYear}
                                onChange={event =>
                                    this.onChangeCopyrightYear(
                                        event.target.value
                                    )
                                }
                            />
                        </div>
                        <div className="formRow">
                            <Label
                                htmlFor="copyrightHolder"
                                l10nKey=""
                                required={true}
                            >
                                Copyright Holder
                            </Label>
                            <textarea
                                className={
                                    validationErrors.copyrightHolder
                                        ? "invalidInput"
                                        : ""
                                }
                                name="copyrightHolder"
                                value={this.state.copyrightHolder}
                                onChange={event =>
                                    this.onChangeCopyrightHolder(
                                        event.target.value
                                    )
                                }
                            />
                        </div>
                        <LicenseSelection
                            content={this.state.licenseInfo}
                            handleLicenseSelectionChange={(field, newValue) =>
                                this.handleLicenseSelectionChange(
                                    field,
                                    newValue
                                )
                            }
                            onCreativeCommonsChange={(field, newValue) =>
                                this.handleCreativeCommonsChange(
                                    field,
                                    newValue
                                )
                            }
                        />
                        <div className="bottomButtonRow">
                            <BloomButton
                                l10nKey="Common.OK"
                                enabled={isFormValid}
                                onClick={() => {
                                    this.handleCloseModal(true);
                                }}
                                hasText={true}
                            >
                                OK
                            </BloomButton>
                            <BloomButton
                                l10nKey="Common.Cancel"
                                enabled={true}
                                onClick={() => {
                                    this.handleCloseModal();
                                }}
                                hasText={true}
                            >
                                Cancel
                            </BloomButton>
                        </div>
                    </div>
                </ReactModal>
            </CloseOnEscape>
        );
        return modal;
    }
}
