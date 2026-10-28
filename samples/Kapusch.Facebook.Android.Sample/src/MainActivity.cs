using Android.App;
using Android.Content;
using Android.OS;
using Android.Widget;
using Kapusch.Facebook.Android;

namespace Kapusch.Facebook.Android.Sample;

[Activity(Label = "Kapusch Facebook Android Sample", MainLauncher = true)]
public sealed class MainActivity : Activity
{
	private const int LoginRequestCode = 4242;
	private TextView? _status;

	protected override void OnCreate(Bundle? savedInstanceState)
	{
		base.OnCreate(savedInstanceState);

		var layout = new LinearLayout(this) { Orientation = Orientation.Vertical };
		_status = new TextView(this);
		layout.AddView(_status);

		AddButton(layout, "Start Facebook Login", StartLogin);

		// App Events: the manifest template disables every automatic collection switch, so nothing
		// is sent until the user grants consent here. A real app persists the choice and re-applies
		// it on every launch because the SDK persists runtime values over the manifest defaults.
		AddButton(layout, "Grant ads consent", GrantConsent);
		AddButton(layout, "Withdraw ads consent", WithdrawConsent);
		AddButton(
			layout,
			"Log StartTrial (test values)",
			() => FacebookAppEvents.LogStartTrial(value: 0m, currencyIsoCode: "USD", orderId: "sample-order")
		);
		AddButton(
			layout,
			"Log Subscribe (test values)",
			() => FacebookAppEvents.LogSubscribe(value: 9.99m, currencyIsoCode: "USD", orderId: "sample-order")
		);
		AddButton(
			layout,
			"Log custom event",
			() =>
				FacebookAppEvents.LogEvent(
					FacebookAppEventNames.ViewedContent,
					new Dictionary<string, object>
					{
						[FacebookAppEventParameterNames.ContentType] = "sample",
						[FacebookAppEventParameterNames.ContentId] = "item-1",
					}
				)
		);
		AddButton(layout, "Flush", FacebookAppEvents.Flush);

		var scroll = new ScrollView(this);
		scroll.AddView(layout);
		SetContentView(scroll);

#if DEBUG
		FacebookAppEvents.SetDebugLoggingEnabled(true);
#endif
		RefreshStatus();
	}

	private void AddButton(LinearLayout layout, string text, Action action)
	{
		var button = new Button(this) { Text = text };
		button.Click += (_, _) =>
		{
			try
			{
				action();
			}
			catch (Exception ex)
			{
				Toast.MakeText(this, $"{ex.GetType().Name}: {ex.Message}", ToastLength.Long)?.Show();
			}

			RefreshStatus();
		};
		layout.AddView(button);
	}

	private static void GrantConsent()
	{
		FacebookAppEvents.SetFlushBehavior(FacebookAppEventsFlushBehavior.Auto);
		FacebookAppEvents.SetAutoInitEnabled(true);
		FacebookAppEvents.SetAdvertiserIdCollectionEnabled(true);
		// App opens are automatic events, so they need automatic logging on. Turn automatic purchase
		// logging off in the Meta App Dashboard when a server reports purchases.
		FacebookAppEvents.SetAutoLogAppEventsEnabled(true);
		// ActivateApp sends the one-time install ping; activations are logged from the next activity
		// resume on. This screen is already resumed, so the current session is not counted. A real
		// app calls it on every launch with consent, before the first activity resumes.
		FacebookAppEvents.ActivateApp();
	}

	private static void WithdrawConsent()
	{
		FacebookAppEvents.SetAutoLogAppEventsEnabled(false);
		FacebookAppEvents.SetAdvertiserIdCollectionEnabled(false);
		FacebookAppEvents.SetAutoInitEnabled(false);
		FacebookAppEvents.ClearUserId();
		FacebookAppEvents.ClearUserData();
		// Automatic events stop at once, but the SDK stays fully initialized and keeps flushing queued
		// events until the process restarts. Stop sending for this process; a real app also stops
		// calling the Log* methods.
		FacebookAppEvents.SetFlushBehavior(FacebookAppEventsFlushBehavior.ExplicitOnly);
	}

	private void RefreshStatus()
	{
		if (_status is null)
			return;

		_status.Text =
			$"SDK initialized: {FacebookAppEvents.IsSdkInitialized}, fully: {FacebookAppEvents.IsSdkFullyInitialized}\n"
			+ $"Auto-init: {FacebookAppEvents.IsAutoInitEnabled}, auto-log: {FacebookAppEvents.IsAutoLogAppEventsEnabled}, "
			+ $"advertiser ID: {FacebookAppEvents.IsAdvertiserIdCollectionEnabled}\n"
			+ $"Metadata: {FacebookAppEvents.IsMetadataCollectionEnabled}, product improvement: {FacebookAppEvents.IsAutoLogProductImprovementEnabled}\n"
			+ $"Anonymous ID: {FacebookAppEvents.GetAnonymousId()}";
	}

	private void StartLogin()
	{
		var packageName = PackageName;
		if (string.IsNullOrWhiteSpace(packageName))
		{
			Toast.MakeText(this, "Missing package name", ToastLength.Short)?.Show();
			return;
		}

		var intent = new Intent();
		intent.SetClassName(packageName, AndroidFacebookInterop.LoginActivityClassName);
		StartActivityForResult(intent, LoginRequestCode);
	}

	protected override void OnActivityResult(int requestCode, Result resultCode, Intent? data)
	{
		base.OnActivityResult(requestCode, resultCode, data);

		if (requestCode != LoginRequestCode)
			return;

		var hasData = data is not null;
		var status = hasData
			? (NativeSignInStatus)
				data!.GetIntExtra(
					AndroidFacebookInterop.ExtraStatus,
					(int)NativeSignInStatus.Cancelled
				)
			: NativeSignInStatus.Cancelled;

		if (!hasData)
		{
			Toast.MakeText(this, "No result data", ToastLength.Short)?.Show();
			return;
		}

		switch (status)
		{
			case NativeSignInStatus.Success:
				var token = data!.GetStringExtra(AndroidFacebookInterop.ExtraAccessToken);
				Toast
					.MakeText(this, $"Success (token len: {token?.Length ?? 0})", ToastLength.Short)
					?.Show();
				break;
			case NativeSignInStatus.Cancelled:
				Toast.MakeText(this, "Cancelled", ToastLength.Short)?.Show();
				break;
			case NativeSignInStatus.Failed:
				var error = data!.GetStringExtra(AndroidFacebookInterop.ExtraErrorMessage);
				Toast.MakeText(this, $"Failed: {error}", ToastLength.Short)?.Show();
				break;
		}
	}
}
