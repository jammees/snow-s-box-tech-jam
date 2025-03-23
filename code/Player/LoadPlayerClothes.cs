using Sandbox;
using static Sandbox.Component;

public sealed class LoadPlayerClothes : Component, INetworkSpawn
{
	[Property]
	private SkinnedModelRenderer PlayerModel { get; set; }
	
	// protected override void OnStart()
	// {
	// 	var container = ClothingContainer.CreateFromLocalUser();
	// 	container.Apply( PlayerModel );
	// }

	public void OnNetworkSpawn( Connection owner )
	{
		var container = ClothingContainer.CreateFromJson(owner.GetUserData("avatar"));
		container.Apply( PlayerModel );
	}
}
