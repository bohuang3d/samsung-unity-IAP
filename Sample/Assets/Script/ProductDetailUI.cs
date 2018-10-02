using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ProductDetailUI : MonoBehaviour {

	public SampleUI sampleUI;
	public ProductDetailItemUI itemPrefab;
	public GameObject itemRoot, emptyText;
	public RectTransform content;

	void OnEnable() 
	{
		content.anchoredPosition3D = Vector3.zero;
		emptyText.SetActive(true);
		SamsungIAP.instance.GetProductsDetails("consumable,non-consumable,ARS", OnGetProducts);
	}

	private void OnGetProducts(SamsungIAP.ProductInfoList productList)
	{
		if (gameObject.activeSelf == false)
			return;
		
		productList.results.ForEach((item) =>
			{
				ProductDetailItemUI itemUI = Instantiate (itemPrefab, itemRoot.transform, false) as ProductDetailItemUI;
				itemUI.productName.text = item.mItemName;
				itemUI.productPrice.text = item.mItemPriceString;
				itemUI.productDescription.text = item.mItemDesc;

				string productType = "Type : ";
				if("item".Equals(item.mType)) productType += "item";
				else if("subcription".Equals(item.mType)) productType += "subcription";
				else productType += "Unsupported type";
				itemUI.productType.text = productType;
			});
		
		emptyText.SetActive(false);
	}

	void OnDisable()
	{
		int childs = itemRoot.transform.childCount;
		for (int i = childs - 1; i >= 0; i--)
		{
			GameObject.Destroy(itemRoot.transform.GetChild(i).gameObject);
		}
		emptyText.SetActive(false);
	}

	void Update () 
	{
		if (Input.GetKeyUp (KeyCode.Escape)) 
		{
			gameObject.SetActive (false);
			sampleUI.gameObject.SetActive(true);
		}
	}
}
