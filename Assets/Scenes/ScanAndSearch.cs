using UnityEngine;
using UnityEngine.UI;

public class ScanAndSearch : MonoBehaviour
{
    [SerializeField] BarcodeScanner barcodeScanner;
    
    [SerializeField] Button scanButton;

    [SerializeField] Button CancelButton;
    
    [SerializeField] OpenFoodFactsFetcher fetcher;

    [SerializeField] FoodSearch foodSearch;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        scanButton.onClick.AddListener(StartScanning);
        CancelButton.onClick.AddListener(CancelScanning);
    }

    void CancelScanning()
    {
        barcodeScanner.StopScanning();
    }

    void StartScanning()
    {
        StartCoroutine(barcodeScanner.ScanBarcode(BarcodeFound));
    }
    async void BarcodeFound(string barcode)
    {
        scanButton.gameObject.SetActive(true);
        CancelButton.gameObject.SetActive(false);
        Debug.Log("Barcode found: " + barcode);
        FoodItem food = await fetcher.GetProductByBarcodeAsync(barcode);
        foodSearch.AddPreview(food);
        // Here you can call the search function with the barcode
        // For example: SearchByBarcode(barcode);
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
