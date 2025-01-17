namespace Sandbox;

// from sbox-scenestaging repo

public sealed class GameNetworkManager : Component, Component.INetworkListener
{
	[Property] public GameObject PlayerPrefab { get; set; }
	[Property] public GameObject SpawnPoint { get; set; }

	public void OnActive( Connection channel )
	{
		var clothing = new ClothingContainer();
		clothing.Deserialize( channel.GetUserData( "avatar" ) );

		var player = PlayerPrefab.Clone( SpawnPoint.WorldTransform );

		// Assume that if they have a skinned model renderer, it's the citizen's body
		if ( player.Components.TryGet<SkinnedModelRenderer>( out var body, FindMode.EverythingInSelfAndDescendants ) )
		{
			clothing.Apply( body );
		}
		
		player.NetworkSpawn( channel );
	}
}
