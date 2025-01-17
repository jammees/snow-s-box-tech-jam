namespace Duccsoft;

/// <summary>
/// Specifies the root directory of video path.
/// </summary>
public enum VideoRoot
{
	/// <summary>
	/// Video path is relative to FileSystem.Mounted
	/// </summary>
	[Icon("inventory_2")]
	MountedFileSystem,
	/// <summary>
	/// Video path is relative to FileSystem.Data
	/// </summary>
	[Icon("save")]
	DataFileSystem,
	/// <summary>
	/// Video path is relative to FileSystem.OrganizationData
	/// </summary>
	[Icon("business")]
	OrganizationDataFileSystem,
	/// <summary>
	/// Video path is a URL to an video stream on the internet. This should be
	/// a direct link to the stream itself.
	/// <br/><br/>
	/// To manually test whether a stream is valid, you can use the Open Network Stream 
	/// feature of VLC Media Player, but not every stream that plays in VLC will work here.
	/// </summary>
	[Icon("language")]
	WebStream
}

public static class VideoRootExtensions
{
	/// <summary>
	/// Returns the file system associated with the given VideoRoot, or null if 
	/// no file system exists.
	/// </summary>
	public static BaseFileSystem AsFileSystem( this VideoRoot videoRoot )
	{
		return videoRoot switch
		{
			VideoRoot.MountedFileSystem				=> FileSystem.Mounted,
			VideoRoot.DataFileSystem				=> FileSystem.Data,
			VideoRoot.OrganizationDataFileSystem	=> FileSystem.OrganizationData,
			_ => null
		};
	}
}	
