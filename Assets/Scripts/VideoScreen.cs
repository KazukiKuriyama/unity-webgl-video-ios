using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using AOT;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;
using Object = UnityEngine.Object;

public class MainScreen : MonoBehaviour
{
    // 再生を開始するための関数
    [DllImport("__Internal")]
    private static extern void StartVideo(string file, int time, Action<int> onStarted, Action<int> onStopped, int instanceID);

    [DllImport("__Internal")]
    private static extern void InitVideo(string file,int gameObjectID);

    // 描画するための関数
    [DllImport("__Internal")]
    private static extern void UpdateVideoTexture(int texture);

    private static bool _isVideoInitialized = false;
    private static bool _isStartVideoInitialized = false;

    [SerializeField] private string _videoFilePath;
    private static bool _videoPlaying = false;
    private Texture2D _videoTexture;
    [SerializeField] private Material _targetMaterial;
    [SerializeField] private Texture _defaultTexture;
    private static List<int> _playingVideoGameObjectsID = new List<int>();

    private int _myGameObjectID;

    private void Awake()
    {
        _myGameObjectID = gameObject.GetInstanceID();
        if (!_isStartVideoInitialized)
        {
            var videoPath = Application.streamingAssetsPath + "/" + _videoFilePath;
            _isStartVideoInitialized = true;
            InitVideo(videoPath);
            Debug.Log("_isVideoInitialized = true;");
            _isVideoInitialized = true;
        }
    }


    private async void Start()
    {
        await WaitForVideoInitialized();
    }

    // 何かボタンが押されたら動画を再生する
    private void VideoTest()
    {
        // bbb.mp4 の 1:52 から再生を開始する。
        var videoPath = Application.streamingAssetsPath + "/" + _videoFilePath;
        Debug.Log(videoPath);

        StartVideo(videoPath, 0, OnStarted, OnStopped, _myGameObjectID);
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
            Debug.Log("_isVideoInitialized && _playingVideoGameObjectsID.Contains(_myGameObjectID)");
            if (_videoTexture)
            {
                Destroy(_videoTexture);
            }

            _videoTexture = new Texture2D(1, 1, TextureFormat.ARGB32, false); // jslib側で再生成されるので空で良い
            UpdateVideoTexture((int)_videoTexture.GetNativeTexturePtr());
            _targetMaterial.mainTexture = _videoTexture;
        }
        else
        {
            _targetMaterial.mainTexture = _defaultTexture;
        }
    }
}