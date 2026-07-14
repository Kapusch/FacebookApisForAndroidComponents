package com.kapusch.facebook.androidinterop;

import android.app.Activity;
import android.content.Intent;
import android.graphics.Bitmap;
import android.graphics.BitmapFactory;
import android.os.Bundle;

import com.facebook.CallbackManager;
import com.facebook.FacebookCallback;
import com.facebook.FacebookException;
import com.facebook.share.Sharer;
import com.facebook.share.model.SharePhoto;
import com.facebook.share.model.SharePhotoContent;
import com.facebook.share.widget.ShareDialog;

public final class FacebookShareActivity extends Activity {
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
		if (!dialog.canShow(content, ShareDialog.Mode.AUTOMATIC)) {
			completeFailed("share_dialog_unavailable");
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
