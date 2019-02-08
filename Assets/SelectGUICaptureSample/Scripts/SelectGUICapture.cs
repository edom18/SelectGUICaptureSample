using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

public class SelectGUICapture : MonoBehaviour
{
    public event System.Action<RenderTexture> OnCaptured;

    [SerializeField, Tooltip("GUIをレンダリングしているカメラ")]
    private Camera _guiCamera = null;

    [SerializeField, Tooltip("キャプチャするタイミング")]
    private CameraEvent _cameraEvent = CameraEvent.BeforeImageEffects;

    [SerializeField, Tooltip("合成時に無視されるUIのレイヤー")]
    private LayerMask _captureTargetLayer = -1;

    private Camera _mainCamera = null;
    private RenderTexture _buf = null;
    private CommandBuffer _commandBuffer = null;

    #region ### MonoBehaviour ###
    private void Awake()
    {
        CreateBuffer();
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.S))
        {
            TakeScreenshot();
        }
    }

    /// <summary>
    /// 動作確認用にGizmoでテクスチャを表示する
    /// </summary>
    private void OnGUI()
    {
        if (_buf == null) return;
        GUI.DrawTexture(new Rect(5f, 5f, Screen.width * 0.5f, Screen.height * 0.5f), _buf);
    }
    #endregion ### MonoBehaviour ###

    /// <summary>
    /// バッファを生成する
    /// </summary>
    private void CreateBuffer()
    {
        _buf = new RenderTexture(Screen.width, Screen.height, 0);

        _commandBuffer = new CommandBuffer();
        _commandBuffer.name = "CaptureScene";
        _commandBuffer.Blit(BuiltinRenderTextureType.CurrentActive, _buf);
    }

    /// <summary>
    /// スクリーンショットを撮影する
    /// </summary>
    public void TakeScreenshot()
    {
        AddCommandBuffer();

        StartCoroutine(WaitCapture());
    }

    /// <summary>
    /// コマンドバッファの処理を待つ
    /// </summary>
    private IEnumerator WaitCapture()
    {
        yield return new WaitForEndOfFrame();

        BlendGUI();

        if (OnCaptured != null)
        {
            OnCaptured.Invoke(_buf);
        }

        RemoveCommandBuffer();
    }

    /// <summary>
    /// GUI要素をブレンドする
    /// </summary>
    private void BlendGUI()
    {
        _guiCamera.targetTexture = _buf;

        int tmp = _guiCamera.cullingMask;
        _guiCamera.cullingMask = _captureTargetLayer;

        _guiCamera.Render();

        _guiCamera.cullingMask = tmp;

        _guiCamera.targetTexture = null;
    }

    /// <summary>
    /// メインカメラにコマンドバッファを追加する
    /// </summary>
    private void AddCommandBuffer()
    {
        if (_mainCamera == null)
        {
            _mainCamera = Camera.main;
        }

        _mainCamera.AddCommandBuffer(_cameraEvent, _commandBuffer);
    }

    /// <summary>
    /// メインカメラからコマンドバッファを削除する
    /// </summary>
    private void RemoveCommandBuffer()
    {
        if (_mainCamera == null)
        {
            return;
        }

        _mainCamera.RemoveCommandBuffer(_cameraEvent, _commandBuffer);
    }
} 
