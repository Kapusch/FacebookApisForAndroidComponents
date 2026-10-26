package com.kapusch.facebook.androidinterop;

import android.app.Application;
import android.content.Context;
import android.os.Bundle;

import com.facebook.FacebookSdk;
import com.facebook.LoggingBehavior;
import com.facebook.appevents.AppEventsLogger;

import java.math.BigDecimal;
import java.util.Currency;

/**
 * Thin static facade over the Facebook App Events API, called from managed code through JNI.
 *
 * <p>The upstream SDK owns event persistence, batching and retries. This class adds no queue and
 * no defaults: every privacy-relevant switch is left to the upstream manifest keys unless the
 * caller sets it explicitly.
 */
public final class FacebookAppEventsInterop {
	private FacebookAppEventsInterop() {}

	/**
	 * Initializes the SDK if the FacebookInitProvider did not (for example when the consumer removed
	 * it from the merged manifest), applies the data processing options, then marks the SDK fully
	 * initialized for this process.
	 *
	 * <p>The options are applied between the two steps because FacebookSdk.setDataProcessingOptions
	 * needs the application context that sdkInitialize stores, and fullyInitialize starts the SDK's
	 * first Graph requests, which must already carry Limited Data Use.
	 *
	 * @param dataProcessingOptions null leaves the options unchanged; an empty array clears them
	 */
	@SuppressWarnings("deprecation")
	public static void initializeSdk(Context context, String[] dataProcessingOptions, int country, int state) {
		Context applicationContext = requireContext(context).getApplicationContext();
		if (!FacebookSdk.isInitialized()) {
			FacebookSdk.sdkInitialize(applicationContext);
		}
		if (dataProcessingOptions != null) {
			FacebookSdk.setDataProcessingOptions(dataProcessingOptions, country, state);
		}
		FacebookSdk.fullyInitialize();
	}

	public static boolean isSdkInitialized() {
		return FacebookSdk.isInitialized();
	}

	public static boolean isSdkFullyInitialized() {
		return FacebookSdk.isFullyInitialized();
	}

	public static void setAutoInitEnabled(boolean enabled) {
		requireInitialized("setAutoInitEnabled");
		FacebookSdk.setAutoInitEnabled(enabled);
	}

	public static boolean isAutoInitEnabled() {
		return FacebookSdk.getAutoInitEnabled();
	}

	public static void setAutoLogAppEventsEnabled(boolean enabled) {
		requireInitialized("setAutoLogAppEventsEnabled");
		FacebookSdk.setAutoLogAppEventsEnabled(enabled);
	}

	public static boolean isAutoLogAppEventsEnabled() {
		return FacebookSdk.getAutoLogAppEventsEnabled();
	}

	public static void setAdvertiserIdCollectionEnabled(boolean enabled) {
		requireInitialized("setAdvertiserIdCollectionEnabled");
		FacebookSdk.setAdvertiserIDCollectionEnabled(enabled);
	}

	public static boolean isAdvertiserIdCollectionEnabled() {
		return FacebookSdk.getAdvertiserIDCollectionEnabled();
	}

	public static void setAutoLogMetaDataEnabled(boolean enabled) {
		requireInitialized("setAutoLogMetaDataEnabled");
		FacebookSdk.setAutoLogMetaDataEnabled(enabled);
	}

	public static boolean isAutoLogMetaDataEnabled() {
		return FacebookSdk.getAutoLogMetaDataEnabled();
	}

	public static void setAutoLogProductImprovementEnabled(boolean enabled) {
		requireInitialized("setAutoLogProductImprovementEnabled");
		FacebookSdk.setAutoLogProductImprovementEnabled(enabled);
	}

	public static boolean isAutoLogProductImprovementEnabled() {
		return FacebookSdk.getAutoLogProductImprovementEnabled();
	}

	public static void setDataProcessingOptions(String[] options, int country, int state) {
		requireInitialized("setDataProcessingOptions");
		FacebookSdk.setDataProcessingOptions(options != null ? options : new String[0], country, state);
	}

	public static void setDebugLoggingEnabled(boolean enabled) {
		FacebookSdk.setIsDebugEnabled(enabled);
		if (enabled) {
			FacebookSdk.addLoggingBehavior(LoggingBehavior.APP_EVENTS);
		} else {
			FacebookSdk.removeLoggingBehavior(LoggingBehavior.APP_EVENTS);
		}
	}

	/** @param flushBehavior 0 = AUTO, 1 = EXPLICIT_ONLY (mirrors the managed enum). */
	public static void setFlushBehavior(int flushBehavior) {
		AppEventsLogger.setFlushBehavior(
			flushBehavior == 1
				? AppEventsLogger.FlushBehavior.EXPLICIT_ONLY
				: AppEventsLogger.FlushBehavior.AUTO
		);
	}

	/** @return 0 = AUTO, 1 = EXPLICIT_ONLY (mirrors the managed enum). */
	public static int getFlushBehavior() {
		return AppEventsLogger.getFlushBehavior() == AppEventsLogger.FlushBehavior.EXPLICIT_ONLY ? 1 : 0;
	}

	public static void activateApp(Context context) {
		requireInitialized("activateApp");
		Context applicationContext = requireContext(context).getApplicationContext();
		if (!(applicationContext instanceof Application)) {
			throw new IllegalStateException("activateApp requires an Application context.");
		}
		AppEventsLogger.activateApp((Application) applicationContext);
	}

	public static void logEvent(String eventName, Bundle parameters) {
		getLogger().logEvent(requireName(eventName), parameters);
	}

	public static void logEventWithValue(String eventName, double valueToSum, Bundle parameters) {
		getLogger().logEvent(requireName(eventName), valueToSum, parameters);
	}

	/**
	 * @param amount purchase amount as an invariant decimal string, so no precision is lost
	 *     crossing JNI
	 * @param currencyCode ISO 4217 code
	 */
	public static void logPurchase(String amount, String currencyCode, Bundle parameters) {
		if (amount == null || amount.trim().isEmpty()) {
			throw new IllegalArgumentException("amount is required.");
		}
		if (currencyCode == null || currencyCode.trim().isEmpty()) {
			throw new IllegalArgumentException("currencyCode is required.");
		}
		getLogger().logPurchase(new BigDecimal(amount), Currency.getInstance(currencyCode), parameters);
	}

	public static void flush() {
		getLogger().flush();
	}

	public static String getAnonymousId(Context context) {
		return AppEventsLogger.getAnonymousAppDeviceGUID(requireContext(context).getApplicationContext());
	}

	public static void setUserId(String userId) {
		requireInitialized("setUserId");
		AppEventsLogger.setUserID(userId);
	}

	public static String getUserId() {
		requireInitialized("getUserId");
		return AppEventsLogger.getUserID();
	}

	public static void clearUserId() {
		requireInitialized("clearUserId");
		AppEventsLogger.clearUserID();
	}

	/**
	 * Replaces the stored user data: every value is hashed by the SDK before it is persisted or
	 * sent, and a null value clears its field. AppEventsLogger.setUserData alone merges, keeping the
	 * stored value of null fields, so clear first. Both calls run in order on the SDK's
	 * single-threaded App Events executor.
	 */
	public static void setUserData(
		String email,
		String firstName,
		String lastName,
		String phone,
		String dateOfBirth,
		String gender,
		String city,
		String state,
		String zip,
		String country,
		String externalId
	) {
		requireInitialized("setUserData");
		AppEventsLogger.clearUserData();
		AppEventsLogger.setUserData(
			email,
			firstName,
			lastName,
			phone,
			dateOfBirth,
			gender,
			city,
			state,
			zip,
			country,
			externalId
		);
	}

	public static void clearUserData() {
		requireInitialized("clearUserData");
		AppEventsLogger.clearUserData();
	}

	/**
	 * Returns a new logger for every call. An AppEventsLogger captures the current Facebook access
	 * token when it is created and tags every event it logs with it, so a cached logger would keep
	 * attributing events to a user who has since logged out (or never pick up a later login).
	 * Creating one is cheap: the event queue and the flush state are static in the SDK.
	 */
	private static AppEventsLogger getLogger() {
		requireInitialized("App Events logging");
		return AppEventsLogger.newLogger(FacebookSdk.getApplicationContext());
	}

	/**
	 * Before sdkInitialize, most of these SDK calls fail inside CrashShield, which swallows the
	 * error, or on an SDK background thread, and settings are kept in memory only, where the
	 * persisted value can replace them at initialization. Fail loudly instead so a consent setting
	 * is never silently dropped.
	 */
	private static void requireInitialized(String operation) {
		if (!FacebookSdk.isInitialized()) {
			throw new IllegalStateException(
				operation
					+ " requires an initialized Facebook SDK. Keep FacebookInitProvider in the merged"
					+ " manifest or call FacebookAppEvents.InitializeSdk first."
			);
		}
	}

	private static Context requireContext(Context context) {
		if (context == null) {
			throw new IllegalArgumentException("context is required.");
		}
		return context;
	}

	private static String requireName(String eventName) {
		if (eventName == null || eventName.trim().isEmpty()) {
			throw new IllegalArgumentException("eventName is required.");
		}
		return eventName;
	}
}
