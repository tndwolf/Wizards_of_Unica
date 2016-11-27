using SFML.Graphics;
using SFML.Window;

namespace tndwolf.ECS {
	/// <summary>
	/// Progress bar widget.
	/// </summary>
	public class ProgressBar: Widget {
		public enum BarDirection {
			RIGHT,
			UP
		}

		protected RectangleShape border = new RectangleShape();
		protected float max = 100f;
		protected float current = 100f;
		protected RectangleShape progress = new RectangleShape();
		protected Vector2f size;

		public ProgressBar(int entity): base(entity) {
			border.FillColor = Color.Transparent;
			border.OutlineColor = Color.White;
			border.OutlineThickness = 1f;
			progress.FillColor = Color.White;
			Direction = BarDirection.UP;
			IsManaged = true;
		}

		/// <summary>
		/// Gets or sets the color of the border.
		/// </summary>
		/// <value>The color of the border.</value>
		public Color BorderColor {
			get { return border.OutlineColor; }
			set { border.OutlineColor = value; }
		}

		/// <summary>
		/// Gets or sets the border thickness.
		/// </summary>
		/// <value>The border thickness.</value>
		public float BorderThickness {
			get { return border.OutlineThickness; }
			set { progress.OutlineThickness = value; }
		}

		/// <summary>
		/// Internal method to recalculate the bar and position it inside the widget's
		/// border.
		/// </summary>
		protected void Calculate() {
			var cCurrent = current / max;
			//Services.Logger.Debug("ProgressBar.Calculate", current + " vs " + max);
			switch(Direction) {
				case BarDirection.RIGHT:
					cCurrent *= Size.X;
					break;
				case BarDirection.UP:
					cCurrent *= Size.Y;
					progress.Position = new Vector2f(0f, Size.Y - cCurrent);
					progress.Size = new Vector2f(Size.X, cCurrent);
					break;
			}
		}

		/// <summary>
		/// Gets or sets the current value within [0, Max].
		/// </summary>
		/// <value>The current.</value>
		public float Current {
			get { return current; }
			set { 
				current = (value > Max) ? Max : (value < 0) ? 0 : value; 
				Calculate(); 
			}
		}

		/// <summary>
		/// Gets or sets the "fill" direction of the progress bar.
		/// </summary>
		/// <value>The direction.</value>
		public BarDirection Direction { get; set; }

		public override void Draw(RenderTarget target, RenderStates states) {
			states.Transform.Translate(Position);
			states.Transform.Scale(Scale);
			progress.Draw(target, states);
			border.Draw(target, states);
		}

		/// <summary>
		/// Gets or sets the color of the bar.
		/// </summary>
		/// <value>The color of the fill.</value>
		public Color FillColor {
			get { return progress.FillColor; }
			set { progress.FillColor = value; }
		}

		/// <summary>
		/// Gets or sets the maximum value of the bar.
		/// </summary>
		/// <value>The max.</value>
		public float Max {
			get { return max; }
			set { max = value; Calculate(); }
		}

		/// <summary>
		/// Gets or sets the size of the widget. The bar occupies all of it
		/// except for the border
		/// </summary>
		/// <value>The size.</value>
		public Vector2f Size {
			get { return size; }
			set { 
				size = value; 
				border.Size = value; 
				Calculate(); 
			}
		}
	}
}

