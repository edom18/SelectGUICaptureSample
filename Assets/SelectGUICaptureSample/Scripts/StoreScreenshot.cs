using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class StoreScreenshot : MonoBehaviour
{
    [SerializeField]
    private SelectGUICapture _capture = null;

    private void Awake()
    {
        _capture.OnCaptured += Store;
    }

    private void Store(RenderTexture buffer)
    {
        // 対象バッファのサイズと同じTexture2Dを生成する

        int width = buffer.width;
        int height = buffer.height;
        Texture2D tex = new Texture2D(width, height, TextureFormat.RGB24, false);

        // 現在のアクティブなRenderTextureをバックアップしておく
        RenderTexture tmp = RenderTexture.active;

        // アクティブなバッファを引数のバッファに差し替える
        RenderTexture.active = buffer;

        // アクティブなバッファからピクセルを読み込む
        tex.ReadPixels(new Rect(0, 0, width, height), 0, 0);
        tex.Apply();

        // バックアップしていたものに戻す
        RenderTexture.active = tmp;

        // 読み込んだピクセルをPNGとしてエンコードする
        byte[] screenshot = tex.EncodeToPNG();

#if UNITY_EDITOR
        string savePath = Application.dataPath + "/../screenshot.png";
#else
        string savePath = Application.temporaryCachePath + "/screenshot-temp.png";
#endif
        Debug.LogFormat("Will store screenshot to '{0}'", savePath);

        // FileStreamを開いてバイト配列をファイルに書き込む
        using (FileStream fs = new FileStream(savePath, FileMode.Create, FileAccess.Write))
        {
            using (BinaryWriter bw = new BinaryWriter(fs))
            {
                bw.Write(screenshot);
                bw.Close();
            }
            fs.Close();
        }

        Destroy(tex);
    }
}
