using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using AOT;
using UnityEngine;

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
        // [SerializeField, Header("Option")] private Texture _defaultTexture;
        private int _myGameObjectID;
        
        /// <summary>
        /// URLの初期設定
        /// </summary>
        /// <param name="videoFileURL"></param>
        private void InitializeVideoFileURL(string videoFileURL)
        {
            if (_isUseStreamingAssets)
                SetRelativeStreamingAssetURL(videoFileURL);
            else
                SetAbsoluteAssetURL(videoFileURL);
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

        private void Awake()
        {
            _myGameObjectID = gameObject.GetInstanceID();
            InitializeVideoFileURL(_videoFileURL);
        }

        /// <summary>
        /// ビデオタグの初期化
        /// </summary>
        public void Initialize()
        {
            InitializeVideo(_videoFileURL, _myGameObjectID);
            _isVideoInitialized = true;
        }

        /// <summary>
        /// 動画再生
        /// </summary>
        public void Play()
        {
            if (_isVideoInitialized)
            {
                StartVideo(_videoFileURL, 0, OnStarted, OnStopped, _myGameObjectID);
            }
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
            if (_isVideoInitialized && s_playingVideoGameObjectsID.Contains(_myGameObjectID))
            {
                RenderVideoTexture();
            }
            else
            {
                // Graphics.Blit(_defaultTexture, _targetRenderTexture);
            }
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