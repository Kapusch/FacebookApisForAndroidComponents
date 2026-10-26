using System.Collections.Concurrent;
using System.Globalization;
using Android.Content;
using Android.OS;
using Android.Runtime;

namespace Kapusch.Facebook.Android;

/// <summary>
/// Thin JNI bridge to the Facebook Android SDK App Events API (<c>FacebookSdk</c> and
/// <c>AppEventsLogger</c>). The upstream SDK owns persistence, batching and retries; this class
/// adds no queue and changes no default.
/// </summary>
/// <remarks>
/// <para>
/// Every setter maps one to one to the upstream SDK. Runtime values written by the
/// <c>Set*Enabled</c> methods are persisted by the SDK and take precedence over the manifest
/// meta-data on later launches. Apply the consumer's consent state explicitly on every launch.
/// </para>
/// <para>
/// Every setter, the identity methods and the event methods throw
/// <see cref="Java.Lang.IllegalStateException"/> until <c>FacebookSdk.sdkInitialize</c> ran (the
/// default <c>FacebookInitProvider</c> runs it at process start). Before that the SDK would drop
/// them silently. When the provider is removed, call <see cref="InitializeSdk"/> first and pass the
/// data processing options to it. Invalid arguments rejected by the Java layer surface as
/// <see cref="Java.Lang.Throwable"/> subclasses.
/// </para>
/// </remarks>
public static class FacebookAppEvents
{
	private const string InteropClassName = "com/kapusch/facebook/androidinterop/FacebookAppEventsInterop";

	private static readonly Lazy<IntPtr> InteropClass = new(
		() => JNIEnv.FindClass(InteropClassName),
		LazyThreadSafetyMode.ExecutionAndPublication
	);
	private static readonly ConcurrentDictionary<string, IntPtr> MethodIds = new(StringComparer.Ordinal);

	/// <summary>Gets whether <c>FacebookSdk.sdkInitialize</c> already ran in this process.</summary>
	public static bool IsSdkInitialized => CallBool("isSdkInitialized");

	/// <summary>
	/// Gets whether the SDK is fully initialized (auto-init enabled, or <see cref="InitializeSdk"/>
	/// called). Until then the SDK refuses to process Graph API responses.
	/// </summary>
	public static bool IsSdkFullyInitialized => CallBool("isSdkFullyInitialized");

	/// <summary>Gets the SDK auto-init flag (<c>com.facebook.sdk.AutoInitEnabled</c>).</summary>
	public static bool IsAutoInitEnabled => CallBool("isAutoInitEnabled");

	/// <summary>Gets the automatic App Events logging flag (<c>com.facebook.sdk.AutoLogAppEventsEnabled</c>).</summary>
	public static bool IsAutoLogAppEventsEnabled => CallBool("isAutoLogAppEventsEnabled");

	/// <summary>Gets the advertiser ID collection flag (<c>com.facebook.sdk.AdvertiserIDCollectionEnabled</c>).</summary>
	public static bool IsAdvertiserIdCollectionEnabled => CallBool("isAdvertiserIdCollectionEnabled");

	/// <summary>Gets the app metadata collection flag (<c>com.facebook.sdk.AutoLogMetaDataEnabled</c>, SDK 19+).</summary>
	public static bool IsAutoLogMetaDataEnabled => CallBool("isAutoLogMetaDataEnabled");

	/// <summary>
	/// Gets the product-improvement data flag (<c>com.facebook.sdk.AutoLogProductImprovementEnabled</c>, SDK 19+).
	/// </summary>
	public static bool IsAutoLogProductImprovementEnabled => CallBool("isAutoLogProductImprovementEnabled");

	/// <summary>Gets the current App Events flush behavior.</summary>
	public static FacebookFlushBehavior FlushBehavior =>
		(FacebookFlushBehavior)
			JNIEnv.CallStaticIntMethod(InteropClass.Value, Method("getFlushBehavior", "()I"));

	/// <summary>
	/// Initializes the SDK when the <c>FacebookInitProvider</c> did not (for example when the consumer
	/// removed the provider to defer the SDK until consent), applies
	/// <paramref name="dataProcessingOptions"/>, then marks the SDK fully initialized for this
	/// process. Unlike <see cref="SetAutoInitEnabled"/>, nothing is persisted.
	/// </summary>
	/// <remarks>
	/// Pass Limited Data Use here rather than through <see cref="SetDataProcessingOptions"/> when the
	/// provider is removed: the setter needs an initialized SDK, and full initialization starts the
	/// SDK's first Graph requests, so this is the only point where the options apply to them.
	/// </remarks>
	/// <param name="context">Any context; the application context is used. Defaults to the application context.</param>
	/// <param name="dataProcessingOptions">
	/// Options such as <see cref="FacebookDataProcessingOptions.LimitedDataUse"/>.
	/// <see langword="null"/> leaves the current options unchanged; an empty array clears them.
	/// </param>
	/// <param name="dataProcessingCountry">Country for the options; <c>0</c> lets Meta geolocate the user.</param>
	/// <param name="dataProcessingState">State for the options; <c>0</c> lets Meta geolocate the user.</param>
	public static void InitializeSdk(
		Context? context = null,
		string[]? dataProcessingOptions = null,
		int dataProcessingCountry = 0,
		int dataProcessingState = 0
	)
	{
		var array = dataProcessingOptions is null ? IntPtr.Zero : JNIEnv.NewArray(dataProcessingOptions);
		try
		{
			CallVoid(
				"initializeSdk",
				"(Landroid/content/Context;[Ljava/lang/String;II)V",
				new JValue(ResolveContext(context)),
				new JValue(array),
				new JValue(dataProcessingCountry),
				new JValue(dataProcessingState)
			);
		}
		finally
		{
			if (array != IntPtr.Zero)
			{
				JNIEnv.DeleteLocalRef(array);
			}
		}
	}

	/// <summary>
	/// Sets the persisted auto-init flag. <see langword="true"/> also fully initializes the SDK
	/// (<c>FacebookSdk.setAutoInitEnabled</c>).
	/// </summary>
	public static void SetAutoInitEnabled(bool enabled) => CallVoid("setAutoInitEnabled", "(Z)V", new JValue(enabled));

	/// <summary>
	/// Sets the persisted automatic App Events logging flag. <see langword="true"/> starts activity
	/// tracking, which requires an initialized SDK. Automatic logging covers the install ping, app
	/// activation and, when enabled in Events Manager, in-app purchases and subscriptions.
	/// </summary>
	public static void SetAutoLogAppEventsEnabled(bool enabled) =>
		CallVoid("setAutoLogAppEventsEnabled", "(Z)V", new JValue(enabled));

	/// <summary>Sets the persisted advertiser ID (GAID) collection flag.</summary>
	public static void SetAdvertiserIdCollectionEnabled(bool enabled) =>
		CallVoid("setAdvertiserIdCollectionEnabled", "(Z)V", new JValue(enabled));

	/// <summary>
	/// Sets the persisted app metadata collection flag (screen title and AppLink URL attached to
	/// events, SDK 19+). <see langword="false"/> also discards metadata already collected.
	/// </summary>
	public static void SetAutoLogMetaDataEnabled(bool enabled) =>
		CallVoid("setAutoLogMetaDataEnabled", "(Z)V", new JValue(enabled));

	/// <summary>Sets the persisted product-improvement data flag (SDK 19+).</summary>
	public static void SetAutoLogProductImprovementEnabled(bool enabled) =>
		CallVoid("setAutoLogProductImprovementEnabled", "(Z)V", new JValue(enabled));

	/// <summary>
	/// Sets data processing options, for example <see cref="FacebookDataProcessingOptions.LimitedDataUse"/>.
	/// An empty array explicitly opts out. Country and state <c>0</c> let Meta geolocate the user.
	/// Requires an initialized SDK; with the <c>FacebookInitProvider</c> removed, pass the options to
	/// <see cref="InitializeSdk"/> instead.
	/// </summary>
	public static void SetDataProcessingOptions(string[] options, int country = 0, int state = 0)
	{
		ArgumentNullException.ThrowIfNull(options);
		var array = JNIEnv.NewArray(options);
		try
		{
			CallVoid(
				"setDataProcessingOptions",
				"([Ljava/lang/String;II)V",
				new JValue(array),
				new JValue(country),
				new JValue(state)
			);
		}
		finally
		{
			JNIEnv.DeleteLocalRef(array);
		}
	}

	/// <summary>
	/// Enables or disables SDK debug mode and the <c>APP_EVENTS</c> logging behavior, so logged and
	/// flushed events appear in logcat. Intended for diagnostics only.
	/// </summary>
	public static void SetDebugLoggingEnabled(bool enabled) =>
		CallVoid("setDebugLoggingEnabled", "(Z)V", new JValue(enabled));

	/// <summary>Sets when queued events are flushed to Meta.</summary>
	public static void SetFlushBehavior(FacebookFlushBehavior behavior)
	{
		if (!Enum.IsDefined(behavior))
		{
			throw new ArgumentOutOfRangeException(nameof(behavior), behavior, "Unknown flush behavior.");
		}

		CallVoid("setFlushBehavior", "(I)V", new JValue((int)behavior));
	}

	/// <summary>
	/// Logs app activation and the one-time install ping, then starts automatic activate/deactivate
	/// tracking (<c>AppEventsLogger.activateApp</c>). Call it when automatic logging is disabled and
	/// the consumer still wants install and launch measurement. Call it once the SDK is fully
	/// initialized (<see cref="SetAutoInitEnabled"/> or <see cref="InitializeSdk"/>); before that the
	/// SDK discards the install ping response and only retries on a later activation.
	/// </summary>
	public static void ActivateApp(Context? context = null) =>
		CallVoid("activateApp", "(Landroid/content/Context;)V", new JValue(ResolveContext(context)));

	/// <summary>Logs an event (<c>AppEventsLogger.logEvent</c>).</summary>
	/// <param name="name">Event name, for example a <see cref="FacebookAppEventNames"/> constant.</param>
	/// <param name="parameters">
	/// Optional parameters. Values must be <see cref="string"/>, <see cref="bool"/> (sent as
	/// <c>"1"</c>/<c>"0"</c>), an integral type, <see cref="float"/>, <see cref="double"/> or
	/// <see cref="decimal"/>.
	/// </param>
	/// <param name="valueToSum">Optional value Meta sums for this event (for example a price).</param>
	public static void LogEvent(
		string name,
		IReadOnlyDictionary<string, object>? parameters = null,
		double? valueToSum = null
	)
	{
		ArgumentException.ThrowIfNullOrWhiteSpace(name);
		if (valueToSum is { } value && !double.IsFinite(value))
		{
			throw new ArgumentOutOfRangeException(nameof(valueToSum), value, "valueToSum must be finite.");
		}

		using var bundle = ToBundle(parameters);
		var eventName = JNIEnv.NewString(name);
		try
		{
			if (valueToSum is { } sum)
			{
				CallVoid(
					"logEventWithValue",
					"(Ljava/lang/String;DLandroid/os/Bundle;)V",
					new JValue(eventName),
					new JValue(sum),
					new JValue(bundle?.Handle ?? IntPtr.Zero)
				);
			}
			else
			{
				CallVoid(
					"logEvent",
					"(Ljava/lang/String;Landroid/os/Bundle;)V",
					new JValue(eventName),
					new JValue(bundle?.Handle ?? IntPtr.Zero)
				);
			}
		}
		finally
		{
			JNIEnv.DeleteLocalRef(eventName);
		}
	}

	/// <summary>
	/// Logs <see cref="FacebookAppEventNames.Purchased"/> with <c>fb_currency</c> and the amount as
	/// value to sum (<c>AppEventsLogger.logPurchase</c>). The SDK flushes purchases immediately
	/// unless the flush behavior is <see cref="FacebookFlushBehavior.ExplicitOnly"/>.
	/// </summary>
	/// <param name="amount">Purchase amount.</param>
	/// <param name="currencyIsoCode">ISO 4217 currency code, for example <c>EUR</c>.</param>
	/// <param name="parameters">Optional extra parameters.</param>
	public static void LogPurchase(
		decimal amount,
		string currencyIsoCode,
		IReadOnlyDictionary<string, object>? parameters = null
	)
	{
		var currency = RequireCurrency(currencyIsoCode, nameof(currencyIsoCode));
		using var bundle = ToBundle(parameters);
		var amountString = JNIEnv.NewString(amount.ToString(CultureInfo.InvariantCulture));
		var currencyString = JNIEnv.NewString(currency);
		try
		{
			CallVoid(
				"logPurchase",
				"(Ljava/lang/String;Ljava/lang/String;Landroid/os/Bundle;)V",
				new JValue(amountString),
				new JValue(currencyString),
				new JValue(bundle?.Handle ?? IntPtr.Zero)
			);
		}
		finally
		{
			JNIEnv.DeleteLocalRef(amountString);
			JNIEnv.DeleteLocalRef(currencyString);
		}
	}

	/// <summary>
	/// Logs <see cref="FacebookAppEventNames.StartTrial"/> with Meta's documented parameters.
	/// </summary>
	/// <param name="value">Optional value to sum, typically the price after the trial. Requires <paramref name="currencyIsoCode"/>.</param>
	/// <param name="currencyIsoCode">ISO 4217 currency code sent as <c>fb_currency</c>.</param>
	/// <param name="orderId">Optional <c>fb_order_id</c>: one ID shared by all events of a subscription.</param>
	/// <param name="parameters">Optional extra parameters. Explicit arguments win over the same keys here.</param>
	public static void LogStartTrial(
		decimal? value = null,
		string? currencyIsoCode = null,
		string? orderId = null,
		IReadOnlyDictionary<string, object>? parameters = null
	) => LogValuedEvent(FacebookAppEventNames.StartTrial, value, currencyIsoCode, orderId, parameters);

	/// <summary>
	/// Logs <see cref="FacebookAppEventNames.Subscribe"/> with Meta's documented parameters.
	/// </summary>
	/// <param name="value">Optional value to sum, typically the subscription price. Requires <paramref name="currencyIsoCode"/>.</param>
	/// <param name="currencyIsoCode">ISO 4217 currency code sent as <c>fb_currency</c>.</param>
	/// <param name="orderId">Optional <c>fb_order_id</c>: one ID shared by all events of a subscription.</param>
	/// <param name="parameters">Optional extra parameters. Explicit arguments win over the same keys here.</param>
	public static void LogSubscribe(
		decimal? value = null,
		string? currencyIsoCode = null,
		string? orderId = null,
		IReadOnlyDictionary<string, object>? parameters = null
	) => LogValuedEvent(FacebookAppEventNames.Subscribe, value, currencyIsoCode, orderId, parameters);

	/// <summary>Logs <see cref="FacebookAppEventNames.CompletedRegistration"/>.</summary>
	/// <param name="registrationMethod">Optional <c>fb_registration_method</c>, for example <c>email</c>.</param>
	/// <param name="parameters">Optional extra parameters. Explicit arguments win over the same keys here.</param>
	public static void LogCompleteRegistration(
		string? registrationMethod = null,
		IReadOnlyDictionary<string, object>? parameters = null
	)
	{
		var merged = Merge(parameters);
		if (!string.IsNullOrWhiteSpace(registrationMethod))
		{
			merged[FacebookAppEventParameterNames.RegistrationMethod] = registrationMethod;
		}

		LogEvent(FacebookAppEventNames.CompletedRegistration, merged);
	}

	/// <summary>Sends queued events now (<c>AppEventsLogger.flush</c>). Not a delivery receipt.</summary>
	public static void Flush() => CallVoid("flush", "()V");

	/// <summary>
	/// Gets the SDK's persisted anonymous app/device ID (<c>AppEventsLogger.getAnonymousAppDeviceGUID</c>),
	/// for example to forward to a server-side integration.
	/// </summary>
	public static string GetAnonymousId(Context? context = null) =>
		CallString("getAnonymousId", "(Landroid/content/Context;)Ljava/lang/String;", new JValue(ResolveContext(context)))
		?? string.Empty;

	/// <summary>Gets the user ID attached to events, or <see langword="null"/>.</summary>
	public static string? GetUserId() => CallString("getUserId", "()Ljava/lang/String;");

	/// <summary>
	/// Sets the persisted user ID attached to subsequent events (<c>AppEventsLogger.setUserID</c>).
	/// <see langword="null"/> clears it.
	/// </summary>
	public static void SetUserId(string? userId)
	{
		if (userId is null)
		{
			ClearUserId();
			return;
		}

		var value = JNIEnv.NewString(userId);
		try
		{
			CallVoid("setUserId", "(Ljava/lang/String;)V", new JValue(value));
		}
		finally
		{
			JNIEnv.DeleteLocalRef(value);
		}
	}

	/// <summary>Clears the persisted user ID (<c>AppEventsLogger.clearUserID</c>).</summary>
	public static void ClearUserId() => CallVoid("clearUserId", "()V");

	/// <summary>
	/// Sets advanced-matching user data (<c>AppEventsLogger.setUserData</c>). The SDK normalizes and
	/// SHA-256 hashes every value (values that already look like SHA-256 hashes are kept as is) before
	/// persisting or sending it. Fields left <see langword="null"/> keep their previously stored value;
	/// call <see cref="ClearUserData"/> to remove everything.
	/// </summary>
	public static void SetUserData(FacebookUserData userData)
	{
		ArgumentNullException.ThrowIfNull(userData);
		string?[] values =
		[
			userData.Email,
			userData.FirstName,
			userData.LastName,
			userData.Phone,
			userData.DateOfBirth,
			userData.Gender,
			userData.City,
			userData.State,
			userData.Zip,
			userData.Country,
			userData.ExternalId,
		];
		var handles = new IntPtr[values.Length];
		try
		{
			var args = new JValue[values.Length];
			for (var i = 0; i < values.Length; i++)
			{
				handles[i] = values[i] is null ? IntPtr.Zero : JNIEnv.NewString(values[i]);
				args[i] = new JValue(handles[i]);
			}

			CallVoid("setUserData", "(Ljava/lang/String;Ljava/lang/String;Ljava/lang/String;Ljava/lang/String;Ljava/lang/String;Ljava/lang/String;Ljava/lang/String;Ljava/lang/String;Ljava/lang/String;Ljava/lang/String;Ljava/lang/String;)V", args);
		}
		finally
		{
			foreach (var handle in handles)
			{
				if (handle != IntPtr.Zero)
				{
					JNIEnv.DeleteLocalRef(handle);
				}
			}
		}
	}

	/// <summary>Clears advanced-matching user data (<c>AppEventsLogger.clearUserData</c>).</summary>
	public static void ClearUserData() => CallVoid("clearUserData", "()V");

	private static void LogValuedEvent(
		string name,
		decimal? value,
		string? currencyIsoCode,
		string? orderId,
		IReadOnlyDictionary<string, object>? parameters
	)
	{
		var merged = Merge(parameters);
		if (!string.IsNullOrWhiteSpace(currencyIsoCode))
		{
			merged[FacebookAppEventParameterNames.Currency] = RequireCurrency(
				currencyIsoCode,
				nameof(currencyIsoCode)
			);
		}
		else if (value is not null)
		{
			throw new ArgumentException("A currency is required when a value is provided.", nameof(currencyIsoCode));
		}

		if (!string.IsNullOrWhiteSpace(orderId))
		{
			merged[FacebookAppEventParameterNames.OrderId] = orderId;
		}

		LogEvent(name, merged, value is { } amount ? (double)amount : null);
	}

	private static Dictionary<string, object> Merge(IReadOnlyDictionary<string, object>? parameters)
	{
		var merged = new Dictionary<string, object>(StringComparer.Ordinal);
		if (parameters is not null)
		{
			foreach (var (key, value) in parameters)
			{
				merged[key] = value;
			}
		}

		return merged;
	}

	private static string RequireCurrency(string? currencyIsoCode, string parameterName)
	{
		ArgumentException.ThrowIfNullOrWhiteSpace(currencyIsoCode, parameterName);
		var code = currencyIsoCode.Trim().ToUpperInvariant();
		if (code.Length != 3 || !code.All(char.IsAsciiLetterUpper))
		{
			throw new ArgumentException("Expected a three-letter ISO 4217 currency code.", parameterName);
		}

		return code;
	}

	private static Bundle? ToBundle(IReadOnlyDictionary<string, object>? parameters)
	{
		if (parameters is null || parameters.Count == 0)
		{
			return null;
		}

		var bundle = new Bundle();
		try
		{
			foreach (var (key, value) in parameters)
			{
				if (string.IsNullOrWhiteSpace(key))
				{
					throw new ArgumentException("Parameter names must not be empty.", nameof(parameters));
				}

				switch (value)
				{
					case string text:
						bundle.PutString(key, text);
						break;
					case bool flag:
						bundle.PutString(
							key,
							flag ? FacebookAppEventParameterValues.Yes : FacebookAppEventParameterValues.No
						);
						break;
					case byte or sbyte or short or ushort or int:
						bundle.PutInt(key, Convert.ToInt32(value, CultureInfo.InvariantCulture));
						break;
					case uint or long:
						bundle.PutLong(key, Convert.ToInt64(value, CultureInfo.InvariantCulture));
						break;
					case ulong unsignedLong:
						bundle.PutString(key, unsignedLong.ToString(CultureInfo.InvariantCulture));
						break;
					case float single when float.IsFinite(single):
						bundle.PutFloat(key, single);
						break;
					case double number when double.IsFinite(number):
						bundle.PutDouble(key, number);
						break;
					case decimal amount:
						// The SDK stringifies every value; keep decimals exact.
						bundle.PutString(key, amount.ToString(CultureInfo.InvariantCulture));
						break;
					default:
						throw new ArgumentException(
							$"Unsupported value for parameter '{key}'. Use a string, bool, integral, finite floating-point or decimal value.",
							nameof(parameters)
						);
				}
			}

			return bundle;
		}
		catch
		{
			bundle.Dispose();
			throw;
		}
	}

	private static Context ResolveContext(Context? context) =>
		context
		?? global::Android.App.Application.Context
		?? throw new InvalidOperationException("No Android application context is available.");

	private static IntPtr Method(string name, string signature) =>
		MethodIds.GetOrAdd(
			name + signature,
			static (_, state) => JNIEnv.GetStaticMethodID(InteropClass.Value, state.name, state.signature),
			(name, signature)
		);

	private static void CallVoid(string name, string signature, params JValue[] args) =>
		JNIEnv.CallStaticVoidMethod(InteropClass.Value, Method(name, signature), args);

	private static bool CallBool(string name) =>
		JNIEnv.CallStaticBooleanMethod(InteropClass.Value, Method(name, "()Z"));

	private static string? CallString(string name, string signature, params JValue[] args) =>
		JNIEnv.GetString(
			JNIEnv.CallStaticObjectMethod(InteropClass.Value, Method(name, signature), args),
			JniHandleOwnership.TransferLocalRef
		);
}
