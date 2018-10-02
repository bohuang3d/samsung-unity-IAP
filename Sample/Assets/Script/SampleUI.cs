using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using UnityEngine;


public class SampleUI : MonoBehaviour 
{
	private const string KEY_BULLET_COUNT = "consumable";
	private const string KEY_GUN_LEVEL = "non-consumable";
	private const string KEY_INFINITE_BULLET = "ARS";

    private string passThroughParam = "";

    public Image shotImage, gunImage;
	public Sprite[] gunSprite;
	public Text bulletCountText, bulletMaxText, bulletInfiniteText;
	public GameObject productDetail;

	private AndroidJavaObject activityContext;
	private int bulletCount = 5;
	private int gunLevel = 0;
	private bool infiniteBullet = false;

	void Awake() 
	{
		Screen.SetResolution (720, 1280, false);
    }

	void Start() 
	{
        bulletCount = PlayerPrefs.GetInt(KEY_BULLET_COUNT, 5);
        gunLevel = PlayerPrefs.GetInt(KEY_GUN_LEVEL, 0);
        infiniteBullet = (PlayerPrefs.GetInt(KEY_INFINITE_BULLET, 0) == 1);

        bulletCountText.text = "" + bulletCount;
        gunImage.sprite = gunSprite[gunLevel];
        SetInfiniteMode(infiniteBullet);

        SamsungIAP.instance.SetOperationMode(SamsungIAP.OperationMode.OPERATION_MODE_TEST);
        SamsungIAP.instance.GetOwnedList(SamsungIAP.ItemType.all, OnGetOwnedProducts);
    }

    private void OnGetOwnedProducts(SamsungIAP.OwnedProductList ownedList)
	{
        int gunLevel = 0;
        bool infiniteBullet = false;
        string unconsumedItems = "";

        ownedList.results.ForEach((item) =>
		{
            if (item.mConsumableYN == "Y")
            {
                if (unconsumedItems != "") unconsumedItems += ", ";
                unconsumedItems += item.mPurchaseId;
            }
            else if (item.mItemId.Equals(KEY_GUN_LEVEL)) gunLevel = 1;
            else if (item.mItemId.Equals(KEY_INFINITE_BULLET)) infiniteBullet = true;
		});

        if( gunLevel != this.gunLevel ) SetGunLevel(gunLevel);
        if (infiniteBullet != this.infiniteBullet) SetInfiniteMode(infiniteBullet);

        if ( unconsumedItems != "" )
            SamsungIAP.instance.ConsumePurchasedItems(unconsumedItems, OnConsumePurchasedItemListener);
	}

	void OnEnable() 
	{
        Debug.Log("OnEnable");

        shotImage.gameObject.SetActive(false);
    }

    void SaveData()
    {
        PlayerPrefs.SetInt(KEY_BULLET_COUNT, bulletCount);
        PlayerPrefs.SetInt(KEY_GUN_LEVEL, gunLevel);
        PlayerPrefs.SetInt(KEY_INFINITE_BULLET, infiniteBullet ? 1 : 0);
        PlayerPrefs.Save();
    }

    void OnDisable() 
	{
        Debug.Log("OnDisable");
        SaveData();
	}

	void Update () 
	{
        if (Input.GetKeyUp(KeyCode.Escape)) Application.Quit();
	}

    void OnApplicationPause(bool pauseStatus)
    {
        if (pauseStatus == true)
        {
            Debug.Log("OnApplicationPause");
            SaveData();
        }
    }

    public void OnClick_Shot() 
	{
		if (bulletCount <= 0) 
		{
			ShowToast ("You are out of bullets! Try get some!");
			return;
		}

        if( !infiniteBullet ) UpdateBulletCount(-1);

		shotImage.gameObject.SetActive(true);
		shotImage.canvasRenderer.SetAlpha(1.0f);
		shotImage.CrossFadeAlpha (0, 1.0f, true);
	}

	public void OnClick_GetBullet()
	{
		if (bulletCount >= 5 || infiniteBullet) {
			ShowToast ("You already have max bullets!");
			return;
		}

		SamsungIAP.instance.StartPayment(KEY_BULLET_COUNT, passThroughParam, OnPayment_GetBullet);
	}

    private void OnPayment_GetBullet(SamsungIAP.PurchaseVo purchasedInfo)
    {
        SamsungIAP.instance.ConsumePurchasedItems(purchasedInfo.mPurchaseId, OnConsumePurchasedItemListener);
        UpdateBulletCount(1);
    }

    public void OnClick_UpgradeGun()
	{
        if (gunLevel == 1)
        {
            ShowToast("You already have upgraded gun!");
            return;
        }

        SamsungIAP.instance.StartPayment(KEY_GUN_LEVEL, passThroughParam, OnPayment_UpgradeGun);
	}

    private void OnPayment_UpgradeGun(SamsungIAP.PurchaseVo purchasedInfo)
    {
        SetGunLevel(1);
    }
    
    public void OnClick_GetInfiniteBullets()
	{
        if (infiniteBullet)
        {
            ShowToast("You already have infinte bullets!");
            return;
        }

        SamsungIAP.instance.StartPayment(KEY_INFINITE_BULLET, passThroughParam, OnPayment_GetInfinteBullets);
	}

    private void OnPayment_GetInfinteBullets(SamsungIAP.PurchaseVo purchasedInfo)
    {
        SetInfiniteMode(true);
    }

    private void OnConsumePurchasedItemListener(SamsungIAP.ConsumedList consumedList)
    {
        
    }

    public void OnClick_ProductDetail()
	{
		productDetail.SetActive(true);
		gameObject.SetActive(false);
	}

	private void UpdateBulletCount(int offset)
	{
		bulletCount = Mathf.Clamp(bulletCount + offset, 0, 6);
		bulletCountText.text = "" + bulletCount;
	}

	private void SetGunLevel(int gunLevel)
	{
		this.gunLevel = gunLevel;
		gunImage.sprite = gunSprite[gunLevel];
	}

	private void SetInfiniteMode(bool mode)
	{
		infiniteBullet = mode;
		bulletCountText.gameObject.SetActive (!mode);
		bulletMaxText.gameObject.SetActive (!mode);
		bulletInfiniteText.gameObject.SetActive (mode);
	}

    private void ShowToast(string msg)
	{
		if (activityContext == null) 
		{
			using (AndroidJavaClass cls = new AndroidJavaClass("com.unity3d.player.UnityPlayer"))
			{
				activityContext = cls.GetStatic<AndroidJavaObject>("currentActivity");
			}
		}

		activityContext.Call ("runOnUiThread", new AndroidJavaRunnable (() => {
			AndroidJavaObject toast = new AndroidJavaObject ("android.widget.Toast", activityContext);
			toast.CallStatic<AndroidJavaObject> ("makeText", activityContext, msg, 0).Call ("show");
		}));
	}
}
