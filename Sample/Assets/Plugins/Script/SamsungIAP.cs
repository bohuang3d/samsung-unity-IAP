using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class SamsungIAP : MonoBehaviour
{
    public static SamsungIAP instance;
    private AndroidJavaObject activityContext;
    private string savedPassthroughParam = "";

    private System.Action<OwnedProductList> onGetOwenedListListener;
    private System.Action<ProductInfoList> onGetProductsDetailsListener;
    private System.Action<PurchaseVo> onStartPaymentListener;
    private System.Action<ConsumedList> onConsumePurchasedItemListener;

    void Awake()
    {
        instance = this;
        using (AndroidJavaClass cls = new AndroidJavaClass("com.unity3d.player.UnityPlayer"))
        {
            activityContext = cls.GetStatic<AndroidJavaObject>("currentActivity");
            SetOperationMode(OperationMode.OPERATION_MODE_TEST);
        }
    }

    public void Start()
    {

    }

    public void SetOperationMode(OperationMode mode)
    {
        if (activityContext != null)
            activityContext.Call("setOperationMode", this.name, mode.ToString());
    }

    public void GetProductsDetails(string itemIDs, System.Action<ProductInfoList> listener)
    {
        onGetProductsDetailsListener = listener;
        if (activityContext != null)
            activityContext.Call("getProductDetails", itemIDs);
    }

    public void GetOwnedList(ItemType itemType, System.Action<OwnedProductList> listener)
    {
        onGetOwenedListListener = listener;

        if (activityContext != null)
            activityContext.Call("getOwnedList", itemType.ToString());
    }

    public void StartPayment(string itemID, string passThroughParam, System.Action<PurchaseVo> listener)
    {
        savedPassthroughParam = passThroughParam;
        onStartPaymentListener = listener;

        if (activityContext != null)
            activityContext.Call("startPayment", itemID, passThroughParam, "false");
    }

    public void ConsumePurchasedItems(string purchaseIDs, System.Action<ConsumedList> listener)
    {
        onConsumePurchasedItemListener = listener;

        if (activityContext != null)
            activityContext.Call("consumePurchasedItems", purchaseIDs);
    }

    public void OnGetProductsDetails(string resultJSON)
    {
        ProductInfoList productList = JsonUtility.FromJson<ProductInfoList>(resultJSON);

        for (int i = 0; i < productList.results.Count; ++i)
            Debug.Log("onGetProductsDetails: " + productList.results[i].mItemName);

        if (onGetProductsDetailsListener != null)
            onGetProductsDetailsListener(productList);
    }

    public void OnGetOwnedProducts(string resultJSON)
    {
        OwnedProductList ownedList = JsonUtility.FromJson<OwnedProductList>(resultJSON);

        for (int i = 0; i < ownedList.results.Count; ++i)
        {
            Debug.Log("onGetOwnedProducts: " + ownedList.results[i].mItemName);
        }

        if (onGetOwenedListListener != null)
            onGetOwenedListListener(ownedList);
    }

    public void OnConsumePurchasedItems(string resultJSON)
    {
        ConsumedList consumedList = JsonUtility.FromJson<ConsumedList>(resultJSON);

        for (int i = 0; i < consumedList.results.Count; ++i)
        {
            Debug.Log("OnConsumePurchasedItems: " + consumedList.results[i].mPurchaseId);
        }

        if (onConsumePurchasedItemListener != null)
            onConsumePurchasedItemListener(consumedList);
    }

    public void OnPayment(string resultJSON)
    {
        Debug.Log("onPayment: " + resultJSON);

        PurchaseVo purchasedInfo = JsonUtility.FromJson<PurchaseVo>(resultJSON);

        if (purchasedInfo.mPassThroughParam != savedPassthroughParam)
            Debug.Log("passthroughParam is not matched.");
        else
        {
            /*
            if (purchasedInfo.mConsumableYN == "Y")
                ConsumePurchasedItems(purchasedInfo.mPurchaseId);
            */
            if (onStartPaymentListener != null)
                onStartPaymentListener(purchasedInfo);
        }
    }

    public void OnError(string msg)
    {
        Debug.Log("Galaxy Apps IAP Error: " + msg);
    }

    public enum OperationMode
    {
        OPERATION_MODE_TEST_FAILURE,
        OPERATION_MODE_PRODUCTION,
        OPERATION_MODE_TEST
    }

    public enum ItemType
    {
        item,
        subscription,
        all
    }

    [System.Serializable]
    public class ProductInfoList
    {
        public List<ProductVo> results;
    }

    [System.Serializable]
    public class ProductVo
    {
        public string mItemId = "";
        public string mItemName = "";
        public string mItemPrice = "";
        public string mItemPriceString = "";
        public string mCurrencyUnit = "";
        public string mCurrencyCode = "";
        public string mItemDesc = "";
        public string mItemImageUrl = "";
        public string mItemDownloadUrl = "";
        public string mReserved1 = "";
        public string mReserved2 = "";
        public string mType = "";
        public string mConsumableYN = "";
        public string mFreeTrialPeriod = "";
        public string mSubscriptionDurationUnit = "";
        public string mSubscriptionDurationMultiplier = "";
    }

    public class OwnedProductList
    {
        public List<OwnedProductVo> results;
    }

    [System.Serializable]
    public class OwnedProductVo
    {
        public string mItemId = "";
        public string mItemName = "";
        public string mItemPrice = "";
        public string mItemPriceString = "";
        public string mCurrencyUnit = "";
        public string mCurrencyCode = "";
        public string mItemDesc = "";
        public string mType = "";
        public string mConsumableYN = "";
        public string mSubscriptionEndDate = "";
        public string mPaymentId = "";
        public string mPurchaseId = "";
        public string mPurchaseDate = "";
        public string mPassThroughParam = "";
    }

    [System.Serializable]
    public class PurchaseVo
    {
        public string mItemId = "";
        public string mItemName = "";
        public string mItemDesc = "";
        public string mItemPrice = "";
        public string mItemPriceString = "";
        public string mType = "";
        public string mConsumableYN = "";
        public string mCurrencyUnit = "";
        public string mCurrencyCode = "";
        public string mItemImageUrl = "";
        public string mItemDownloadUrl = "";
        public string mReserved1 = "";
        public string mReserved2 = "";
        public string mPaymentId = "";
        public string mPurchaseDate = "";
        public string mPurchaseId = "";
        public string mPassThroughParam = "";
        public string mVerifyUrl = "";
    }

    public class ConsumedList
    {
        public List<ConsumeVo> results;
    }

    [System.Serializable]
    public class ConsumeVo
    {
        public string mPurchaseId = "";
        public string mStatusString = "";
        public string mStatusCode = "";
    }
}

