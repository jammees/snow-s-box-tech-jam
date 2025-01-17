using System.Threading.Tasks;
using System.Threading;
using System;

namespace Duccsoft;

/// <summary>
/// Provides a handy asynchronous wrapper for loading a VideoPlayer and waiting
/// until its video and audio are both loaded.
/// </summary>
public class AsyncVideoLoader
{
	public AsyncVideoLoader() 
	{
		_videoPlayer = new VideoPlayer();
	}

	public AsyncVideoLoader( VideoPlayer player )
	{
		_videoPlayer = player ?? new VideoPlayer();
	}

	public bool IsLoading { get; private set; }

	private VideoPlayer _videoPlayer;
	private Action _onLoaded;
	private Action _onAudioReady;

	public async Task<VideoPlayer> LoadFromUrl( string url, CancellationToken cancelToken = default )
	{
		void Play( VideoPlayer player ) => player.Play( url );

		await Load( Play, cancelToken );
		return _videoPlayer;
	}

	public async Task<VideoPlayer> LoadFromFile( BaseFileSystem fileSystem, string path, CancellationToken cancelToken )
	{
		void Play( VideoPlayer player ) => player.Play( fileSystem, path );

		await Load( Play, cancelToken );
		return _videoPlayer;
	}

	private async Task Load( Action<VideoPlayer> playAction, CancellationToken cancelToken = default )
	{
		// Attempting to play a video from a thread would throw an exception.
		await GameTask.MainThread( cancelToken );

		if ( IsLoading )
		{
			throw new InvalidOperationException( "Another video was already being loaded. Check IsLoading or create a new instance of AsyncVideoLoader." );
		}

		IsLoading = true;

		bool videoLoaded = false;
		bool audioLoaded = false;

		// Assign private members instead of named methods to the invocation lists of the
		// VideoPlayer delegates to break reference equality between runs.
		_onLoaded = () => videoLoaded = true;
		_onAudioReady = () => audioLoaded = true;

		_videoPlayer.OnLoaded = _onLoaded;
		_videoPlayer.OnAudioReady = _onAudioReady;

		playAction?.Invoke( _videoPlayer );

		// Non-blocking spin until video and audio are loaded.
		while ( !videoLoaded || !audioLoaded )
		{
			// If OnLoaded or OnAudioReady are changed externally before we're finished
			// loading, the video will likely never load. Abort to avoid spinning forever.
			var callbacksChanged = _onLoaded != _videoPlayer.OnLoaded || _onAudioReady != _videoPlayer.OnAudioReady;
			if ( callbacksChanged || cancelToken.IsCancellationRequested )
			{
				IsLoading = false;
				return;
			}

			await GameTask.Yield();
		}

		IsLoading = false;
	}
}
