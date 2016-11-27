using System;
using SFML.Graphics;
using SFML.Window;

namespace tndwolf.ECS {
	public class Polygon2D: Widget {
		Object2D cache = null;
		Color color = Color.White;
		int lookAt = -1;
		VertexArray shape = new VertexArray();
		bool updateCache = false;

		public Polygon2D(int entity) : base(entity) {
			DefaultColor = Color.White;
			shape.PrimitiveType = PrimitiveType.Triangles;
		}

		public void AddVertex(Vector2f position) {
			shape.Append(new Vertex(position, DefaultColor));
		}

		public void AddVertex(Vector2f position, Color color) {
			shape.Append(new Vertex(position, color));
		}

		/// <summary>
		/// Draw the sprite.
		/// </summary>
		/// <param name="target">Target.</param>
		/// <param name="states">States.</param>
		public override void Draw(RenderTarget target, RenderStates states) {
			states.Transform.Translate(Position);
			states.Transform.Scale(Scale);
			target.Draw(shape, states);
		}

		public Color DefaultColor { get; set; }

		/// <summary>
		/// Gets or sets the entity to look at. If the entity is negative or it does
		/// not have an Object2D to point at, the light is static
		/// </summary>
		/// <value>The entity.</value>
		public int LookAt {
			get { return lookAt; }
			set { lookAt = value; updateCache = true; }
		}

		public override void Update(World world) {
			if(updateCache) {
				cache = world.GetComponent<Object2D>(lookAt);
				updateCache = false;
			}
			if(cache != null) {
				Position = cache.Position;
			}
		}
	}
}

