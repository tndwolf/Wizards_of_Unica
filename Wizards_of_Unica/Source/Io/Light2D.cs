using System;
using SFML.Graphics;
using SFML.Window;

namespace tndwolf.ECS {
	public class Light2D: Widget {
		Object2D cache = null;
		Color color = Color.White;
		int lookAt = -1;
		float radius = 1f;
		VertexArray shape = new VertexArray();
		bool updateCache = false;

		public Light2D(int entity) : base(entity) {
			shape.PrimitiveType = PrimitiveType.TrianglesFan;
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

		public Color Color {
			set {
				color = value;
				Recalculate();
			}
		}

		/// <summary>
		/// Gets or sets the entity to look at. If the entity is negative or it does
		/// not have an Object2D to point at, the light is static
		/// </summary>
		/// <value>The entity.</value>
		public int LookAt {
			get { return lookAt; }
			set { lookAt = value; updateCache = true; }
		}

		public float Radius {
			set {
				radius = value;
				Recalculate();
			}
		}

		protected void Recalculate() {
			var tColor = new Color(color.R, color.G, color.B, 0);
			shape.Clear();
			shape.Append(new Vertex(Position, color));
			for(double i = 0; i <= 6.29; i += Math.PI / 6.0) {
				var tPosition = new Vector2f(
					(float)(radius * Math.Cos(i) + Position.X),
					(float)(radius * Math.Sin(i) + Position.Y)
				);
				shape.Append(new Vertex(tPosition, tColor));
			}
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

