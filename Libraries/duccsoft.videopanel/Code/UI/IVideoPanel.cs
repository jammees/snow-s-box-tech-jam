namespace Duccsoft;

/// <summary>
/// Represents an instance of a VideoPlayer that may be controlled with <see cref="VideoControlOverlay"/>.
/// </summary>
public interface IVideoPanel : IValid
{
	/// <summary>
	/// Returns true if the next video is currently being loaded.
	/// </summary>
	bool IsLoading { get; }
	/// <summary>
	/// The duration in seconds of the currently playing video.
	/// </summary>
	float Duration { get; }
	/// <summary>
	/// Returns true if video is currently playing.
	/// </summary>
	bool IsPlaying { get; }
	/// <summary>
	/// Returns the current playback time of the currently playing video.
	/// </summary>
	float PlaybackTime { get; }
	/// <summary>
	/// Returns true if the currently playing video is paused.
	/// </summary>
	bool IsPaused { get; }
	/// <summary>
	/// Moves playback of the currently playing video to a specific time.
	/// </summary>
	/// <param name="time">The desired playback time in seconds.</param>
	void Seek( float time );
	/// <summary>
	/// Toggles between playing/paused states for the currently playing video.
	/// </summary>
	void TogglePause();
	/// <summary>
	/// Provides options for changing audio settings for the currently playing video.
	/// </summary>
	IAudioAccessor Audio { get; }
}
