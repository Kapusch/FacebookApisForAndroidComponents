namespace Kapusch.Facebook.Android;

/// <summary>
/// Advanced-matching user data for <see cref="FacebookAppEvents.SetUserData"/>. Pass raw values:
/// the Facebook SDK normalizes and SHA-256 hashes each one before persisting or sending it.
/// <see cref="FacebookAppEvents.SetUserData"/> replaces every field at once, so a
/// <see langword="null"/> property clears that field (same as the iOS package).
/// </summary>
/// <remarks>
/// Advanced matching sends personal data (hashed) to Meta. Collect it only with a legal basis and
/// clear it with <see cref="FacebookAppEvents.ClearUserData"/> when that basis ends.
/// </remarks>
public sealed record FacebookAppEventsUserData
{
	/// <summary>Email address.</summary>
	public string? Email { get; init; }

	/// <summary>First name.</summary>
	public string? FirstName { get; init; }

	/// <summary>Last name.</summary>
	public string? LastName { get; init; }

	/// <summary>Phone number, including the country code.</summary>
	public string? Phone { get; init; }

	/// <summary>Date of birth as <c>YYYYMMDD</c>.</summary>
	public string? DateOfBirth { get; init; }

	/// <summary>Gender as <c>m</c> or <c>f</c>.</summary>
	public string? Gender { get; init; }

	/// <summary>City.</summary>
	public string? City { get; init; }

	/// <summary>State or region code.</summary>
	public string? State { get; init; }

	/// <summary>Zip or postal code.</summary>
	public string? Zip { get; init; }

	/// <summary>Two-letter ISO 3166-1 country code.</summary>
	public string? Country { get; init; }

	/// <summary>
	/// An ID representing the user in the consumer's own systems. Android only: the iOS package sets it
	/// with <c>SetUserDataValue(FacebookAppEventUserDataTypes.ExternalId, ...)</c> instead.
	/// </summary>
	public string? ExternalId { get; init; }
}
