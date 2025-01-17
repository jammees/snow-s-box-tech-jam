namespace Duccsoft;

/// <summary>
/// Responsible for updating every IMediaUpdateListener that has registered with this system.
/// </summary>
public class MediaUpdateSystem : GameObjectSystem
{
	public MediaUpdateSystem( Scene scene ) : base( scene )
	{
		Listen( Stage.StartUpdate, 0, Update, "Media Update" );
	}

	private HashSet<IMediaUpdateListener> _listeners = new();

	private void Update()
	{
		foreach( var listener in _listeners )
		{
			listener?.MediaUpdate();
		}
	}

	/// <summary>
	/// Allows an IMediaUpdateListener to have its MediaUpdate method called every frame.
	/// </summary>
	public void Register( IMediaUpdateListener listener )
	{
		_listeners.Add( listener );
	}

	/// <summary>
	/// Removes an IMediaUpdateListener from the set of listeners that would have their
	/// MediaUpdate method called every frame. This should be called in the destructor
	/// or in Dispose.
	/// </summary>
	public void Unregister( IMediaUpdateListener listener )
	{
		_listeners.Remove( listener );
	}
}
