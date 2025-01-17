using Sandbox.Audio;

namespace Duccsoft;

/// <summary>
/// Provides access to the audio-related functionality of VideoPlayer.
/// </summary>
public interface IAudioAccessor
{
	/// <summary>
	/// The position from which to play the sound.
	/// </summary>
	public Vector3 Position { get; set; }
	/// <summary>
	/// The volume at which the audio should play.
	/// </summary>
	public float Volume { get; set; }
	/// <summary>
	/// If true, audio will play as if it is coming from the UI. Otherwise, the
	/// audio will be spatialized to sound as if it coming from the world at <see cref="Position"/>.
	/// </summary>
	public bool ListenLocal { get; set; }
	/// <summary>
	/// Specifies which audio mixer the sound should be played through.
	/// </summary>
	public Mixer TargetMixer { get; set; }
	/// <summary>
	/// Specifies whether the audio should be muted. The state of <see cref="Muted"/>
	/// may not necessarily depend on the state of <see cref="Volume"/> or vice versa.
	/// </summary>
	public bool Muted { get; set; }
}
