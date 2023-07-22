
UpdateVideoTexture = {
    UpdateVideoTexture: function (tex) {

        // set texture
        GLctx.deleteTexture(GL.textures[tex]);
        
        var t = GLctx.createTexture();
        // console.log(t);
        t.name = tex;
        GL.textures[tex] = t;
        // console.log(GL);

        elem = document.getElementById("video_screen");
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

mergeInto(LibraryManager.library, UpdateVideoTexture);
