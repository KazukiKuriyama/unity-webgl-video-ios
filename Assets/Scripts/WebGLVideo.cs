using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using AOT;
using UnityEngine;

public class WebGLVideo : MonoBehaviour
{
    // 再生を開始するための関数
    [DllImport("__Internal")]
    private static extern void StartVideo(string file, int time, Action<int> onStarted, Action<int> onStopped,
        int instanceID);

    [DllImport("__Internal")]
    private static extern void InitVideo(string file, int gameObjectID);

    // 描画するための関数
    [DllImport("__Internal")]
    private static extern void UpdateVideoTexture(int texture,int gameObjectID);

    private static bool _isVideoInitialized = false;
    private static bool _isStartVideoInitialized = false;

    [SerializeField] private bool _isUseStreamingAssets;

    [SerializeField, Header("Relative URL from StreamingAssets or absolute path")]
    private string _videoFileURL;

    private static bool _videoPlaying = false;
    private Texture2D _videoTexture;
    [SerializeField] private Material _targetMaterial;
    [SerializeField] private Texture _defaultTexture;
    private static List<int> _playingVideoGameObjectsID = new List<int>();
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
        _videoFileURL = Application.streamingAssetsPath + "/" + videoFileURL;
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
        InitVideo(_videoFileURL, _myGameObjectID);
        _isVideoInitialized = true;
    }


    private async void Start()
    {
        await WaitForVideoInitialized();
    }

    // 何かボタンが押されたら動画を再生する
    private void VideoTest()
    {
        StartVideo(_videoFileURL, 0, OnStarted, OnStopped, _myGameObjectID);
    }

    /// <summary>
    /// jslibから動画の初期化（初期タップ完了を検知）
    /// </summary>
    [MonoPInvokeCallback(typeof(Action))]
    private static void OnInitialized()
    {
        Debug.Log("OnVideoInitialized");
        _isVideoInitialized = true;
    }

    /// <summary>
    /// 動画再生開始イベントハンドラ
    /// </summary>
    [MonoPInvokeCallback(typeof(Action))]
    private static void OnStarted(int gameObjectID)
    {
        Debug.Log("OnStarted");
        if (!_playingVideoGameObjectsID.Contains(gameObjectID))
        {
            _playingVideoGameObjectsID.Add(gameObjectID);
        }
    }

    /// <summary>
    /// 動画停止イベントハンドラ
    /// </summary>
    [MonoPInvokeCallback(typeof(Action))]
    private static void OnStopped(int gameObjectID)
    {
        Debug.Log("OnStopped");
        _playingVideoGameObjectsID.Remove(gameObjectID);
    }

    /// <summary>
    /// 初期タップ検知待ち
    /// </summary>
    private async Task WaitForVideoInitialized()
    {
        while (!_isVideoInitialized)
        {
            await Task.Delay(100); // 100ミリ秒待つ
        }

        Debug.Log("InitializedEvent()");
        InitializedEvent();
    }

    /// <summary>
    /// 動画再生準備完了イベント
    /// </summary>
    void InitializedEvent()
    {
        VideoTest();
    }

    void Update()
    {
        if (_isVideoInitialized && _playingVideoGameObjectsID.Contains(_myGameObjectID))
        {
            if (_videoTexture)
            {
                Destroy(_videoTexture);
            }

            // jslibでtextureを書き込み返却後に描画する
            _videoTexture = new Texture2D(1, 1, TextureFormat.ARGB32, false);
            UpdateVideoTexture((int)_videoTexture.GetNativeTexturePtr(),_myGameObjectID);
            _targetMaterial.mainTexture = _videoTexture;
        }
        else
        {
            _targetMaterial.mainTexture = _defaultTexture;
        }
    }
}