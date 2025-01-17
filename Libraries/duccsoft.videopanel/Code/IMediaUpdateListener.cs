using System;

namespace Duccsoft;

/// <summary>
/// Represents a presentation-related object that must be updated every frame.
/// For example, a video that must update a texture, or a sound that must update 
/// its position.
/// </summary>
public interface IMediaUpdateListener : IDisposable
{
	/// <summary>
	/// Contains the code that must run every frame. Will only be called
	/// if this object has been registered with a MediaUpdateSystem.
	/// </summary>
	void MediaUpdate();

	/// <inheritdoc cref="MediaUpdateSystem.Register(IMediaUpdateListener)"/>
	public static void Register( IMediaUpdateListener listener )
	{
		if ( listener is null )
			return;

		var system = Game.ActiveScene?.GetSystem<MediaUpdateSystem>();
		if ( system is null )
			return;

		system.Register( listener );
	}

	/// <inheritdoc cref="MediaUpdateSystem.Unregister(IMediaUpdateListener)"/>
	public static void Unregister( IMediaUpdateListener listener )
	{
		if ( listener is null )
			return;

		var system = Game.ActiveScene?.GetSystem<MediaUpdateSystem>();
		if ( system is null )
			return;

		system.Unregister( listener );
	}
}
