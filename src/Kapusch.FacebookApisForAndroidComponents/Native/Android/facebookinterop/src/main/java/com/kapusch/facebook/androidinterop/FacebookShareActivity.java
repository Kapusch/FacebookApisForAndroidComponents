package com.kapusch.facebook.androidinterop;

import android.app.Activity;
import android.content.Intent;
import android.graphics.Bitmap;
import android.graphics.BitmapFactory;
import android.os.Bundle;
import android.util.Log;

import com.facebook.AccessToken;
import com.facebook.CallbackManager;
import com.facebook.FacebookCallback;
import com.facebook.FacebookException;
import com.facebook.FacebookSdk;
import com.facebook.share.Sharer;
import com.facebook.share.model.SharePhoto;
import com.facebook.share.model.SharePhotoContent;
import com.facebook.share.widget.ShareDialog;

public final class FacebookShareActivity extends Activity {
	private static final String LOG_TAG = "KapuschFacebookShare";
	private CallbackManager callbackManager;
	private Bitmap bitmap;

	@Override
	protected void onCreate(Bundle savedInstanceState) {
		super.onCreate(savedInstanceState);

		String imagePath = getIntent().getStringExtra(FacebookInteropConstants.EXTRA_SHARE_IMAGE_PATH);
		if (imagePath == null || imagePath.trim().isEmpty()) {
			completeFailed("missing_image_path");
			return;
		}

		bitmap = BitmapFactory.decodeFile(imagePath);
		if (bitmap == null) {
			completeFailed("image_decode_failed");
			return;
		}

		callbackManager = CallbackManager.Factory.create();
		ShareDialog dialog = new ShareDialog(this);
		dialog.registerCallback(callbackManager, new FacebookCallback<Sharer.Result>() {
			@Override
			public void onSuccess(Sharer.Result result) {
				complete("success", RESULT_OK, null);
			}

			@Override
			public void onCancel() {
				complete("cancelled", RESULT_CANCELED, null);
			}

			@Override
			public void onError(FacebookException error) {
				completeFailed(error.getClass().getSimpleName());
			}
		});

		SharePhoto photo = new SharePhoto.Builder().setBitmap(bitmap).build();
		SharePhotoContent content = new SharePhotoContent.Builder().addPhoto(photo).build();
		boolean automaticAvailable = dialog.canShow(content, ShareDialog.Mode.AUTOMATIC);
		boolean nativeAvailable = dialog.canShow(content, ShareDialog.Mode.NATIVE);
		boolean webAvailable = dialog.canShow(content, ShareDialog.Mode.WEB);
		boolean feedAvailable = dialog.canShow(content, ShareDialog.Mode.FEED);
		boolean nativePhotoCapabilityAvailable = ShareDialog.canShow(SharePhotoContent.class);
		boolean accessTokenActive = AccessToken.isCurrentAccessTokenActive();

		Log.i(
			LOG_TAG,
			"ShareDialog availability"
				+ " sdkInitialized=" + FacebookSdk.isInitialized()
				+ " accessTokenActive=" + accessTokenActive
				+ " automatic=" + automaticAvailable
				+ " native=" + nativeAvailable
				+ " nativePhotoCapability=" + nativePhotoCapabilityAvailable
				+ " web=" + webAvailable
				+ " feed=" + feedAvailable
		);

		if (!automaticAvailable) {
			completeFailed(
				"share_dialog_unavailable"
					+ "_native_" + availabilityFlag(nativeAvailable)
					+ "_web_" + availabilityFlag(webAvailable)
			);
			return;
		}
		dialog.show(content, ShareDialog.Mode.AUTOMATIC);
	}

	@Override
	protected void onActivityResult(int requestCode, int resultCode, Intent data) {
		super.onActivityResult(requestCode, resultCode, data);
		if (callbackManager != null) {
			callbackManager.onActivityResult(requestCode, resultCode, data);
		}
	}

	private static String availabilityFlag(boolean available) {
		return available ? "1" : "0";
	}

	private void completeFailed(String errorCode) {
		complete("failed", RESULT_OK, errorCode);
	}

	private void complete(String status, int resultCode, String errorCode) {
		Intent data = new Intent();
		data.putExtra(FacebookInteropConstants.EXTRA_STATUS, status);
		if (errorCode != null) {
			data.putExtra(FacebookInteropConstants.EXTRA_ERROR_CODE, errorCode);
		}
		setResult(resultCode, data);
		finish();
	}

	@Override
	protected void onDestroy() {
		if (bitmap != null && !bitmap.isRecycled()) {
			bitmap.recycle();
		}
		bitmap = null;
		super.onDestroy();
	}
}
