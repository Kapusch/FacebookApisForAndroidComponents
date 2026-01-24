package com.kapusch.facebook.androidinterop;

import android.app.Activity;
import android.content.Intent;
import android.os.Bundle;

import com.facebook.FacebookCallback;
import com.facebook.FacebookException;
import com.facebook.login.LoginManager;
import com.facebook.login.LoginResult;

import com.facebook.CallbackManager;

import java.util.Arrays;

public final class FacebookLoginActivity extends Activity {
	private static final int STATUS_SUCCESS = 0;
	private static final int STATUS_CANCELLED = 1;
	private static final int STATUS_FAILED = 2;

	private CallbackManager callbackManager;

	@Override
	protected void onCreate(Bundle savedInstanceState) {
		super.onCreate(savedInstanceState);

		callbackManager = CallbackManager.Factory.create();

		LoginManager loginManager = LoginManager.getInstance();
		loginManager.registerCallback(callbackManager, new FacebookCallback<LoginResult>() {
			@Override
			public void onSuccess(LoginResult loginResult) {
				String accessToken = loginResult.getAccessToken() != null ? loginResult.getAccessToken().getToken() : null;
				String userId = loginResult.getAccessToken() != null ? loginResult.getAccessToken().getUserId() : null;

				if (accessToken == null || accessToken.isEmpty()) {
					completeFailed("missing_access_token", "Facebook access token is missing.");
					return;
				}

				Intent data = new Intent();
				data.putExtra(FacebookInteropConstants.EXTRA_STATUS, STATUS_SUCCESS);
				data.putExtra(FacebookInteropConstants.EXTRA_ACCESS_TOKEN, accessToken);
				data.putExtra(FacebookInteropConstants.EXTRA_USER_ID, userId);
				setResult(RESULT_OK, data);
				finish();
			}

			@Override
			public void onCancel() {
				Intent data = new Intent();
				data.putExtra(FacebookInteropConstants.EXTRA_STATUS, STATUS_CANCELLED);
				setResult(RESULT_CANCELED, data);
				finish();
			}

			@Override
			public void onError(FacebookException error) {
				completeFailed(error.getClass().getSimpleName(), error.getMessage());
			}

			private void completeFailed(String errorCode, String errorMessage) {
				Intent data = new Intent();
				data.putExtra(FacebookInteropConstants.EXTRA_STATUS, STATUS_FAILED);
				data.putExtra(FacebookInteropConstants.EXTRA_ERROR_CODE, errorCode);
				data.putExtra(FacebookInteropConstants.EXTRA_ERROR_MESSAGE, errorMessage);
				setResult(RESULT_OK, data);
				finish();
			}
		});

		try {
			loginManager.logInWithReadPermissions(this, Arrays.asList("public_profile", "email"));
		} catch (Exception ex) {
			Intent data = new Intent();
			data.putExtra(FacebookInteropConstants.EXTRA_STATUS, STATUS_FAILED);
			data.putExtra(FacebookInteropConstants.EXTRA_ERROR_CODE, ex.getClass().getSimpleName());
			data.putExtra(FacebookInteropConstants.EXTRA_ERROR_MESSAGE, ex.getMessage());
			setResult(RESULT_OK, data);
			finish();
		}
	}

	@Override
	protected void onActivityResult(int requestCode, int resultCode, Intent data) {
		super.onActivityResult(requestCode, resultCode, data);

		if (callbackManager != null) {
			callbackManager.onActivityResult(requestCode, resultCode, data);
		}
	}
}
