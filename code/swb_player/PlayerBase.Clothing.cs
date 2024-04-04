using System.Collections.Generic;

namespace SWB.Player;

public partial class PlayerBase
{
	private ClothingContainer clothingContainer;
	private List<SkinnedModelRenderer> clothingRenderers = new();

	void ApplyClothes( Connection connection )
	{
		var clothesJSON = connection.GetUserData( "avatar" );
		clothingContainer = ClothingContainer.CreateFromJson( clothesJSON );
		clothingContainer.Apply( BodyRenderer );

		BodyRenderer.GameObject.Children.ForEach( c =>
		{
			if ( c.Name.StartsWith( "Clothing" ) )
			{
				var renderer = c.Components.Get<SkinnedModelRenderer>();
				clothingRenderers.Add( renderer );
			}
		} );
	}

	void UpdateClothes()
	{
		// Can take a while to spawn on clients so we check here until they are spawned in
		if ( clothingRenderers.Count == 0 )
		{
			BodyRenderer.GameObject.Children.ForEach( c =>
			{
				if ( c.Name.StartsWith( "Clothing" ) )
				{
					var renderer = c.Components.Get<SkinnedModelRenderer>();
					clothingRenderers.Add( renderer );
				}
			} );
		}

		if ( !IsProxy && IsAlive && IsFirstPerson )
		{
			BodyRenderer.RenderType = ModelRenderer.ShadowRenderType.ShadowsOnly;
		}
		else
		{
			BodyRenderer.RenderType = ModelRenderer.ShadowRenderType.On;
		}

		clothingRenderers.ForEach( c =>
		{
			c.RenderType = BodyRenderer.RenderType;
		} );
	}
}
