export const StartVideo = async (file, time, _onPublished, _onStopped) => {
    const elem = document.getElementById("video_screen");
    elem.setAttribute("style", "display:none;");
    elem.setAttribute("playsinline", "");
    elem.setAttribute("src", `StreamingAssets/${file}`);
    elem.muted = true;
    elem.play();

    // Unityの方に表示する
    elem.currentTime = time;
    if (isSafari()) {
        const unmute = () => {
            elem.muted = false;
        }
        document.getElementById("unity-canvas").addEventListener("touchstart", unmute);
        alert("画面をタップすると音声がでます。");
    } else {
        elem.muted = false;
    }

    _onPublished();

    elem.addEventListener('ended', (event) => {
        _onStopped();
    });
};
