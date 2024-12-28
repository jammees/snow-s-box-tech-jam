using Sandbox;

public sealed class LoadPlayerClothes : Component
{
	[Property]
	private SkinnedModelRenderer PlayerModel { get; set; }
	
	protected override void OnStart()
	{
		var container = ClothingContainer.CreateFromLocalUser();
		container.Apply( PlayerModel );
	}
}
