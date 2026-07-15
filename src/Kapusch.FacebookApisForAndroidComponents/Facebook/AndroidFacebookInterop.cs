using Android.Content;

namespace Kapusch.Facebook.Android;

public static class AndroidFacebookInterop
{
	public static string LoginActivityClassName => "com.kapusch.facebook.androidinterop.FacebookLoginActivity";
	public static string ShareActivityClassName => "com.kapusch.facebook.androidinterop.FacebookShareActivity";

	public static string ExtraStatus => "kfb_status";
	public static string ExtraAccessToken => "kfb_access_token";
	public static string ExtraUserId => "kfb_user_id";
	public static string ExtraErrorCode => "kfb_error_code";
	public static string ExtraErrorMessage => "kfb_error_message";
	public static string ExtraShareImagePath => "kfb_share_image_path";

	public static string LogoutAction => "com.kapusch.facebook.androidinterop.LOGOUT";

	public static void SendSignOutBroadcast(Context context)
	{
		var intent = new Intent(LogoutAction);
		context.SendBroadcast(intent);
	}
}
