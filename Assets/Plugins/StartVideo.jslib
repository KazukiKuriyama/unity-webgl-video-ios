StartVideo = {
    $StartVideoFancs: {},
    InitVideo: function (filePath, gameObjectID) {
        const strFilePath = UTF8ToString(filePath);
        const elem = document.createElement("video");
        const id = "video_screen_" + gameObjectID;
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
        var currentVideoElementId = "video_screen_" + gameObjectID;
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
    UpdateVideoTexture: function (tex, gameObjectID) {

        // set texture
        GLctx.deleteTexture(GL.textures[tex]);
        
        var t = GLctx.createTexture();
        // console.log(t);
        t.name = tex;
        GL.textures[tex] = t;
        // console.log(GL);

        var currentVideoElementId = "video_screen_" + gameObjectID;
        elem = document.getElementById(currentVideoElementId);
        // console.log(elem);
        // console.log(GLctx);
        // target, texture
        GLctx.bindTexture(GLctx.TEXTURE_2D, GL.textures[tex]);
        GLctx.pixelStorei(GLctx.UNPACK_FLIP_Y_WEBGL, true); // flip up down.
        GLctx.texParameteri(GLctx.TEXTURE_2D, GLctx.TEXTURE_WRAP_S, GLctx.CLAMP_TO_EDGE);
        GLctx.texParameteri(GLctx.TEXTURE_2D, GLctx.TEXTURE_WRAP_T, GLctx.CLAMP_TO_EDGE);
        GLctx.texParameteri(GLctx.TEXTURE_2D, GLctx.TEXTURE_MIN_FILTER, GLctx.LINEAR);
        GLctx.texImage2D(GLctx.TEXTURE_2D, 0, GLctx.RGBA, GLctx.RGBA, GLctx.UNSIGNED_BYTE, elem);
    },
}
autoAddDeps(StartVideo, '$StartVideoFancs');
autoAddDeps(StartVideo, '$isIOSSafari');
mergeInto(LibraryManager.library, StartVideo);