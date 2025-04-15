using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using ZXing;
using UnityEngine.UI;
using ZXing.Common;

public class BarcodeScanner : MonoBehaviour
{
    private WebCamTexture webcamTexture;
    private bool          isScanning;

    [Header("Optional Display")] public RawImage          cameraView;
    public                              AspectRatioFitter aspectFitter;
    
    /// <summary>
    /// Starts scanning and returns the first barcode found.
    /// </summary>
    public IEnumerator ScanBarcode(System.Action<string> onBarcodeFound)
    {
        yield return Application.RequestUserAuthorization(UserAuthorization.WebCam);
        if (!Application.HasUserAuthorization(UserAuthorization.WebCam))
        {
            Debug.LogError("Camera permission denied.");
            yield break;
        }

        WebCamDevice device = WebCamTexture.devices[0];
        webcamTexture = new WebCamTexture(device.name, 1280, 720);

        webcamTexture.Play(); // Start first
        isScanning = true;

        if (cameraView != null)
        {
            cameraView.texture = webcamTexture;

            // Apply after Play()
            cameraView.rectTransform.localEulerAngles = new Vector3(0, 0, -webcamTexture.videoRotationAngle);

            if (webcamTexture.videoVerticallyMirrored)
            {
                cameraView.rectTransform.localScale = new Vector3(1, -1, 1);
            }
            else
            {
                cameraView.rectTransform.localScale = new Vector3(1, 1, 1);
            }
        }

        IBarcodeReader reader = new BarcodeReader();
        reader.Options = new DecodingOptions
        {
            TryHarder = true,
            PossibleFormats = new List<BarcodeFormat>
            {
                BarcodeFormat.EAN_13,
                BarcodeFormat.UPC_A,
                BarcodeFormat.CODE_128
            }
        };

        while (isScanning)
        {
            if (webcamTexture.width > 100)
            {
                try
                {
                    // Optionally update UI aspect ratio
                    if (cameraView != null && aspectFitter != null)
                    {
                        float ratio = (float)webcamTexture.width / webcamTexture.height;
                        aspectFitter.aspectRatio = ratio;
                    }

                    // Try to decode
                    var snap = new Texture2D(webcamTexture.width, webcamTexture.height, TextureFormat.RGBA32, false);
                    snap.SetPixels32(webcamTexture.GetPixels32());
                    snap.Apply();
                    
                    

                    var result = reader.Decode(snap.GetPixels32(), snap.width, snap.height);
                    if (result != null)
                    {
                        isScanning = false;

                        if (webcamTexture != null && webcamTexture.isPlaying)
                            webcamTexture.Stop();

                        if (cameraView != null)
                            cameraView.texture = null;

                        onBarcodeFound?.Invoke(result.Text);
                        yield break;
                    }
                }
                catch
                {
                }
            }

            yield return new WaitForSeconds(0.3f);
        }
    }

    public void StopScanning()
    {
        isScanning = false;

        if (webcamTexture != null && webcamTexture.isPlaying)
            webcamTexture.Stop();

        if (cameraView != null)
            cameraView.texture = null;

        Debug.Log("📷 Scanning manually stopped.");
    }
}