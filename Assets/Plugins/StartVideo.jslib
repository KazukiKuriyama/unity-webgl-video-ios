StartVideo = {
    $StartVideoFancs: {},
    InitVideo: function (filePath, gameObjectID) {
        const strFilePath = UTF8ToString(filePath);
        const elem = document.createElement("video");
        const id = "video_screen" + gameObjectID;
        elem.id = id;
        document.getElementById("unity-canvas").appendChild(elem);
        elem.setAttribute("style", "display:none;");
        elem.setAttribute("playsinline", "");
        elem.setAttribute("src", strFilePath);
        elem.muted = true;
        const unmute = () => {
            elem.muted = false;
        }
        document.getElementById("unity-canvas").addEventListener("touchstart", unmute);
        alert("画面をタップすると音声がでます。");
    },
    StartVideo: function (filePath, time, _onStarted, _onStopped, gameObjectID) {
        const strFilePath = UTF8ToString(filePath);
        var currentVideoElementId = "video_screen" + gameObjectID;
        const elem = document.getElementById(currentVideoElementId);
        elem.play();
        // console.log(elem);
        // Unityの方に表示する
        elem.currentTime = time;
        // if (isIOSSafari()) {
        //     const unmute = () => {
        //         elem.muted = false;
        //     }
        //     document.getElementById("unity-canvas").addEventListener("touchstart", unmute);
        //     alert("画面をタップすると音声がでます。");
        // } else {
        //     elem.muted = false;
        // }
        // C#のPiyo関数を呼ぶ
        Module['dynCall_vi'](_onStarted, gameObjectID);

        elem.addEventListener('ended', (event) => {
            Module['dynCall_vi'](_onStopped, gameObjectID);
        });
    },
    $isIOSSafari: function () {
        return /^((?!chrome|android).)*safari/i.test(navigator.userAgent);
    },
}
autoAddDeps(StartVideo, '$StartVideoFancs');
autoAddDeps(StartVideo, '$isIOSSafari');
mergeInto(LibraryManager.library, StartVideo);