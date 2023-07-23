WebGLVideo = {
    InitializeVideo: function (filePath, gameObjectId) {
        const strFilePath = UTF8ToString(filePath);
        const elem = document.createElement("video");
        elem.id = getVideoScreenId(gameObjectId);
        document.getElementById("unity-container").appendChild(elem);
        elem.setAttribute("style", "display:none;");
        elem.setAttribute("playsinline", "");
        elem.setAttribute("src", strFilePath);
        elem.muted = false;
    },
    StartVideo: function (filePath, time, _onStarted, _onStopped, gameObjectId) {
        const strFilePath = UTF8ToString(filePath);
        var currentVideoElementId = getVideoScreenId(gameObjectId);
        const elem = document.getElementById(currentVideoElementId);
        elem.play();
        // Unityの方に表示する
        elem.currentTime = time;
        Module['dynCall_vi'](_onStarted, gameObjectId);

        elem.addEventListener('ended', (event) => {
            Module['dynCall_vi'](_onStopped, gameObjectId);
        });
    },
    UpdateVideoTexture: function (tex, gameObjectId) {

        // set texture
        GLctx.deleteTexture(GL.textures[tex]);

        var t = GLctx.createTexture();
        // console.log(t);
        t.name = tex;
        GL.textures[tex] = t;
        // console.log(GL);

        var currentVideoElementId = getVideoScreenId(gameObjectId);
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
    $isIOSSafari: function () {
        return /^((?!chrome|android).)*safari/i.test(navigator.userAgent);
    },
    $webVideoFlags: { IsClicked: false },
    $startVideoFancs: {},
    $getVideoScreenId: function (gameObjectId) {
        return "video_screen_" + gameObjectId;
    },
}
autoAddDeps(WebGLVideo, '$getVideoScreenId');
autoAddDeps(WebGLVideo, '$webVideoFlags');
autoAddDeps(WebGLVideo, '$startVideoFancs');
autoAddDeps(WebGLVideo, '$isIOSSafari');
mergeInto(LibraryManager.library, WebGLVideo);