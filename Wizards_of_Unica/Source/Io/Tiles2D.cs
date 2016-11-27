using SFML.Graphics;
using SFML.Window;

namespace tndwolf.ECS {
	public class Tiles2D: Object2D {
		public Tiles2D(int entity) : base(entity) { }

		/// <summary>
		/// Draw the tiles.
		/// </summary>
		/// <param name="target">Target.</param>
		/// <param name="states">States.</param>
		public override void Draw(RenderTarget target, RenderStates states) {
			states.Transform.Translate(Position);
			states.Transform.Scale(Scale);
			var dx = new Vector2f(SpriteSheet.GetLocalBounds().Width, 0f);
			var dy = new Vector2f(-dx.X * GridWidth, SpriteSheet.GetLocalBounds().Height);
			for(int y = 0; y < GridHeight; y++) {
				for(int x = 0; x < GridWidth; x++) {
					SpriteSheet.Draw(target, states);
					states.Transform.Translate(dx);
				}
				states.Transform.Translate(dy);
			}
		}


		public int GridHeight { get; set; }

		public int GridWidth { get; set; }
	}
}

