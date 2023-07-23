using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using AOT;
using UnityEngine;
using UnityEngine.Video;

namespace WebGLVideo
{
    public class WebGLVideo : MonoBehaviour
    {
        // static
        private static List<int> s_playingVideoGameObjectsID = new List<int>();
        private static bool s_wasDisplayMutePopUp = false;

        // 再生を開始するための関数
        [DllImport("__Internal")]
        private static extern void StartVideo(string file, int time, Action<int> onStarted, Action<int> onStopped,
            int instanceID);

        [DllImport("__Internal")]
        private static extern void InitializeVideo(string file, int gameObjectID);

        // 描画するための関数
        [DllImport("__Internal")]
        private static extern void UpdateVideoTexture(int texture, int gameObjectID);

        /// <summary> ビデオ再生準備完了フラグ </summary>
        private bool _isVideoInitialized;

        /// <summary> StreamingAssetsからの相対パスを利用するかどうか。Inspectorの設定値を利用する場合のみ利用する </summary>
        [SerializeField] private bool _isUseStreamingAssets;

        [SerializeField, Header("Relative URL from StreamingAssets or absolute path")]
        private string _videoFileURL;

        Texture2D _videoTexture;

        [SerializeField] private RenderTexture _targetRenderTexture;
        [SerializeField] private bool _playOnAwake;

        // [SerializeField, Header("Option")] private Texture _defaultTexture;
        private int _myGameObjectID;


        // Videoコンポーネント対応（UnityEditor動作用）
        private VideoPlayer _videoPlayer;

        private void Awake()
        {
#if UNITY_WEBGL && !UNITY_EDITOR
            _myGameObjectID = gameObject.GetInstanceID();
            InitializeVideoFileURL(_videoFileURL);
#else
            VideoPlayerInitialize();
#endif
        }

        /// <summary>
        ///     ブラウザ以外時に利用するVideoPlayerコンポーネントの初期設定
        /// </summary>
        private void VideoPlayerInitialize()
        {
            _videoPlayer = gameObject.AddComponent<VideoPlayer>();
            _videoPlayer.playOnAwake = _playOnAwake; // 必ずurl設定する前に設定しておく
            _videoPlayer.targetTexture = _targetRenderTexture;
            _videoPlayer.url = InitializeVideoFileURL(_videoFileURL);
            _isVideoInitialized = true;
        }

        /// <summary>
        /// ビデオタグの初期化
        /// </summary>
        public void Initialize()
        {
#if UNITY_WEBGL && !UNITY_EDITOR
            InitializeVideo(_videoFileURL, _myGameObjectID);
            _isVideoInitialized = true;
#else
            ; // ブラウザ以外では実行する必要なし
#endif
        }

        /// <summary>
        /// URLの初期設定
        /// </summary>
        /// <param name="videoFileURL"></param>
        private string InitializeVideoFileURL(string videoFileURL)
        {
            string url;
            if (_isUseStreamingAssets)
                url = SetRelativeStreamingAssetURL(videoFileURL);
            else
                url = SetAbsoluteAssetURL(videoFileURL);
            return url;
        }

        /// <summary>
        /// 相対パス設定
        /// </summary>
        /// <param name="videoFileURL"></param>
        /// <returns></returns>
        public string SetRelativeStreamingAssetURL(string videoFileURL)
        {
            _isUseStreamingAssets = true;
            _videoFileURL = Path.Combine(Application.streamingAssetsPath, videoFileURL);
            return _videoFileURL;
        }

        /// <summary>
        /// 絶対パス設定
        /// </summary>
        /// <param name="videoFileURL"></param>
        /// <returns></returns>
        public string SetAbsoluteAssetURL(string videoFileURL)
        {
            _isUseStreamingAssets = false;
            _videoFileURL = videoFileURL;
            return _videoFileURL;
        }

        /// <summary>
        /// 動画再生
        /// </summary>
        public void Play()
        {
#if UNITY_WEBGL && !UNITY_EDITOR
            if (_isVideoInitialized)
            {
                StartVideo(_videoFileURL, 0, OnStarted, OnStopped, _myGameObjectID);
            }
#else
            _videoPlayer.Play();
#endif
        }

        /// <summary>
        /// 再生停止
        /// </summary>
        public void Stop()
        {
#if UNITY_WEBGL && !UNITY_EDITOR
            ;// to be developed　(video.pause)
#else
            _videoPlayer.Stop();
#endif
        }

        /// <summary>
        /// jslibから動画の初期化（初期タップ完了を検知）
        /// </summary>
        [MonoPInvokeCallback(typeof(Action))]
        private static void OnInitialized()
        {
            Debug.Log("OnVideoInitialized");
        }

        /// <summary>
        /// 動画再生開始イベントハンドラ
        /// </summary>
        [MonoPInvokeCallback(typeof(Action))]
        private static void OnStarted(int gameObjectID)
        {
            Debug.Log("OnStarted");
            if (!s_playingVideoGameObjectsID.Contains(gameObjectID))
            {
                s_playingVideoGameObjectsID.Add(gameObjectID);
            }
        }

        /// <summary>
        /// 動画停止イベントハンドラ
        /// </summary>
        [MonoPInvokeCallback(typeof(Action))]
        private static void OnStopped(int gameObjectID)
        {
            Debug.Log("OnStopped");
            s_playingVideoGameObjectsID.Remove(gameObjectID);
        }

        void Update()
        {
#if UNITY_WEBGL && !UNITY_EDITOR
            if (_isVideoInitialized && s_playingVideoGameObjectsID.Contains(_myGameObjectID))
            {
                RenderVideoTexture();
            }
            else
            {
                // Graphics.Blit(_defaultTexture, _targetRenderTexture);
            }
#endif
        }

        private void RenderVideoTexture()
        {
            if (_videoTexture)
                Destroy(_videoTexture);

            _videoTexture = new Texture2D(1, 1, TextureFormat.ARGB32, false);
            UpdateVideoTexture((int)_videoTexture.GetNativeTexturePtr(), _myGameObjectID);
            Graphics.Blit(_videoTexture, _targetRenderTexture);
        }
    }
}