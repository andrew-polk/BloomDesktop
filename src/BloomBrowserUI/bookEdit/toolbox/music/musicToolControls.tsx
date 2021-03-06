import * as React from "react";
import * as ReactDOM from "react-dom";
import ToolboxToolReactAdaptor from "../toolboxToolReactAdaptor";
import {
    H1,
    Div,
    IUILanguageAwareProps,
    Label
} from "../../../react_components/l10n";
import { RadioGroup, Radio } from "../../../react_components/radio";
import axios from "axios";
import { ToolBox, ITool } from "../toolbox";
import Slider from "rc-slider";
import AudioRecording from "../talkingBook/audioRecording";
import "./music.less";

interface IMusicState {
    activeRadioValue: string;
    volumeSliderPosition: number; // 1..100
    audioEnabled: boolean;
    musicName: string;
    playing: boolean;
}

// This react class implements the UI for the music toolbox.
// Note: this file is included in toolboxBundle.js because webpack.config says to include all
// tsx files in bookEdit/toolbox.
// The toolbox is included in the list of tools because of the one line of immediately-executed code
// which adds an instance of Music to ToolBox.getMasterToolList().
export class MusicToolControls extends React.Component<{}, IMusicState> {
    // duplicates information in HtmlDom.cs
    // The names of the attributes (of the main page div) which store the background
    // audio file name (relative to the audio folder) and the volume (a fraction
    // of full volume).
    private static musicAttrName = "data-backgroundaudio";
    private static musicVolumeAttrName = MusicToolControls.musicAttrName +
        "volume";
    private static kDefaultVolumeFraction = 0.5;
    private static narrationPlayer: AudioRecording;
    private addedListenerToPlayer: boolean;
    constructor() {
        super({});
        this.state = this.getStateFromHtmlOfPage();
    }
    public hideTool() {
        const rawPlayer = document.getElementById(
            "musicPlayer"
        ) as HTMLMediaElement;
        rawPlayer.pause();
    }

    public showTool() {
        this.updateBasedOnContentsOfPage();
    }

    public updateMarkup() {
        // This isn't exactly updating the markup, but it needs to happen when we switch pages,
        // just like updating markup. Using this hook does mean it will (unnecessarily) happen
        // every time the user pauses typing while this tool is active. I don't much expect people
        // to be editing the book and configuring background music at the same time, so I'm not
        // too worried. If it becomes a performance problem, we could enhance ITool with a
        // function that is called just when the page switches.
        this.updateBasedOnContentsOfPage();
    }
    public getStateFromHtmlOfPage(): IMusicState {
        let audioFileName = ToolboxToolReactAdaptor.getBloomPageAttr(
            MusicToolControls.musicAttrName
        );
        let hasMusicAttr = typeof audioFileName === typeof ""; // may be false or undefined if missing
        if (!audioFileName) {
            audioFileName = ""; // null won't handle split
        }
        const state = {
            activeRadioValue: "continueMusic",
            volumeSliderPosition: Math.round(
                MusicToolControls.kDefaultVolumeFraction * 100
            ),
            audioEnabled: false,
            musicName: "",
            playing: false
        }; // default state
        if (!hasMusicAttr) {
            // No data-backgroundAudio attr at all is our default state, continue from previous page
            // (including possibly no audio, if previous page had none). If audio is set on a previous
            // page, it can flow over into this one.
        } else if (audioFileName) {
            // If we have a non-empty music attr, we're setting new music right here.
            state.activeRadioValue = "newMusic";
            state.audioEnabled = true;
            const volume = MusicToolControls.getPlayerVolumeFromAttributeOnPage(
                audioFileName
            );
            state.volumeSliderPosition = MusicToolControls.convertVolumeToSliderPosition(
                volume
            );
            state.musicName = this.getDisplayNameOfMusicFile(audioFileName);
        } else {
            // If we have the attribute, but the value is empty, we're explicitly turning it off.
            state.activeRadioValue = "noMusic";
        }
        return state;
    }

    // This is not very react-ive. In theory, I think, if things can be updated from outside the control,
    // they are props, not state. But then things that can be updated from inside it are state.
    // All these things can be affected from outside (e.g., when we change pages), while the purpose of
    // the control is to change them. Probably in an ideal world, all our state would be props, and we
    // would issue events when we want them changed, and if our parent wants our display to change
    // accordingly, it would change our props. But at least for now, this is the outer boundary of the
    // react stuff, and allowing this component to know how to update the HTML when things change and
    // itself when the HTML changes lets it encapsulate a lot more of the functionality. Hence this
    // method (and the fact that various changes update the HTML rather than raising events).
    public updateBasedOnContentsOfPage(): void {
        this.setState(this.getStateFromHtmlOfPage());
    }

    public render() {
        return (
            <div className="musicBody">
                <Div
                    className="musicHelp"
                    l10nKey="EditTab.Toolbox.Music.Overview"
                >
                    You can set up background music to play with this page when
                    the book is viewed in the Bloom Reader app.
                </Div>
                <RadioGroup
                    onChange={val => this.setRadio(val)}
                    value={this.state.activeRadioValue}
                >
                    <Radio
                        l10nKey="EditTab.Toolbox.Music.NoMusic"
                        value="noMusic"
                    >
                        No Music
                    </Radio>
                    <Radio
                        l10nKey="EditTab.Toolbox.Music.ContinueMusic"
                        value="continueMusic"
                    >
                        Continue music from previous page
                    </Radio>
                    <div className="musicChooseWrapper">
                        <Radio
                            l10nKey="EditTab.Toolbox.Music.NewMusic"
                            value="newMusic"
                        >
                            Start new music
                        </Radio>
                        <Label
                            className="musicChooseFile"
                            l10nKey="EditTab.Toolbox.Music.Choose"
                            onClick={() => this.chooseMusicFile()}
                        >
                            Choose...
                        </Label>
                    </div>
                </RadioGroup>

                <div
                    className={
                        "button-label-wrapper" +
                        (this.state.audioEnabled ? "" : " disabled")
                    }
                    id="musicOuterWrapper"
                >
                    <div id="musicPlayAndLabelWrapper">
                        <div className="musicButtonWrapper">
                            <button
                                id="musicPreview"
                                className={
                                    "music-button ui-button enabled" +
                                    (this.state.playing ? " playing" : "")
                                }
                                onClick={() => this.previewMusic()}
                            />
                        </div>
                        <div id="musicFilename">{this.state.musicName}</div>
                    </div>
                    <div
                        id="musicVolumePercent"
                        style={{
                            visibility: this.state.audioEnabled
                                ? "visible"
                                : "hidden"
                        }}
                    >
                        {this.state.volumeSliderPosition}%
                    </div>
                    <div id="musicSetVolume">
                        <img
                            className="speaker-volume"
                            src="speaker-volume.svg"
                        />
                        <div className="bgSliderWrapper">
                            <Slider
                                className="musicVolumeSlider"
                                value={this.state.volumeSliderPosition}
                                disabled={!this.state.audioEnabled}
                                onChange={value => this.sliderMoved(value)}
                            />
                        </div>
                    </div>
                </div>
                {
                    // preload=none prevents the audio element from asking for the audio as soon as it gets a new src value,
                    // which in BL-3153 was faster than the c# thread writing the file could finish with it.
                    // As an alternative, a settimeout() in the javascript also worked, but
                    // this seems more durable. By the time the user can click Play, we'll be done.}
                }
                <audio id="musicPlayer" preload="none" />
            </div>
        );
    }

    private getPlayer(): HTMLMediaElement {
        return document.getElementById("musicPlayer") as HTMLMediaElement;
    }

    private previewMusic() {
        const player = this.getPlayer();
        if (!this.addedListenerToPlayer) {
            player.addEventListener("ended", () =>
                this.setState({ playing: false })
            );
            this.addedListenerToPlayer = true;
        }
        MusicToolControls.previewBackgroundMusic(
            player,
            () => this.state.playing,
            playing => this.setState({ playing: playing })
        );
    }

    public static previewBackgroundMusic(
        player: HTMLMediaElement,
        currentlyPlaying: () => boolean, // caller function for testing whether already playing
        setPlayState: (boolean) => void
    ) {
        // call-back function for changing playing state
        // automatic indentation is weird here. This is the start of the body of previewBackgroundMusic,
        // not of setPlayState, which is just a function parameter.
        let audioFileName = ToolboxToolReactAdaptor.getBloomPageAttr(
            this.musicAttrName
        );
        if (!audioFileName) {
            return;
        }
        if (currentlyPlaying()) {
            player.pause();
            if (this.narrationPlayer) {
                this.narrationPlayer.stopListen();
                this.narrationPlayer = null;
            }
            setPlayState(false);
            return;
        }
        const bookSrc = ToolboxToolReactAdaptor.getPageFrame().src;
        const index = bookSrc.lastIndexOf("/");
        const bookFolderUrl = bookSrc.substring(0, index + 1);
        const musicUrl = encodeURI(bookFolderUrl + "audio/" + audioFileName);
        // The ?nocache argument is ignored, except that it ensures each time we do this,
        // src is a different URL, so the player treats it as a new sound to play.
        // Without this it may not play if it hasn't changed.
        player.setAttribute(
            "src",
            musicUrl + "?nocache=" + new Date().getTime()
        );
        player.volume = this.getPlayerVolumeFromAttributeOnPage(audioFileName);
        player.play();
        // Play the audio during animation
        this.narrationPlayer = new AudioRecording();
        this.narrationPlayer.setupForListen();
        this.narrationPlayer.listen();
        setPlayState(true);
    }

    private pausePlaying() {
        this.getPlayer().pause();
        this.setState({ playing: false });
    }

    private setRadio(val: string) {
        if (val === this.state.activeRadioValue) return;
        const audioEnabled = val === "newMusic";
        this.setState({
            activeRadioValue: val,
            audioEnabled: audioEnabled,
            musicName: ""
        });
        if (!audioEnabled) {
            this.pausePlaying();
        }
        switch (val) {
            case "noMusic":
                ToolboxToolReactAdaptor.setBloomPageAttr(
                    MusicToolControls.musicAttrName,
                    ""
                );
                break;
            case "continueMusic":
                ToolboxToolReactAdaptor.getBloomPage().removeAttribute(
                    MusicToolControls.musicAttrName
                );
                break;
            // choosing the third button doesn't change anything, until you actually choose a file.
        }
    }

    // Get the audio volume. The value of audioFileName is checked to determine whether data-backgroundAudioVolume
    // is used at all. If anything goes wrong, or we're not specifying new music for this page,
    // we return the default volume.
    private static getPlayerVolumeFromAttributeOnPage(
        audioFileName: string
    ): number {
        const audioVolumeStr = ToolboxToolReactAdaptor.getBloomPageAttr(
            this.musicVolumeAttrName
        );
        let audioVolumeFraction: number =
            MusicToolControls.kDefaultVolumeFraction;
        if (audioFileName && audioVolumeStr) {
            try {
                audioVolumeFraction = parseFloat(audioVolumeStr);
            } catch (e) {
                audioVolumeFraction = MusicToolControls.kDefaultVolumeFraction;
            }
            if (
                isNaN(audioVolumeFraction) ||
                audioVolumeFraction > 1.0 ||
                audioVolumeFraction < 0.0
            ) {
                audioVolumeFraction = MusicToolControls.kDefaultVolumeFraction;
            }
        }
        return audioVolumeFraction;
    }

    // Instead of a linear progression, we make the slider be very insensitive on the left,
    // so that you have more control at the low volumes that are likely for background music.
    // We are using 3 but 2 is also ok. Using 3 because the effect feels right to me (subjectively,
    // with a very small sample size).
    private static kLowEndVolumeSensitivity: number = 3;

    // Position is always 0 to 100, and resulting volume is always 0.0 to 1.0, via a non-linear function
    private static convertSliderPositionToVolume(
        position1To100: number
    ): number {
        return Math.pow(
            position1To100 / 100,
            MusicToolControls.kLowEndVolumeSensitivity
        );
    }

    // Volume is always 0.0 to 1.0, and the resulting position is always 0 to 100
    // In between is a non-linear function.
    private static convertVolumeToSliderPosition(
        volumeFraction: number
    ): number {
        return Math.round(
            Math.pow(
                volumeFraction,
                1 / MusicToolControls.kLowEndVolumeSensitivity
            ) * 100
        );
    }

    private sliderMoved(position1to100: number): void {
        const volume = MusicToolControls.convertSliderPositionToVolume(
            position1to100
        );
        ToolboxToolReactAdaptor.setBloomPageAttr(
            MusicToolControls.musicVolumeAttrName,
            volume.toString()
        );
        this.getPlayer().volume = volume;
        this.setState((prevState, props) => {
            return { volumeSliderPosition: position1to100 };
        });
    }

    private chooseMusicFile() {
        axios.get("/bloom/api/music/ui/chooseFile").then(result => {
            const fileName = result.data;
            if (!fileName) {
                return;
            }
            ToolboxToolReactAdaptor.setBloomPageAttr(
                MusicToolControls.musicAttrName,
                fileName
            );
            this.setState({
                activeRadioValue: "newMusic",
                audioEnabled: true,
                musicName: this.getDisplayNameOfMusicFile(fileName)
            });
        });
    }

    private getDisplayNameOfMusicFile(fileName: string) {
        return fileName.split(".")[0];
    }
}

export class MusicToolAdaptor extends ToolboxToolReactAdaptor {
    private controlsElement: MusicToolControls;
    public makeRootElement(): HTMLDivElement {
        return super.adaptReactElement(
            <MusicToolControls
                ref={renderedElement =>
                    (this.controlsElement = renderedElement)
                }
            />
        );
    }
    public id(): string {
        return "music";
    }
    public showTool() {
        this.controlsElement.showTool();
    }
    public hideTool() {
        this.controlsElement.hideTool();
    }
    public updateMarkup() {
        this.controlsElement.updateMarkup();
    }
}
