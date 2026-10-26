namespace Kapusch.Facebook.Android;

/// <summary>Mirrors <c>AppEventsLogger.FlushBehavior</c>.</summary>
public enum FacebookAppEventsFlushBehavior
{
	/// <summary>The SDK flushes periodically and when enough events are queued (default).</summary>
	Auto = 0,

	/// <summary>Events are sent only when <see cref="FacebookAppEvents.Flush"/> is called, purchases included.</summary>
	ExplicitOnly = 1,
}
