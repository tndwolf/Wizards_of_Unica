using System;
using System.Collections.Generic;
using Jint;
using SFML.Graphics;
using SFML.Window;

namespace tndwolf.ECS {
	public partial class GameMechanics {
		Engine js;
		GridManager gridManager;
		TurnManager turnManager;
		public World World;

		/// <summary>
		/// Add the specified component to the current world.
		/// </summary>
		/// <param name="component">Component.</param>
		public void Add(GameComponent component) {
			World.Add(component);
		}

		/// <summary>
		/// Creates an entity from the specified template at position (x, y).
		/// </summary>
		/// <param name="template">Template ID.</param>
		/// <param name="x">The x coordinate.</param>
		/// <param name="y">The y coordinate.</param>
		public int Create(string template, int x, int y) {
			try {
				Services.Logger.Debug("GameMechanics.Create", "Creating " + template + " at " + x + "," + y);
				var res = World.GenerateNewEntityId();
				Services.GameFactory.BuildFromTemplate(World, template, res);
				gridManager.SetPosition(res, x, y);
				var taBuff = World.GetComponent<TurnActor>(res);
				if(taBuff != null) {
					Execute(taBuff.OnCreate);
				}
				return res;
			}
			catch {
				return -1;
			}
		}

		/// <summary>
		/// Creates a trigger at the defined position and with a specific size.
		/// </summary>
		/// <returns>The trigger.</returns>
		/// <param name="script">The script to be executed on triggered.</param>
		/// <param name="x">The x coordinate.</param>
		/// <param name="y">The y coordinate.</param>
		/// <param name="width">Width.</param>
		/// <param name="height">Height.</param>
		public int CreateTrigger(string script, int x, int y, int width, int height) {
			try {
				//Services.Logger.Debug("GameMechanics.CreateTrigger", "Creating at " + x + "," + y);
				var res = World.GenerateNewEntityId();
				var trigger = new GridObject(res);
				trigger.IsTrigger = true;
				trigger.OnEnter = script;
				trigger.Width = width;
				trigger.Height = height;
				trigger.SetPosition(x, y);
				World.Add(trigger);
				var verify = gridManager.Get(x, y);
				//if (verify != null) Services.Logger.Debug("GameMechanics.CreateTrigger", "Created at " + x + "," + y);
				return res;
			}
			catch {
				return -1;
			}
		}

		public void Delay(int initiative, string script) {
			// TODO I should create a new TurnActor and add to the script the
			// Actor, Target, and TargetX/Y to not lose them
			var delayedActor = new TurnActor(World.GenerateNewEntityId());
			delayedActor.Initiative = initiative;
			delayedActor.OnRound = script;
			World.Add(delayedActor);
		}

		public void Delete(int entity) {
			World.Delete(entity);
		}

		public int Diagnose() {
			return World.Diagnose();
		}

		/// <summary>
		/// Execute the specified script.
		/// </summary>
		/// <param name="script">Script.</param>
		public object Execute(string script) {
			//Services.Logger.Debug("GameMechanics.Execute", script);
			try {
				//js.Execute(script);
				js.Execute("(function(){" + script + "}());");
				//Services.Logger.Debug("GameMechanics.Execute", "(function(){" + script + "})();");
				//Services.Logger.Debug("GameMechanics.Execute", ((bool)(js.GetCompletionValue().ToObject()) == true).ToString());
				return js.GetCompletionValue().ToObject();
			}
			catch (Exception ex){
				Services.Logger.Warn("GameMechanics.Execute", ex.ToString());
				return null;
			}
		}

		public void GenerateWorld(string levelFile) {
			var wf = new WorldFactory();
			wf.Initialize(levelFile);
			var map = World.GetComponents<Map>()[0] as Map;
			wf.LoadTilemask(map);
			var res = wf.Generate(map);
		}

		/// <summary>
		/// Gets all at entities at (x, y).
		/// </summary>
		/// <returns>The entities.</returns>
		/// <param name="x">The x coordinate.</param>
		/// <param name="y">The y coordinate.</param>
		public int[] GetAllAt(int x, int y) {
			return gridManager.GetAll(x, y).ConvertAll((c) => c.Entity).ToArray(); 
		}

		/// <summary>
		/// Gets the direction of movement of an entity as a (dx, dy) vector normalized
		/// between (1, -1) where 0 is no direction on an axis.
		/// </summary>
		/// <returns>The direction.</returns>
		/// <param name="entity">Entity.</param>
		public int[] GetDirection(int entity) {
			var goBuff = World.GetComponent<GridObject>(entity);
			if(goBuff != null)
				return new int[] { Math.Sign(goBuff.X - goBuff.OldX), Math.Sign(goBuff.Y - goBuff.OldY) };
			else
				return new int[] { 0, 0 };
		}

		public float GetDistance(int fromEntity, int toEntity) {
			var res = 0f;
			var fromBuff = World.GetComponent<GridObject>(fromEntity);
			var toBuff = World.GetComponent<GridObject>(toEntity);
			if(fromBuff != null && toBuff != null) {
				res = (float)Math.Sqrt(
					(toBuff.X - fromBuff.X) * (toBuff.X - fromBuff.X) +
					(toBuff.Y - fromBuff.Y) * (toBuff.Y - fromBuff.Y)
				);
			}
			return res;
		}

		/// <summary>
		/// Gets the direction of movement of an entity as a (dx, dy) vector.
		/// </summary>
		/// <returns>The movement.</returns>
		/// <param name="entity">Entity.</param>
		public int[] GetMovement(int entity) {
			var goBuff = World.GetComponent<GridObject>(entity);
			if(goBuff != null)
				return new int[] { goBuff.X - goBuff.OldX, goBuff.Y - goBuff.OldY };
			else
				return new int[] { 0, 0 };
		}

		/// <summary>
		/// Gets the position of an entity as a (grid X, grid Y) pair of coordinates.
		/// Returns (-1, -1) if the position is undefined.
		/// </summary>
		/// <returns>The position.</returns>
		/// <param name="entity">Entity.</param>
		public int[] GetPosition(int entity) {
			var goBuff = World.GetComponent<GridObject>(entity);
			if(goBuff != null)
				return new int[] { goBuff.X, goBuff.Y };
			else
				return new int[] { -1, -1 };
		}

		/// <summary>
		/// Gets the position of an entity relative to another on the grid as a (left, top)
		/// pair of coordinates.
		/// Returns (0, 0) if the position is undefined.
		/// </summary>
		/// <returns>The position.</returns>
		/// <param name="fromEntity">Entity.</param>
		/// <param name="toEntity">Entity.</param>
		public int[] GetRelativePosition(int fromEntity, int toEntity) {
			var fromBuff = World.GetComponent<GridObject>(fromEntity);
			var toBuff = World.GetComponent<GridObject>(toEntity);
			if(fromBuff != null && toBuff != null)
				return new int[] { toBuff.X - fromBuff.X, toBuff.Y - fromBuff.Y };
			else
				return new int[] { 0, 0 };
		}

		/// <summary>
		/// Gets a variable from a valid TurnActor.
		/// </summary>
		/// <returns>The variable.</returns>
		/// <param name="entity">Entity.</param>
		/// <param name="name">Name of the variable.</param>
		public int GetVariable(int entity, string name) {
			int res;
			var tmp = World.GetComponent<TurnActor>(entity);
			if(tmp != null && tmp.Variables.TryGetValue(name, out res)) {
				return res;
			}
			else {
				return 0;
			}
		}

		/// <summary>
		/// Initialize the services, it should be used only one at the start of the service.
		/// </summary>
		/// <param name="world">World.</param>
		public void Initialize(World world) {
			World = world;
			gridManager = world.GetSystem<GridManager>();
			turnManager = world.GetSystem<TurnManager>();
			js = new Engine(cfg => cfg.AllowClr(typeof(Services).Assembly));
			js.Execute("var Game = importNamespace('tndwolf.ECS');");
			js.Execute("var Audio = Game.Services.Audio;");
			js.Execute("var Factory = Game.Services.GameFactory;");
			js.Execute("var Inputs = Game.Services.Inputs;");
			js.Execute("var Logger = Game.Services.Logger;");
			js.Execute("var Mechanics = Game.Services.GameMechanics;");
			// Constants
			js.Execute("var INVALID_ENTITY = " + World.INVALID_ENTITY + ";");
			js.Execute("var DAMAGE_TYPE_FIRE = \"" + DAMAGE_TYPE_FIRE + "\";");
			js.Execute("var DAMAGE_TYPE_PHYSICAL = \"" + DAMAGE_TYPE_PHYSICAL + "\";");
			js.Execute("var DAMAGE_TYPE_UNTYPED = \"" + DAMAGE_TYPE_UNTYPED + "\";");
			Actor = -1;
			Player = -1;
			Target = -1;
			TargetX = 0;
			TargetY = 0;
		}

		/// <summary>
		/// Resets the player information to the start of the level.
		/// </summary>
		/// <returns>The player.</returns>
		/// <param name="template">Template.</param>
		public int InitializePlayer(string template) {
			try {
				var res = World.GenerateNewEntityId();
				Services.GameFactory.BuildFromTemplate(World, template, res);
				var startPosition = (World.GetComponents<Map>()[0] as Map).StartCell;
				gridManager.SetPosition(res, startPosition.X, startPosition.Y);
				var taBuff = World.GetComponent<TurnActor>(res);
				if(taBuff != null) {
					Execute(taBuff.OnCreate);
				}
				Player = res;
				return Player;
			}
			catch {
				Services.Logger.Error("GameMechanics.InitializePlayer", "Initialization Error");
				return -1;
			}
		}

		/// <summary>
		/// Normalize the specified vector between 1 and -1.
		/// </summary>
		/// <param name="vector">Vector.</param>
		public void Normalize(ref int[] vector) {
			for(var i = 0; i < vector.Length; i++) {
				vector[i] = Math.Sign(vector[i]);
			}
		}

		/// <summary>
		/// Rolls a number of virtual dice.
		/// </summary>
		/// <param name="howMany">How many dice.</param>
		/// <param name="sides">Sides of each die.</param>
		/// <param name="mod">Modifier if any.</param>
		public int Roll(int howMany, int sides, int mod = 0) {
			var res = howMany + mod;
			for(var i = 0; i < howMany; i++) {
				res += Services.Rng.Next(sides);
			}
			return res;
		}

		/// <summary>
		/// Istantaneously moves an entity to the specified grid position
		/// </summary>
		/// <param name="entity">Entity.</param>
		/// <param name="x">The x coordinate.</param>
		/// <param name="y">The y coordinate.</param>
		public void SetPostition(int entity, int x, int y) {
			gridManager.SetPosition(entity, x, y);
		}

		/// <summary>
		/// Sets a numeric variable for a valid TurnActor.
		/// </summary>
		/// <param name="entity">Entity.</param>
		/// <param name="name">Name of the variable.</param>
		/// <param name="val">Value.</param>
		public void SetVariable(int entity, string name, int val) {
			var tmp = World.GetComponent<TurnActor>(entity);
			if(tmp != null) {
				tmp.Variables[name] = val;
			}
		}

		/// <summary>
		/// Gets the level entry point as a set of (x, y) coordinates.
		/// </summary>
		/// <value>The start position.</value>
		public int[] StartPosition {
			get { 
				var tmp = (World.GetComponents<Map>()[0] as Map).StartCell;
				return new int[] { tmp.X, tmp.Y };
			}
		}

		/// <summary>
		/// Adds a wait command to the real-time queue.
		/// </summary>
		/// <param name="waitMillis">Wait millis.</param>
		public void Wait(int waitMillis) {
			turnManager.Wait(waitMillis);
		}

		#region Actors shorthands
		int currentActor = World.INVALID_ENTITY;
		int currentTarget = World.INVALID_ENTITY;
		int player = -1;
		int targetX = -1;
		int targetY = -1;

		public int Actor {
			get { return currentActor; }
			set {
				currentActor = value;
				js.SetValue("Actor", value);
			}
		}

		public int INVALID_ENTITY {
			get { return World.INVALID_ENTITY; }
		}

		public int Player {
			get { return player; }
			set {
				var playerBuff = World.GetComponent<TurnActor>(value);
				if(playerBuff != null) {
					playerBuff.IsPlayer = true;
					player = value;
					js.SetValue("Player", value);
				}
			}
		}

		public void SetActors(int actor, int target, int targetX, int targetY) {
			Actor = actor;
			Target = target;
			TargetX = targetX;
			TargetY = targetY;
		}

		public int Target {
			get { return currentTarget; }
			set {
				currentTarget = value;
				js.SetValue("Target", value);
			}
		}

		public int TargetX {
			get { return targetX; }
			set {
				targetX = value;
				js.SetValue("TargetX", value);
			}
		}

		public int TargetY {
			get { return targetY; }
			set {
				targetY = value;
				js.SetValue("TargetY", value);
			}
		}
		#endregion

	}
}
