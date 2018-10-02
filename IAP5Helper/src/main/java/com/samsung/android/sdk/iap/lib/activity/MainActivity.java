package com.samsung.android.sdk.iap.lib.activity;

import android.os.Bundle;
import android.util.Log;

import com.samsung.android.sdk.iap.lib.R;
import com.samsung.android.sdk.iap.lib.helper.HelperDefine;
import com.samsung.android.sdk.iap.lib.helper.IapHelper;
import com.samsung.android.sdk.iap.lib.listener.OnConsumePurchasedItemsListener;
import com.samsung.android.sdk.iap.lib.listener.OnGetOwnedListListener;
import com.samsung.android.sdk.iap.lib.listener.OnGetProductsDetailsListener;
import com.samsung.android.sdk.iap.lib.listener.OnPaymentListener;
import com.samsung.android.sdk.iap.lib.vo.ConsumeVo;
import com.samsung.android.sdk.iap.lib.vo.ErrorVo;
import com.samsung.android.sdk.iap.lib.vo.OwnedProductVo;
import com.samsung.android.sdk.iap.lib.vo.ProductVo;
import com.samsung.android.sdk.iap.lib.vo.PurchaseVo;
import com.unity3d.player.UnityPlayer;
import com.unity3d.player.UnityPlayerActivity;

import java.util.ArrayList;


public class MainActivity extends UnityPlayerActivity
{
    private final String TAG = MainActivity.class.getSimpleName();
    private static HelperDefine.OperationMode IAP_MODE = HelperDefine.OperationMode.OPERATION_MODE_PRODUCTION;

    private IapHelper mIapHelper = null;
    private String mCallback = "";

    // Setup activity layout
    @Override protected void onCreate(Bundle savedInstanceState)
    {
        super.onCreate(savedInstanceState);

        mIapHelper = IapHelper.getInstance( this.getApplicationContext() );
    }

    @Override
    protected void onRestart() {
        setContentView(R.layout.base_dialog);
        Log.v("IAP", "OnRestart.............................................");
        super.onRestart();
    }

    void setOperationMode(String objName, String _mode)
    {
        Log.v("IAP", "setOperationMode: " + _mode + ", callback object: " + objName);

        mCallback = objName;

        if(_mode.equals("OPERATION_MODE_TEST_FAILURE"))
            mIapHelper.setOperationMode(HelperDefine.OperationMode.OPERATION_MODE_TEST_FAILURE);
        else if(_mode.equals("OPERATION_MODE_TEST"))
            mIapHelper.setOperationMode(HelperDefine.OperationMode.OPERATION_MODE_TEST);
        else
            mIapHelper.setOperationMode(HelperDefine.OperationMode.OPERATION_MODE_PRODUCTION);
    }

    void getProductDetails(String itemIDs)
    {
        Log.v("IAP", "getProductDetails: " + itemIDs);

        mIapHelper.getProductsDetails(itemIDs, mOnGetProductsDetailsListener );
    }

    OnGetProductsDetailsListener mOnGetProductsDetailsListener = new OnGetProductsDetailsListener()
    {
        @Override
        public void onGetProducts(ErrorVo _errorVo, ArrayList<ProductVo> _ProductList )
        {
            Log.v(TAG, "onGetProducts");
            Log.v(TAG, "_errorVo.getErrorCode() : " + _errorVo.getErrorCode());

            if (_errorVo != null && _errorVo.getErrorCode() == IapHelper.IAP_ERROR_NONE)
            {
                String productJSON = "{ \"results\" : [";
                for( int i = 0; i < _ProductList.size(); ++i ) {
                    productJSON += _ProductList.get(i).getJsonString();
                    if( i+ 1 < _ProductList.size())
                        productJSON += ",\n";
                }
                productJSON += "]}";

                UnityPlayer.UnitySendMessage(mCallback, "OnGetProductsDetails", productJSON);
            }
            else
            {
                UnityPlayer.UnitySendMessage(mCallback, "OnError", _errorVo.getErrorString());
            }
        }
    };

    void getOwnedList(String _itemType)
    {
        Log.v("IAP", "getOwnedList: " + _itemType);

        mIapHelper.getOwnedList(_itemType, mOnGetOwnedListListener);
    }

    OnGetOwnedListListener mOnGetOwnedListListener = new OnGetOwnedListListener()
    {
        @Override
        public void onGetOwnedProducts(ErrorVo _errorVo, ArrayList<OwnedProductVo> _ownedList )
        {
            Log.v(TAG, "onGetOwnedProducts");

            if( _errorVo != null && _errorVo.getErrorCode() == IapHelper.IAP_ERROR_NONE )
            {
                String productJSON = "{ \"results\" : [";
                for( int i = 0; i < _ownedList.size(); ++i ) {
                    productJSON += _ownedList.get(i).getJsonString();
                    if( i+ 1 < _ownedList.size())
                        productJSON += ",\n";
                }
                productJSON += "]}";

//                Log.v(TAG, "productJSON = " + productJSON);

                UnityPlayer.UnitySendMessage(mCallback, "OnGetOwnedProducts", productJSON);
            }
            else
            {
                UnityPlayer.UnitySendMessage(mCallback, "OnError", _errorVo.getErrorString());
            }
        }
    };

    void startPayment(String _itemId, String _passthroughParam, String _showSuccessDialog)
    {
        Log.v("IAP", "startPayment: " + _itemId);

        if(_showSuccessDialog.equals("true"))
            mIapHelper.startPayment(_itemId, _passthroughParam, true, onPaymentListener);
        else
            mIapHelper.startPayment(_itemId, _passthroughParam, false, onPaymentListener);
    }

    OnPaymentListener onPaymentListener = new OnPaymentListener()
    {
        @Override
        public void onPayment(ErrorVo _errorVo, PurchaseVo _purchaseVo )
        {
            if (_errorVo != null && _errorVo.getErrorCode() == IapHelper.IAP_ERROR_NONE)
            {
                UnityPlayer.UnitySendMessage(mCallback, "OnPayment", _purchaseVo.getJsonString());
            }
            else
            {
                UnityPlayer.UnitySendMessage(mCallback, "OnError", _errorVo.getErrorString());
            }
        }
    };

    void consumePurchasedItems(String _purchaseId)
    {
        Log.v("IAP", "consumePurchasedItems: " + _purchaseId);

        mIapHelper.consumePurchasedItems(_purchaseId, onConsumePurchasedItemsListener);
    }

    OnConsumePurchasedItemsListener onConsumePurchasedItemsListener = new OnConsumePurchasedItemsListener()
    {
        @Override
        public void onConsumePurchasedItems( ErrorVo _errorVo, ArrayList<ConsumeVo> _consumeList )
        {
            if(_errorVo.getErrorCode() == IapHelper.IAP_ERROR_NONE)
            {
                String productJSON = "{ \"results\" : [";
                for( int i = 0; i < _consumeList.size(); ++i ) {
                    productJSON += _consumeList.get(i).getJsonString();
                    if( i+ 1 < _consumeList.size())
                        productJSON += ",\n";
                }
                productJSON += "]}";

                UnityPlayer.UnitySendMessage(mCallback, "OnConsumePurchasedItems", productJSON);
            }
            else
            {
                UnityPlayer.UnitySendMessage(mCallback, "OnError", _errorVo.getErrorString());
            }
        }
    };
}




