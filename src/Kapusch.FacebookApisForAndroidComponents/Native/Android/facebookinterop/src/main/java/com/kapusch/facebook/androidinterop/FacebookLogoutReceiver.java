package com.kapusch.facebook.androidinterop;

import android.content.BroadcastReceiver;
import android.content.Context;
import android.content.Intent;

import com.facebook.login.LoginManager;

public final class FacebookLogoutReceiver extends BroadcastReceiver {
	@Override
	public void onReceive(Context context, Intent intent) {
		try {
			LoginManager.getInstance().logOut();
		} catch (Exception ignored) {
			// Best-effort only.
		}
	}
}
