using System.Collections.Generic;
using SFML.Window;

namespace tndwolf.ECS {
	public class UserInterafaceSystem: GameSystem {
		Widget inFocus = null;
		List<Widget> widgets;

		public int HoldFor { get; set; }

		public Widget InFocus {
			get { return inFocus; }
			set { if(widgets.Contains(value)) inFocus = value; }
		}

		override public void Update(World world) {
			HoldFor -= world.DeltaTime;
			//Services.Logger.Debug("UserInterafaceSystem.Update", "Holding for " + HoldFor);
			if(HoldFor > 0) return;
			switch(Services.Inputs.Command) {
				case Command.MOVE:
					if(inFocus == null) {
						var turnManager = world.GetSystem<TurnManager>();
						turnManager.SetNextPlayerAction(
							TurnManager.PlayerAction.MOVE,
							Services.Inputs.HorizontalMovement,
							Services.Inputs.VerticalMovement);
					}
					else {
						world.Delete(inFocus);
						inFocus = null;
					}
					break;
				case Command.LEFT_MOUSE:
					if(inFocus == null) {
						var mouse = Services.Inputs.MousePosition;
						var cell = Services.GameMechanics.MouseToWorld(mouse.X, mouse.Y);
						var turnManager = world.GetSystem<TurnManager>();
						turnManager.SetNextPlayerAction(TurnManager.PlayerAction.SKILL, cell[0], cell[1]);
					}
					else {
						world.Delete(inFocus);
						inFocus = null;
					}
					break;
				case Command.SKILL:
					Services.GameMechanics.SelectedGuiSkill = int.Parse(Services.Inputs.Unicode);
					break;
				case Command.TAB:
					Services.GameMechanics.ToggleGrid();
					break;
				case Command.WHEEL:
					var root = world.GetComponent<Layer>(Renderer.ROOT_LAYER);
					Services.Logger.Debug("Scale", root.ScaleX.ToString());
					if(root != null) {
						var scaleK = Services.Inputs.WheelMovement / 10f;
						var scaleV = root.Scale + new Vector2f(scaleK, scaleK);
						if(scaleV.X <= 4f && scaleV.X >= 0.7f) {
							root.Scale = root.Scale + new Vector2f(scaleK, scaleK);
						}
						Services.Inputs.Clear();
					}
					break;
			}
			Services.Inputs.Clear();
		}

		#region GameSystem
		override public void Initialize(World world) {
			widgets = new List<Widget>();
		}

		override public int Diagnose() {
			Services.Logger.Debug("UserInterafaceSystem.Diagnose", "----- Total components: " + widgets.Count);
			return 0;
		}

		override public bool Register(GameComponent component) {
			if(component is Widget) {
				widgets.Add(component as Widget);
				return true;
			}
			return false;
		}

		override public void UnRegister(GameComponent component) {
			var refComponent = component as Widget;
			widgets.Remove(refComponent);
		}
		#endregion
	}
}
