using SFML.Graphics;
using System.Collections.Generic;
using SFML.Window;

namespace tndwolf.ECS {
	public class TextBox: Widget {
		public RectangleShape bgRect = new RectangleShape();
		List<Text> rows = new List<Text>();
		float lastRowWidth = 0;

		public TextBox(int entity) : base(entity) {
			Size = 16;
			BackgroundColor = Color.Black;
			DefaultColor = Color.White;
		}

		public Color BackgroundColor {
			set { bgRect.FillColor = value; }
		}

		public Color DefaultColor { get; set; }

		public override void Draw(RenderTarget target, RenderStates states) {
			states.Transform.Translate(Position);
			states.Transform.Scale(Scale);
			bgRect.Draw (target, states);
			foreach(var text in rows) {
				text.Draw(target, states);
			}
		}

		public Font Font { get; set; }

		public float Height {
			get { return bgRect.Size.Y; }
			set {
				bgRect.Size = new Vector2f(bgRect.Size.X, value);
			}
		}

		public float MaxWidth { get; set; }

		public int Size { get; set; }

		public float Width {
			get { return bgRect.Size.X; }
			set {
				bgRect.Size = new Vector2f(value, bgRect.Size.Y);
			}
		}

		public void Append(string text, Color? color = null) {
			if(rows.Count < 1) {
				var newText = new Text(text, Font);
				newText.CharacterSize = (uint)Size;
				newText.Color = (color == null) ? DefaultColor : (Color)color;
				rows.Add(newText);
			}
			else {
				var lastText = rows[rows.Count - 1];
				lastText.DisplayedString += text;
			}
		}

		public void Write(string text, Color? color = null) {
			var newText = new Text(text, Font);
			newText.CharacterSize = (uint)Size;
			newText.Color = (color == null) ? DefaultColor : (Color)color;
			var width = newText.GetLocalBounds().Width;
			var height = newText.GetLocalBounds().Height;
			if(rows.Count != 0) {
				var lastText = rows[rows.Count - 1];
				if(width + lastRowWidth > MaxWidth) {
					newText.Position = new Vector2f(0f, lastText.Position.Y + height);
					lastRowWidth = width;
					Height += height;
				}
				else {
					newText.Position = new Vector2f(
						lastText.GetLocalBounds().Left + 
						lastText.GetLocalBounds().Width, 
						lastText.Position.Y);
					lastRowWidth += width;
				}
				if(lastRowWidth > Width)
					Width = lastRowWidth;
			}
			else {
				Width = width;
				Height = height;
			}
			rows.Add(newText);
		}
	}
}

