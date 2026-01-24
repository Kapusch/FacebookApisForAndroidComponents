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

	protected override void OnCreate(Bundle? savedInstanceState)
	{
		base.OnCreate(savedInstanceState);

		var button = new Button(this) { Text = "Start Facebook Login" };
		button.Click += (_, _) =>
		{
			var intent = new Intent();
			intent.SetClassName(PackageName, AndroidFacebookInterop.LoginActivityClassName);
			StartActivityForResult(intent, LoginRequestCode);
		};

		SetContentView(button);
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
