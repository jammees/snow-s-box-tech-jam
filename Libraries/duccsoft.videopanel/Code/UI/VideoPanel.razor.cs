using Sandbox.Audio;
using Sandbox.UI;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Duccsoft;

/// <summary>
/// A Panel that manages an instance of a VideoPlayer, using its texture as the background image.
/// Supports all the playback controls of VideoPlayer.
/// </summary>
[Alias("video")]
public partial class VideoPanel : Panel, IVideoPanel
{
	/// <summary>
	/// The path/url of a video relative to VideoRoot.
	/// </summary>
	public string VideoPath { get; set; }
	/// <summary>
	/// Specifies where the video may be found, whether it comes from a BaseFileSystem or a website.
	/// Set this to <see cref="VideoRoot.WebStream"/> if VideoPath is a URL.
	/// </summary>
	public VideoRoot VideoRoot { get; set; } = VideoRoot.MountedFileSystem;
	/// <summary>
	/// If true, the video will automatically loop whenever HasReachedEnd is true.
	/// </summary>
	public bool ShouldLoop { get; set; } = true;
	public bool AutoPlay { get; set; } = true;
	public bool StartMuted { get; set; } = false;


	public bool ShowControls { get; set; } = false;
	public bool AutoHideControls { get; set; } = true;
	public float AutoHideDelay { get; set; } = 1f;
	
	public float Width
	{
		get
		{
			if ( Style is null || Parent is null )
				return 0f;

			return Style.Width.Value.GetPixels( Parent.Box.Rect.Width );
		}
		set
		{
			if ( Style is null )
				return;

			Style.Width = Length.Pixels( value );
		}
	}
	public float Height
	{
		get
		{
			if ( Style is null || Parent is null )
				return 0f;

			return Style.Height.Value.GetPixels( Parent.Box.Rect.Height );
		}
		set
		{
			if ( Style is null )
				return;

			Style.Height = Length.Pixels( value );
		}
	}

	/// <summary>
	/// If true, the video is in the process of being loaded (e.g. from a remote server).
	/// </summary>
	public bool IsLoading => _videoLoader?.IsLoading != false || ( IsPlaying && _sinceLastTextureUpdate > 0.2f );
	/// <summary>
	/// If true, the video is playing, is not paused, and has not yet finished playing.
	/// </summary>a
	public bool IsPlaying => !HasReachedEnd && VideoPlayer is not null && !VideoPlayer.IsPaused;
	/// <summary>
	/// If true, this video has reached the end of the file. If ShouldLoop is enabled, the video will
	/// automatically loop.
	/// </summary>
	public bool HasReachedEnd => VideoPlayer != null && VideoPlayer.PlaybackTime >= VideoPlayer.Duration;

	#region Controls
	/// <inheritdoc cref="VideoPlayer.Pause"/>
	public void Pause() => VideoPlayer?.Pause();

	/// <inheritdoc cref="VideoPlayer.IsPaused"/>
	public bool IsPaused => VideoPlayer?.IsPaused == true;

	/// <inheritdoc cref="VideoPlayer.Resume"/>
	public void Resume() => VideoPlayer?.Resume();

	/// <inheritdoc cref="VideoPlayer.TogglePause"/>
	public void TogglePause() => VideoPlayer?.TogglePause();

	/// <inheritdoc cref="VideoPlayer.Seek(float)"/>
	public void Seek( float time ) => VideoPlayer?.Seek( time );

	/// <inheritdoc cref="VideoPlayer.Duration"/>
	public float Duration => VideoPlayer?.Duration ?? 0f;

	/// <summary>
	/// Returns the current playback time in seconds of the video, or if setting,
	/// will seek to the specified playback time in seconds.
	/// </summary>
	public float PlaybackTime
	{
		get => VideoPlayer?.PlaybackTime ?? 0f;
		set => Seek( value );
	}

	/// <inheritdoc cref="VideoPlayer.Stop"/>
	public void Stop() => VideoPlayer?.Stop();

	/// <summary>
	/// Provides access to the various audio-related properties of VideoPlayer such
	/// as Volume and Position.
	/// </summary>
	public IAudioAccessor Audio => _audioAccessor;

	/// <summary>
	/// Specifies a GameObject in the world from which the audio of this video shall be emitted.
	/// </summary>
	public GameObject AudioSource 
	{
		get => _audioSource;
		set
		{
			_audioSource = value;
			if ( _audioAccessor is not null )
			{
				_audioAccessor.Target = value;
			}
		}
	}
	private GameObject _audioSource;
	public MixerHandle TargetMixer
	{
		get => _targetMixer;
		set
		{
			_targetMixer = value;
			if ( _audioAccessor is not null )
			{
				_audioAccessor.TargetMixer = value.Get();
			}
		}
	}
	private MixerHandle _targetMixer;
	#endregion

	// Internal state
	private VideoPlayer VideoPlayer { get; set; }
	private Texture VideoTexture { get; set; }
	private AsyncVideoLoader _videoLoader;
	private Task<VideoPlayer> _videoLoadTask;
	private TrackingAudioAccessor _audioAccessor;
	private CancellationTokenSource _cancelSource = new();
	private bool _shouldPauseNextFrame;
	private RealTimeSince _sinceLastTextureUpdate;

	protected override int BuildHash() => HashCode.Combine( ShowControls, AutoHideControls, AutoHideDelay );

	public override void SetProperty( string name, string value )
	{
		if ( name == "src" )
		{
			VideoPath = value;
		}

		if ( name == "width" )
		{
			Style.Width = Length.Pixels( value.ToFloat( 160 ) );
		}

		if ( name == "height" )
		{
			Style.Height = Length.Pixels( value.ToFloat( 90 ) );
		}

		if ( name == "muted" )
		{
			StartMuted = value.ToBool();
		}

		base.SetProperty( name, value );
	}

	protected async override void OnAfterTreeRender( bool firstTime )
	{
		base.OnAfterTreeRender( firstTime );

		if ( !firstTime )
			return;

		VideoPlayer = new VideoPlayer();

		// Attempt to play whatever video is specified by the initial VideoPath and VideoRoot.
		await PlayVideo();
	}

	/// <summary>
	/// Attempt to play whatever video is specified by the current VideoPath and VideoRoot.
	/// </summary>
	private async Task PlayVideo()
	{
		// Now we're using the new video path and root.
		_previousVideoPath = VideoPath;
		_previousVideoRoot = VideoRoot;

		Stop();

		// Blank out the background when setting a new background image.
		Style.SetBackgroundImage( (Texture)null );
		StateHasChanged();
		VideoTexture = null;

		// Trying to play nothing? Success means stopping the current video.
		if ( string.IsNullOrWhiteSpace( VideoPath ) )
		{
			return;
		}

		// Cancel whatever other video might be loading.
		if ( _videoLoadTask is not null )
		{
			CancelVideoLoad();
			// Wait for that video to finish cancelling before we load our own.
			await _videoLoadTask;
		}

		VideoPlayer ??= new VideoPlayer();
		VideoPlayer.OnTextureData = OnTextureData;

		_cancelSource = new CancellationTokenSource();
		// Cache the CancellationToken because Task.IsCanceled isn't whitelisted,
		// and subsequent calls to PlayVideo would create a new CancellationTokenSource.
		var cancelToken = _cancelSource.Token;

		_videoLoadTask = LoadVideo( cancelToken );
		VideoPlayer = await _videoLoadTask;
		_videoLoadTask = null;

		// If loading the video was cancelled...
		if ( cancelToken.IsCancellationRequested )
		{
			// ...don't touch the video player, and don't update anything, because
			// there may be another video that began loading later and finished before
			// this one, and we don't want to overwrite the effect it had.
			return;
		}

		_shouldPauseNextFrame = !AutoPlay;
		ConfigureAudio();
		UpdateBackgroundImage();
	}

	private void OnTextureData( ReadOnlySpan<byte> span, Vector2 size )
	{
		_sinceLastTextureUpdate = 0f;
		int width = (int)size.x;
		int height = (int)size.y;
		if ( VideoTexture is null || VideoTexture.Size != size )
		{
			VideoTexture = Texture.Create( width, height, ImageFormat.RGBA8888 )
				.WithName( "VideoPanel_Texture" )
				.Finish();
			UpdateBackgroundImage();
		}
		VideoTexture.Update( span, 0, 0, width, height );

		if ( _shouldPauseNextFrame )
		{
			VideoPlayer.Pause();
			_shouldPauseNextFrame = false;
		}
	}

	/// <summary>
	/// By default, will ensure that a TrackingAudioAccessor is created and
	/// configured to use the VideoPlayer and AudioSource of this VideoPanel.
	/// <br/><br/>
	/// Called in PlayVideo after a video is loaded, but before OnPostVideoLoad.
	/// </summary>
	private void ConfigureAudio()
	{
		_audioAccessor ??= new TrackingAudioAccessor()
		{
			Muted = StartMuted
		};
		_audioAccessor.VideoPlayer = VideoPlayer;
		_audioAccessor.Target = AudioSource;
		_audioAccessor.TargetMixer = TargetMixer.Get();
	}

	/// <summary>
	/// By default, sets the background image of this panel to VideoTexture and rebuilds the UI.
	/// <br/><br/>
	/// Called in PlayVideo after a video is loaded, but before OnPostVideoLoad.
	/// </summary>
	private void UpdateBackgroundImage()
	{
		if ( VideoPlayer is null )
			return;

		Style.SetBackgroundImage( VideoTexture );
		StateHasChanged();
	}
	

	/// <summary>
	/// Load whatever video is specified by the current VideoPath and VideoRoot, returning
	/// an instance of a VideoPlayer. Called during PlayVideo.
	/// </summary>
	private async Task<VideoPlayer> LoadVideo( CancellationToken cancelToken )
	{
		_videoLoader ??= new AsyncVideoLoader( VideoPlayer );

		if ( VideoRoot == VideoRoot.WebStream )
		{
			return await _videoLoader.LoadFromUrl( VideoPath, cancelToken );
		}
		else
		{
			return await _videoLoader.LoadFromFile( VideoRoot.AsFileSystem(), VideoPath, cancelToken );
		}
	}

	private void CancelVideoLoad()
	{
		_cancelSource?.Cancel();
		_cancelSource?.Dispose();
		_cancelSource = null;
	}

	// Refresh detection
	private string _previousVideoPath;
	private VideoRoot _previousVideoRoot;

	private bool VideoHasChanged => _previousVideoPath != VideoPath || _previousVideoRoot != VideoRoot;

	public override void Tick()
	{
		if ( VideoPlayer is null )
			return;

		// If the VideoPath or VideoRoot have changed...
		if ( VideoHasChanged )
		{
			// ...play that new video instead.
			_ = PlayVideo();
			return;
		}

		// The VideoTexture will not update unless Present is called.
		VideoPlayer.Present();

		DetectAndHandleLoop();
	}

	/// <summary>
	/// By default, detects whether the video has reached its conclusion. 
	/// If so, and if ShouldLoop is true, the playback time will be 
	/// set to the start of the video.
	/// </summary>
	private void DetectAndHandleLoop()
	{
		// We use a custom looping mechanism because VideoPlayer.Repeat seems to mess up PlaybackTime.
		if ( ShouldLoop && HasReachedEnd )
		{
			VideoPlayer.Seek( 0f );
		}
	}

	public override void OnDeleted()
	{
		CancelVideoLoad();
		VideoPlayer?.Stop();
		VideoPlayer?.Dispose();
		VideoTexture?.Dispose();
		_audioAccessor?.Dispose();
	}
}
