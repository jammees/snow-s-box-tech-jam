using Sandbox;

public sealed class LoadPlayerClothes : Component
{
	[Property]
	private SkinnedModelRenderer PlayerModel { get; set; }
	
	protected override void OnStart()
	{
		if ( IsProxy )
			return;
		
		var container = ClothingContainer.CreateFromLocalUser();
		container.Apply( PlayerModel );

		PlayerModel.GameObject.Root.Network.Refresh();
	}
}
