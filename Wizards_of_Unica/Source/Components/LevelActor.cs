using System;
using System.Collections.Generic;
using System.Xml.Serialization;
using SFML.Window;

namespace tndwolf.ECS {
	[Serializable]
	public class EnemyBlueprint {
		public EnemyBlueprint() {
			TemplateID = "";
			Threat = 1;
		}

		[XmlAttribute]
		public string TemplateID;

		[XmlAttribute]
		public int Threat;
	}

	[Serializable]
	public class MusicLoop {
		[XmlAttribute("Name")]
		public string ID;

		[XmlAttribute]
		public int MaxThreat;
	}

	[Serializable]
	public class LevelActor: TurnActor {
		public LevelActor() : base(-1) {
			IsManaged = true;
			Progression = new List<int> { 1 };
		}

		public LevelActor(int entity) : base(entity) {
			IsManaged = true;
			Progression = new List<int> { 1 };
		}

		protected void GenerateEncounter(World world, int deltaThreat) {
			Services.Logger.Info("EncounterManager.GenerateEncounter", "Threat " + deltaThreat + " spawning");
			// let's find a group of enemies at this threat level
			var encounter = new List<EnemyBlueprint>();
			var encounterLevel = 0;
			var i = 0;
			var MAX_ITER = 5;
			while(encounterLevel < deltaThreat) {
				var e = EnemyBlueprints[Services.Rng.Next(EnemyBlueprints.Count)];
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
						CurrentThreat += e.Threat;
						Services.Logger.Info ("LevelActor.GenerateEncounter", "Created object " + e.TemplateID + " at " + position);
					}
				}
			}
		}

		override public void Update(World world) {
			if(AlwaysIncreaseThreat) {
				Threat += Progression[ProgressionIndex];
				Threat = Math.Min(Threat, MaxThreat);
				ProgressionIndex++;
				ProgressionIndex %= Progression.Count;
				var deltaThreat = Threat - CurrentThreat;
				if(deltaThreat > 0) {
					Services.Logger.Info("LevelActor.Update", "Generating encounter of level " + deltaThreat);
					GenerateEncounter(world, deltaThreat);
				}
			}
		}

		[XmlArray]
		[XmlArrayItem]
		public List<EnemyBlueprint> EnemyBlueprints = new List<EnemyBlueprint>();

		[XmlArray]
		[XmlArrayItem(Type = typeof(MusicLoop))]
		public List<MusicLoop> MusicLoops = new List<MusicLoop>();

		[XmlAttribute]
		public bool AlwaysIncreaseThreat { get; set; }

		[XmlAttribute]
		public int CurrentThreat { get; set; }

		[XmlAttribute]
		public int MaxThreat { get; set; }

		public List<int> Progression { get; set; }

		[XmlAttribute]
		public int ProgressionIndex { get; set; }

		[XmlAttribute]
		public int Threat { get; set; }

		[XmlAttribute]
		public int VisibleThreat { get; set; }
	}
}
