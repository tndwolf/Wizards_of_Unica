using System.Collections.Generic;
using SFML.Graphics;

namespace tndwolf.ECS {
	/// <summary>
	/// Widget containing other widgets. Transformations applies to this will
	/// be applied to the contained widgets
	/// </summary>
	public class Layer: Widget, IWidgetContainer {
		RenderTexture layerTexture = new RenderTexture(2048, 2048);

		List<Widget> widgets = new List<Widget> ();

		public Layer(int entity): base (entity) {
			ClearColor = Color.Transparent;
		}

		/// <summary>
		/// Add the specified widget to this container.
		/// </summary>
		/// <param name="widget">Widget.</param>
		public virtual void Add(Widget widget) {
			widgets.Add(widget);
		}

		/// <summary>
		/// Gets or sets the blend mode for the whole layer.
		/// </summary>
		/// <value>The blend mode.</value>
		public BlendMode BlendMode { get; set; }

		public void Clear() {
			foreach(var widget in widgets) {
				widget.DeleteMe = true;
				if(widget is IWidgetContainer) {
					(widget as IWidgetContainer).Clear();
				}
			}
			widgets.Clear();
		}

		public Color ClearColor { get; set; }

		public override void Draw (RenderTarget target, RenderStates states) {
			layerTexture.Clear (ClearColor);
			foreach (var widget in widgets) {
				layerTexture.Draw (widget);
			}
			layerTexture.Display ();
			states.Transform.Translate (Position);
			states.Transform.Scale (Scale);
			states.BlendMode = BlendMode;
			target.Draw (new Sprite(layerTexture.Texture), states);
		}

		/// <summary>
		/// Get the widget with the specified entity.
		/// </summary>
		/// <param name="entity">Entity identifier.</param>
		public virtual Widget Get(int entity) {
			Widget res = null;
			foreach(var widget in widgets) {
				if(widget.Entity == entity) {
					res = widget;
					break;
				}
				if(widget is Layer) {
					res = (widget as Layer).Get(entity);
					if(res != null) {
						break;
					}
				}
			}
			return res;
		}

		public override void Update (World world) {
			widgets.RemoveAll((w) => w.DeleteMe);
			foreach (var widget in widgets) {
				widget.Update (world);
			}
			widgets.Sort();
		}
	}
}

