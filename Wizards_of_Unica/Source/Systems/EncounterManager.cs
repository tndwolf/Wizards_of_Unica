using System;
using System.Collections.Generic;
using SFML.Window;

namespace tndwolf.ECS {
	public class EncounterManager: GameSystem {
		LevelActor level = null;

		protected void GenerateEncounter(World world, int deltaThreat) {
			Services.Logger.Info("EncounterManager.GenerateEncounter", "Threat " + deltaThreat + " spawning");
			// let's find a group of enemies at this threat level
			var encounter = new List<EnemyBlueprint>();
			var encounterLevel = 0;
			var i = 0;
			var MAX_ITER = 5;
			while(encounterLevel < deltaThreat) {
				var e = level.EnemyBlueprints[Services.Rng.Next(level.EnemyBlueprints.Count)];
				if(encounterLevel + e.Threat <= deltaThreat) {
					encounterLevel += e.Threat;
					encounter.Add(e);
					continue;
				}
				if(i++ >= MAX_ITER) {
					Services.Logger.Info("EncounterManager.GenerateEncounter", "Unable to generate correct group");
					return;
				}
			}

			if(encounter.Count > 0) {
				// find a place where to spawn them
				var grid = world.GetSystem<GridManager>();
				var p = Services.GameMechanics.GetPosition(Services.GameMechanics.Player);
				var minX = p[0] - GridManager.FOV_UPDATE_RADIUS;
				var minY = p[1] - GridManager.FOV_UPDATE_RADIUS;
				var maxX = p[0] + GridManager.FOV_UPDATE_RADIUS;
				var maxY = p[1] + GridManager.FOV_UPDATE_RADIUS;
				var sminX = p[0] - grid.LastSightRadius;
				var sminY = p[1] - grid.LastSightRadius;
				var smaxX = p[0] + grid.LastSightRadius;
				var smaxY = p[1] + grid.LastSightRadius;
				var possibleCells = new List<Vector2i>();
				//Logger.Info ("AreaAI", "OnRound", "Spawn center " + new {p[0], p[1]}.ToString());
				for(int y = minY; y < maxY; y++) {
					for(int x = minX; x < maxX; x++) {
						// must spawn outside sight
						// TODO if not specified differently?
						if(x > sminX && x < smaxX && y > sminY && y < smaxY) {
							continue;
						}
						if(x > 3 && y > 3 && x < grid.Width - 2 && y < grid.Height - 2 &&
							grid.IsWalkable(x, y) && grid.Get(x, y) == null) {
							//Logger.Info ("AreaAI", "OnRound", "Adding possible cell " + new {x, y}.ToString());
							possibleCells.Add(new Vector2i(x, y));
						}
					}
				}
				// finally spawn
				foreach(var e in encounter) {
					if(possibleCells.Count > 0) {
						var position = possibleCells[Services.Rng.Next(possibleCells.Count)];
						possibleCells.Remove(position);
						Services.GameMechanics.Create(e.TemplateID, position.X, position.Y);
						level.CurrentThreat += e.Threat;
						//Logger.Info ("AreaAI", "OnRound", "Created object " + e.TemplateID + " at " + position.ToString());
					}
				}
			}
		}

		override public void Update(World world) {
			if(level != null && level.AlwaysIncreaseThreat) {
					level.Threat += level.Progression[level.ProgressionIndex];
					level.Threat = Math.Min(level.Threat, level.MaxThreat);
					level.ProgressionIndex++;
					level.ProgressionIndex %= level.Progression.Count;
					var deltaThreat = level.Threat - level.CurrentThreat;
					if(deltaThreat > 0) {
						GenerateEncounter(world, deltaThreat);
					}
			}
		}

		#region GameSystem basics
		override public int Diagnose() {
			Services.Logger.Debug("EncounterManager.Diagnose", "----- LevelActor: " + level);
			return 0;
		}

		override public void Initialize(World world) {
			try {
				level = world.GetComponents<LevelActor>()[0] as LevelActor;
			}
			catch {
				level = null;
			}
		}

		override public bool Register(GameComponent component) {
			if(component is LevelActor) {
				level = component as LevelActor;
				return true;
			}
			else {
				return false;
			}
		}

		override public void UnRegister(GameComponent component) {
			if(level == component) level = null;
		}
		#endregion
	}
}
