using Sandbox.Audio;
using System;

namespace Duccsoft;

/// <summary>
/// Stores and applies the audio settings of a VideoPlayer, with the option
/// to have the audio position track a GameObject.
/// </summary>
public class TrackingAudioAccessor : IAudioAccessor, IMediaUpdateListener
{
	/// <summary>
	/// Creates a new TrackingAudioAccessort that does not yet target a VideoPlayer.
	/// </summary>
	public TrackingAudioAccessor() 
	{
		IMediaUpdateListener.Register( this );
	}
	
	/// <summary>
	/// Creates a new TrackingAudioAccessor targeting the specified VideoPlayer.
	/// </summary>
	public TrackingAudioAccessor( VideoPlayer videoPlayer )
	{
		VideoPlayer = videoPlayer;
		IMediaUpdateListener.Register( this );
	}

	/// <summary>
	/// The instance of VideoPlayer that shall have its audio settings changed by this object.
	/// </summary>
	public VideoPlayer VideoPlayer 
	{
		get => _videoPlayer;
		set
		{
			_videoPlayer = value;
			MediaUpdate();
		}
	}
	private VideoPlayer _videoPlayer;

	/// <summary>
	/// The GameObject from which to play the sound. The sound will automatically
	/// follow the GameObject.
	/// </summary>
	public GameObject Target 
	{
		get => _target;
		set
		{
			_target = value;
			_listenLocal = !_target.IsValid();
			if ( !_listenLocal )
			{
				_position = _target.WorldPosition;
			}
			MediaUpdate();
		}
	}
	private GameObject _target;

	/// <summary>
	/// The position from which to play the sound. Setting this will also
	/// set <see cref="Target"/> to null.
	/// </summary>
	public Vector3 Position 
	{
		get => _position;
		set
		{
			_position = value;
			_target = null;
			MediaUpdate();
		}
	}
	private Vector3 _position;

	/// <inheritdoc cref="IAudioAccessor.Volume"/>
	public float Volume 
	{ 
		get => _volume;
		set
		{
			_volume = value;
			MediaUpdate();
		}
	}
	private float _volume = 1f;

	/// <inheritdoc cref="IAudioAccessor.ListenLocal"/>
	public bool ListenLocal 
	{
		get => _listenLocal;
		set
		{
			_listenLocal = value;
			if ( _listenLocal )
			{
				_target = null;
			}
			MediaUpdate();
		}
	}
	private bool _listenLocal;

	/// <inheritdoc cref="IAudioAccessor.TargetMixer"/>
	public Mixer TargetMixer
	{
		get => _targetMixer;
		set
		{
			_targetMixer = value;
			MediaUpdate();
		}
	}
	private Mixer _targetMixer;

	/// <inheritdoc cref="IAudioAccessor.Muted"/>
	public bool Muted
	{
		get => _muted;
		set 
		{
			_muted = value;
			MediaUpdate();
		}
	}
	private bool _muted;

	/// <summary>
	/// Updates properties of the underlying audio player to match the values specified in this object.
	/// This will automatically be called on every frame.
	/// </summary>
	public void MediaUpdate()
	{
		if ( VideoPlayer is null )
			return;

		if ( Target.IsValid() )
		{
			_position = Target.WorldPosition;
		}

		VideoPlayer.Muted = Muted;
		VideoPlayer.Audio.Position = Position;
		VideoPlayer.Audio.Volume = Volume;
		VideoPlayer.Audio.ListenLocal = ListenLocal;
		VideoPlayer.Audio.TargetMixer = TargetMixer;
	}

	/// <summary>
	/// Unregisters this <see cref="IMediaUpdateListener"/> from <see cref="MediaUpdateSystem"/>.
	/// </summary>
	public void Dispose()
	{
		IMediaUpdateListener.Unregister( this );
		GC.SuppressFinalize( this );
	}
}
