using System.Collections.Generic;
using SFML.Graphics;
using SFML.Window;

namespace tndwolf.ECS {
	public class Renderer: GameSystem {
		public const int ROOT_LAYER = 1000; // TODO this magic number for the root _WILL_ cause problems...
		List<Widget> allWidgets = new List<Widget>();
		List<Widget> rootWidgets = new List<Widget>();

		public Camera2D ActiveCamera {
			get { return allWidgets.Find((c) => c is Camera2D) as Camera2D; }
		}

		override public int Diagnose() {
			Services.Logger.Debug("Renderer.Diagnose", "----- Total widgets: " + allWidgets.Count);
			Services.Logger.Debug("Renderer.Diagnose", "----- Total root: " + rootWidgets.Count);
			return 0;
		}

		public int FPS { 
			set { Services.Window.SetFramerateLimit((uint)value); }
		}

		override public void Initialize(World world) {
			Services.Graphics.DefaultFont = new Font(Services.Graphics.AssetsFolder + "/munro.ttf");
			allWidgets = new List<Widget>();
			rootWidgets = new List<Widget>();
			FPS = 30;
		}

		override public bool Register(GameComponent component) {
			if(component is Widget) {
				var widget = component as Widget;
				allWidgets.Add(widget);
				var parent = allWidgets.Find((p) => p.Entity == widget.Parent);
				if(parent != null && parent is Layer) {
					(parent as Layer).Add((Widget)component);
				}
				else {
					rootWidgets.Add(widget);
				}
				if(component is TextBox) {
					(component as TextBox).Font = Services.Graphics.DefaultFont;
				}
				return true;
			}
			return false;
		}

		override public void UnRegister(GameComponent component) {
			var buff = component as Widget;
			allWidgets.Remove(buff);
			rootWidgets.Remove(buff);
			if(component is IWidgetContainer) {
				(component as IWidgetContainer).Clear();
			}
		}

		override public void Update(World world) {
			Services.Window.Clear();
			foreach(var widget in rootWidgets) {
				widget.Update(world);
				Services.Window.Draw(widget);
			}
			Services.Window.Display();
		}

		public Vector2f WorldScale {
			get { return rootWidgets.Find((w) => w.Entity == ROOT_LAYER).Scale; }
		}
	}
}

