using System;
using SFML.Graphics;
using SFML.Window;

namespace tndwolf.ECS {
	public interface IWidgetContainer {
		void Clear();
	}

	public class Widget: GameComponent, Drawable, IComparable {
		public Widget(int entity = -1) : base(entity) {
			IsManaged = true;
			Scale = new Vector2f(1f, 1f);
		}

		public int Parent { get; set; }

		public Vector2f Position { get; set; }

		public Vector2f Scale { get; set; }

		public float ScaleX {
			get { return Scale.X; }
			set { Scale = new Vector2f(value, ScaleY); }
		}

		public float ScaleY {
			get { return Scale.Y; }
			set { Scale = new Vector2f(ScaleX, value); }
		}

		public float X {
			get { return Position.X; }
			set { Position = new Vector2f(value, Y); }
		}

		public float Y {
			get { return Position.Y; }
			set { Position = new Vector2f(X, value); }
		}

		public int Z {
			get;
			set;
		}

		#region Drawable implementation
		public virtual void Draw(RenderTarget target, RenderStates states) {
			return;
		}
		#endregion

		#region IComparable implementation
		public virtual int CompareTo(object obj) {
			try {
				var rhs = obj as Widget;
				return Z.CompareTo(rhs.Z);
			}
			catch {
				Services.Logger.Warn("Widget", "Trying to compare to non widget object");
				return 0;
			}
		}
		#endregion
	}
}

