using Sandbox.UI;
using System;

namespace Duccsoft;

/// <summary>
/// A Panel meant to stretch over an entire VideoPanel and provide a progress bar and buttons.
/// </summary>
public partial class VideoControlOverlay : Panel
{
	/// <summary>
	/// The VideoPanel instance that we will be controlling. You may replace this with any
	/// <see cref="IVideoPanel"/> object that you that you wish to control using this UI.
	/// </summary>
	public IVideoPanel VideoPanel { get; set; }
	/// <summary>
	/// If true, the video controls will automatically be hidden after the AutoHideDelay period has elapsed
	/// and the user is not hovering their mouse over this panel.
	/// </summary>
	public bool AutoHide { get; set; } = true;
	/// <summary>
	/// If AutoHide is true, this is the time in seconds that must elapse before this panel
	/// will conceal the video controls.
	/// </summary>
	public float AutoHideDelay { get; set; } = 1f;

	#region Presentation
	private string OverlayClass
	{
		get
		{
			var overlayClass = VideoPanel?.IsLoading == true ? "loading" : string.Empty;

			if ( !AutoHide )
				return overlayClass;

			var hovered = PseudoClass.HasFlag( PseudoClass.Hover );
			if ( hovered )
			{
				_lastHovered = 0f;
				return overlayClass;
			}
			return _lastHovered > AutoHideDelay ? $"{overlayClass} conceal" : overlayClass;
		}
	}
	private string TimecodeAreaClass => ProgressSeconds > 3600 || DurationSeconds > 3600
		? "big"
		: string.Empty;
	private string ProgressText => FormatTime( ProgressSeconds );
	private string DurationText => FormatTime( DurationSeconds );
	private string FormatTime( float seconds )
	{
		var time = TimeSpan.FromSeconds( seconds );
		return time.Hours > 0
			? time.ToString( "hh':'mm':'ss" )
			: time.ToString( "mm':'ss" );
	}
	private string PlayButtonIcon
	{
		get
		{
			if ( !VideoPanel.IsValid() )
				return "play_arrow";

			return VideoPanel.IsPaused ? "play_arrow" : "pause";
		}
	}

	private float ProgressSeconds => VideoPanel?.PlaybackTime ?? 0f;
	private float DurationSeconds => VideoPanel?.Duration ?? 0f;
	private string VolumeButtonIcon => VideoPanel?.Audio?.Muted == false ? "volume_up" : "volume_off";
	#endregion

	private RealTimeSince _lastHovered = 20f;

	protected override int BuildHash()
	{
		return HashCode.Combine( VideoPanel?.IsPlaying, VideoPanel?.IsPaused, VideoPanel?.Audio?.Muted, (int)ProgressSeconds, TimecodeAreaClass, OverlayClass  );
	}

	#region Controls
	private float PlaybackTime
	{
		get => ProgressSeconds;
		set => VideoPanel?.Seek( value );
	}

	private void ProgressBarChanged( float value )
	{
		PlaybackTime = value;
	}

	private void TogglePause() => VideoPanel?.TogglePause();
	private void ToggleMute()
	{
		if ( !VideoPanel.IsValid() || VideoPanel.Audio is null )
			return;

		VideoPanel.Audio.Muted = !VideoPanel.Audio.Muted;
	}
	#endregion
}
