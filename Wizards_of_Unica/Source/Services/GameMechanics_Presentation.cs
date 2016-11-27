using System;
using System.Collections.Generic;
using SFML.Graphics;
using SFML.Window;

namespace tndwolf.ECS {
	public partial class GameMechanics {
		public class Portrait {
			public string Name = "";
			public Object2D Icon = null;
			public ProgressBar Progress = null;
		}

		public readonly Color SELECTED_BORDER_COLOR = new Color(0, 255, 255);
		public readonly Color UNSELECTED_BORDER_COLOR = new Color(127, 127, 127);

		List<Portrait> playerPortraits = new List<Portrait>(); // 0 is always the player, then there are the skills
		Portrait targetPortrait = new Portrait();

		public int AddCamera(int parent, int lookAt) {
			var res = World.GenerateNewEntityId();
			var camera = new Camera2D(res);
			camera.Parent = parent;
			camera.LookAt = lookAt;
			World.Add(camera);
			return res;
		}

		public int AddLight(int radius, int red, int green, int blue, int alpha) {
			var res = World.GenerateNewEntityId();
			var light = new Light2D(res);
			light.Color = new Color((byte)red, (byte)green, (byte)blue, (byte)alpha);
			light.Radius = radius;
			return res;
		}

		public int AddParticle(string template, int gridX, int gridY) {
			var res = World.GenerateNewEntityId();
			Services.GameFactory.BuildFromTemplate(World, template, res);
			var ps = World.GetComponent<ParticleSystem>(res);
			var tmpPosition = WorldToPixel(gridX, gridY);
			ps.Position = new Vector2f(tmpPosition[0], tmpPosition[1]);
			Services.Logger.Debug("GameMechanics.AddParticle", string.Join(",", new int[] { gridX, gridY }) + " -> " + string.Join(",", tmpPosition));
			return res;
		}

		public Portrait buildPortrait(int guiId, string templateId, string potraitName, float x, float y, Color barColor) {
			var xIcon = Utils.XmlParser.GetNode("object2d", Services.GameFactory.GetById(templateId));
			var icon = Services.GameFactory.LoadObject2D(xIcon, guiId);
			Services.GameFactory.LoadWidget(xIcon, icon);
			var scale = 2f;
			var res = new Portrait();
			res.Name = potraitName;
			res.Icon = icon;
			res.Icon.Position = new Vector2f(x, y);
			res.Icon.Scale = new Vector2f(scale, scale);
			res.Progress = new ProgressBar(guiId);
			res.Progress.Size = new Vector2f(32f, 32f);
			res.Progress.Scale = new Vector2f(scale, scale);
			res.Progress.Position = new Vector2f(x, y);
			res.Progress.Current = 0f;
			res.Progress.FillColor = barColor;
			res.Progress.BorderColor = UNSELECTED_BORDER_COLOR;
			World.Add(res.Icon);
			World.Add(res.Progress);
			return res;
		}

		public void LookAt(int entity, int lookAt) {
			var cam = World.GetComponent<Camera2D>(entity);
			if(cam != null) {
				cam.LookAt = lookAt;
			}
			else {
				var light = World.GetComponent<Light2D>(entity);
				if(light != null) {
					light.LookAt = lookAt;
				}
			}
		}

		public void LookAt(int entity, int gridX, int gridY) {
			var cellSize = gridManager.CellSize;
			var cam = World.GetComponent<Camera2D>(entity);
			if(cam != null) {
				cam.Position = new Vector2f(cellSize * gridX, cellSize * gridY);
				cam.LookAt = -1;
			}
			else {
				var light = World.GetComponent<Light2D>(entity);
				if(light != null) {
					light.Position = new Vector2f(cellSize * gridX, cellSize * gridY);
					light.LookAt = -1;
				}
			}
		}

		/// <summary>
		/// Translate window relative pixel/mouse coordinates to world grid positions.
		/// </summary>
		/// <returns>An array in the form (grid X, grid Y).</returns>
		/// <param name="mouseX">Mouse x.</param>
		/// <param name="mouseY">Mouse y.</param>
		public int[] MouseToWorld(int mouseX, int mouseY) {
			var renderer = World.GetSystem<Renderer>();
			var activeCamera = renderer.ActiveCamera;
			if(activeCamera != null) {
				var cellSize = gridManager.CellSize * renderer.WorldScale.X;
				var lookAt = World.GetComponent<GridObject>(activeCamera.LookAt);
				var lookAtOutput = World.GetComponent<Object2D>(activeCamera.LookAt);
				var cX = (int)Services.Window.Size.X / 2;
				var cY = (int)Services.Window.Size.Y / 2;
				mouseX = (int)Math.Floor((mouseX - cX + cellSize / 2) / cellSize);
				mouseY = (int)Math.Floor((mouseY - cY + cellSize / 2) / cellSize);
				return new int[] {
					lookAt.X + mouseX,
					lookAt.Y + mouseY
				};
			}
			else {
				return new int[] { 0, 0 };
			}
		}

		public void Play(string sound, int entity = World.INVALID_ENTITY) {
			var coeff = 1f;
			if(entity != World.INVALID_ENTITY) {
				var MinDistance = 3f;
				var Attenuation = 10f;
				var Distance = GetDistance(Player, entity);
				coeff = MinDistance / (MinDistance + Attenuation * (Math.Max(Distance, MinDistance) - MinDistance));
				Services.Logger.Debug("GameMechanics.Play", entity + " at coeff " + coeff + " with distance " + Distance);
			}
			Services.Audio.Play(sound, coeff);
		}

		public Portrait PlayerPortrait { get { return playerPortraits[0]; } }

		/// <summary>
		/// Sets the animation of an entity. The entity must have an
		/// associated Object2D
		/// </summary>
		/// <param name="entity">Entity.</param>
		/// <param name="animation">Animation ID.</param>
		public void SetAnimation(int entity, string animation) {
			var output = World.GetComponent<Object2D>(entity);
			if(output != null) {
				//Services.Logger.Debug("GameMechanics.SetAnimation", entity + " -> " + animation);
				output.Animation = animation;
			}
		}

		/// <summary>
		/// Sets the facing of an Object2D, respective to dx.
		/// </summary>
		/// <param name="entity">Entity.</param>
		/// <param name="dx">Delta X, -1 = left facing, 1 = right facing.</param>
		public void SetFacing(int entity, int dx) {
			var output = World.GetComponent<Object2D>(entity);
			if(output != null && dx < 0) {
				output.Mirror = true;
			}
			else if(output != null && dx > 0) {
				output.Mirror = false;
			}
		}

		/// <summary>
		/// Sets the idle animation of an entity. The entity must have an
		/// associated Object2D.
		/// </summary>
		/// <param name="entity">Entity.</param>
		/// <param name="animation">Animation ID.</param>
		public void SetIdleAnimation(int entity, string animation) {
			var output = World.GetComponent<Object2D>(entity);
			if(output != null) {
				//Services.Logger.Debug("GameMechanics.SetAnimation", entity + " -> " + animation);
				output.IdleAnimation = animation;
			}
		}

		/// <summary>
		/// Show or hide the specified entity and show by applying an Alpha tween.
		/// </summary>
		/// <param name="entity">Entity.</param>
		/// <param name="show">If set to <c>true</c> show.</param>
		public void Show(int entity, bool show = true) {
			var output = World.GetComponent<ShowHideBehaviour>(entity);
			if(output == null || output.DeleteMe == true) {
				World.Add(new ShowHideBehaviour(entity, show));
			}
			else {
				output.Show = show;
			}
		}

		public void ShowDialogue(string content) {
			var db = new DialogueBehavior(World.GenerateNewEntityId());
			db.Icon = PlayerPortrait.Icon;
			db.Text = content;
			World.Add(db);
		}


		public void ShowGui() {
			foreach(var portrait in playerPortraits) {
				World.Delete(portrait.Icon);
				World.Delete(portrait.Progress);
			}
			playerPortraits.Clear();
			var x = 16f;
			var y = 16f;
			var dY = 80f;
			var guiId = World.GenerateNewEntityId();
			var playerPortrait = buildPortrait(guiId, "portrait_exekiel", "Exekiel", x, y, new Color(255, 0, 0, 127));
			playerPortraits.Add(playerPortrait);
			var playerTmp = World.GetComponent<TurnActor>(Player);
			foreach(var skill in playerTmp.Skills) {
				//skill.RoundsToGo = skill.CoolDown + 1;
				if(skill.Show == true) {
					y += dY;
					var skillPortrait = buildPortrait(guiId, skill.Template, skill.Id, x, y, new Color(0, 0, 0, 127));
					skillPortrait.Progress.Max = skill.CoolDown;
					playerPortraits.Add(skillPortrait);
				}
			}
			SelectedGuiSkill = 1;
		}

		/// <summary>
		/// Shows or Hides the tactical grid.
		/// </summary>
		public void ToggleGrid() {
			var map = World.GetComponents<Map>()[0] as Map;
			map.DrawGrid = (map.DrawGrid == true) ? false : true;
		}

		public void UpdateGui() {
			var playerTmp = World.GetComponent<TurnActor>(Player);
			Services.Logger.Debug("GameMechanics.UpdateGui", "Updating");
			foreach(var skill in playerTmp.Skills) {
				var portrait = playerPortraits.Find((p) => p.Name == skill.Id);
				if(portrait != null) {
					Services.Logger.Debug("GameMechanics.UpdateGui", "Updating skill " + skill.Id + " to " + skill.RoundsToGo + "/" + skill.CoolDown);
					portrait.Progress.Current = skill.RoundsToGo;
				}
			}
		}

		/// <summary>
		/// Translates world coordinates to pixel coordinates. The returned position
		/// is always the center of the cell.
		/// </summary>
		/// <returns>An array in the form (center X, center Y).</returns>
		/// <param name="gridX">Grid x.</param>
		/// <param name="gridY">Grid y.</param>
		public int[] WorldToPixel(int gridX, int gridY) {
			var cellSize = gridManager.CellSize;
			var map = World.GetComponents<Map>()[0] as Map;
			return new int[] {
				(int)(gridX * cellSize - cellSize/2 - map.Position.X),
				(int)(gridY * cellSize - cellSize/2 - map.Position.Y)
			};
		}
	}
}
