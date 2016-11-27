namespace tndwolf.ECS {
	/// <summary>
	/// This components automatically updates the specific Layer components to be
	/// centered on one of its child entities (even if nested inside another layer).
	/// </summary>
	public class Camera2D: Widget {
		public Camera2D (int entity): base(entity) { }

		/// <summary>
		/// Gets or sets the entity to look at. The entity must have a valid Widget
		/// as one of its components to be used as reference
		/// </summary>
		/// <value>The entity.</value>
		public int LookAt { get; set; }

		public override void Update(World world) {
			var layer = world.GetComponent<Layer>(Parent);
			var at = layer.Get(LookAt);
			var halfWindow = Services.Window.Size / 2;
			layer.Position = new SFML.Window.Vector2f(
				halfWindow.X - at.Position.X * layer.ScaleX, 
				halfWindow.Y - at.Position.Y * layer.ScaleY
			);
		}
	}
}

