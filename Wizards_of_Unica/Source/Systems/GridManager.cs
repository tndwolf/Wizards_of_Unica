using System;
using System.Collections.Generic;

namespace tndwolf.ECS {
	public class GridManager: GameSystem {
		public const int FOV_UPDATE_RADIUS = 10;

		List<GridObject> actors = new List<GridObject>();
		Map map;
		World world;

		public void CalculateFoV(int cx, int cy, int sightRadius, int updateRadius = FOV_UPDATE_RADIUS) {
			LastSightRadius = sightRadius;
			for(int y = cy - updateRadius; y < cy + updateRadius; y++) {
				for(int x = cx - updateRadius; x < cx + updateRadius; x++) {
					if(IsValid(x, y)) {
						var isVisible = TestInLoS(cx, cy, x, y, sightRadius);
						map.SetVisible(x, y, isVisible);
						var targets = GetAll(x, y);
						if(targets.Count == 0) {
							map.Set(x, y, Map.MASK_CONTAINS_ALLY, false);
							map.Set(x, y, Map.MASK_CONTAINS_ENEMY, false);
						}
						else if(Services.GameMechanics.IsEnemy(Services.GameMechanics.Player, targets[0].Entity)) {
							map.Set(x, y, Map.MASK_CONTAINS_ALLY, false);
							map.Set(x, y, Map.MASK_CONTAINS_ENEMY, true);
							foreach(var target in targets) {
								Services.GameMechanics.Show(target.Entity, isVisible);
							}
						}
						else {
							map.Set(x, y, Map.MASK_CONTAINS_ALLY, true);
							map.Set(x, y, Map.MASK_CONTAINS_ENEMY, false);
							foreach(var target in targets) {
								Services.GameMechanics.Show(target.Entity, isVisible);
							}
						}
					}
				}
			}
			map.Set(cx, cy, Map.MASK_CONTAINS_ENEMY, false);
			map.Set(cx, cy, Map.MASK_CONTAINS_ALLY, true);
		}

		public float CellSize { get; set; }

		protected void DoMove(GridObject obj, int x, int y, bool isPushed, GridObject trigger) {
			var dx = x - obj.X;
			obj.X = x;
			obj.Y = y;
			if(isPushed == false) {
				Services.GameMechanics.SetFacing(obj.Entity, dx);
				Services.GameMechanics.SetAnimation(obj.Entity, "MOVE");
			}
			if(trigger != null) {
				Services.Logger.Debug("GridManager.DoMove", "Triggering " + trigger.Entity);
				Services.GameMechanics.SetActors(trigger.Entity, obj.Entity, x, y);
				Services.GameMechanics.Execute(trigger.OnEnter);
			}
		}

		public GridObject Get(int x, int y) {
			return actors.Find((i) => x >= i.X && x <= i.MaxX && y >= i.Y && y <= i.MaxY);
		}

		public List<GridObject> GetAll(int x, int y) {
			return actors.FindAll((i) => x >= i.X && x <= i.MaxX && y >= i.Y && y <= i.MaxY);
		}

		public int Height { get { return map.Height; } }

		public bool IsEmpty(int x, int y) {
			return map.IsWalkable(x, y) && Get(x, y) == null;
		}

		public bool IsValid(int x, int y) {
			return (x >= 0 && x < map.Width) && (y >= 0 && y < map.Height);
		}

		public bool IsWalkable(int x, int y) {
			return map.IsWalkable(x, y);
		}

		public int LastSightRadius { get; protected set; }

		/// <summary>
		/// Sets the target for a grid object movement. The object will start
		/// moving immediately at next Update. If the object has an Object2D reference
		/// it will be updated accordingly
		/// </summary>
		/// <param name="entity">Entity.</param>
		/// <param name="dx">Delta x.</param>
		/// <param name="dy">Delta y.</param>
		public bool Move(int entity, int dx, int dy, bool isPushed = false) {
			var res = false;
			var obj = actors.Find((x) => x.Entity == entity);
			if(obj != null) {
				res = TryMove(obj, obj.X + dx, obj.Y + dy, isPushed);
				if(res == false) {
					res = TryMove(obj, obj.X, obj.Y + dy, isPushed);
					if(res == false) {
						res = TryMove(obj, obj.X + dx, obj.Y, isPushed);
					}
				}
			}
			return res;
		}

		/// <summary>
		/// Istantaneously sets the entity position to a grid cell
		/// </summary>
		/// <returns>The position.</returns>
		/// <param name="entity">Entity.</param>
		/// <param name="x">The grid x coordinate.</param>
		/// <param name="y">The grid y coordinate.</param>
		public void SetPosition(int entity, int x, int y) {
			//Services.Logger.Debug("GridManager.SetPosition", "Trying to move entity " + entity + " to " + x + "," + y);
			var obj = actors.Find((a) => a.Entity == entity);
			if(obj != null) {
				//Services.Logger.Debug("GridManager.SetPosition", "Done");
				obj.SetPosition(x, y);
			}
			// HACK this thing does not belong here, but it is easier this way
			var output = world.GetComponent<Object2D>(entity);
			if(output != null) {
				output.X = x * CellSize;
				output.Y = y * CellSize;
			}
		}

		protected void StepMove(GridObject obj) {
			var output = world.GetComponent<Object2D>(obj.Entity);
			var signX = Math.Sign(obj.X - obj.OldX);
			var signY = Math.Sign(obj.Y - obj.OldY);
			var endX = obj.X * CellSize;
			var endY = obj.Y * CellSize;
			var deltaX = signX * world.DeltaTime * obj.MoveSpeed;
			var deltaY = signY * world.DeltaTime * obj.MoveSpeed;
			var predictedX = output.X + deltaX;
			var predictedY = output.Y + deltaY;

			if(signX > 0 && predictedX > endX) {
				predictedX = endX;
			}
			else if(signX < 0 && predictedX < endX) {
				predictedX = endX;
			}
			if(signY > 0 && predictedY > endY) {
				predictedY = endY;
			}
			else if(signY < 0 && predictedY < endY) {
				predictedY = endY;
			}

			output.X = predictedX;
			output.Y = predictedY;
			if(predictedX == endX && predictedY == endY) {
				obj.OldX = obj.X;
				obj.OldY = obj.Y;
				output.Animation = output.IdleAnimation;
			}
		}

		protected bool TryMove(GridObject obj, int x, int y, bool isPushed) {
			var target = Get(x, y);
			var res = false;
			if(target != null) {
				Services.Logger.Debug("GridManager.TryMove", "Found target");
				if(target.IsTrigger && IsWalkable(x, y)) {
					Services.Logger.Debug("GridManager.TryMove", "Trigger and Walkable");
					DoMove(obj, x, y, isPushed, target);
					res = true;
				}
				else if(Services.GameMechanics.IsEnemy(obj.Entity, target.Entity)) {
					Services.Logger.Debug("GridManager.TryMove", "Enemy");
					Services.GameMechanics.Attack(obj.Entity, target.Entity);
					res = true;
				}
				else {
					Services.Logger.Debug("GridManager.TryMove", "Unforeseen case");
				}
			}
			else if(IsWalkable(x, y)) {
				Services.Logger.Debug("GridManager.TryMove", "No target and Walkable");
				DoMove(obj, x, y, isPushed, null);
				res = true;
			}
			else {
				Services.Logger.Debug("GridManager.TryMove", "Non walkable, no target");
			}
			return res;
		}

		override public void Update(World world) {
			foreach(var obj in actors) {
				if(obj.HasToMove) {
					StepMove(obj);
				}
			}
		}

		public bool TestInLoS(int x0, int y0, int x1, int y1, int maxRadius = 10) {
			var dx = Math.Abs(x0 - x1);
			var dy = Math.Abs(y0 - y1);
			var sx = (x0 > x1) ? -1 : 1;
			var sy = (y0 > y1) ? -1 : 1;

			var x = x0;
			var y = y0;
			int i = 0;
			if(dx > dy) {
				var err = dx / 2f;
				while(x != x1) {
					// check x,y
					if(map.IsOpaque(x, y)) return false;
					err -= dy;
					if(err < 0) {
						y += sy;
						err += dx;
					}
					x += sx;
					i++;
				}
			}
			else {
				var err = dy / 2f;
				while(y != y1) {
					// check x,y
					if(map.IsOpaque(x, y)) return false;
					err -= dx;
					if(err < 0) {
						x += sx;
						err += dy;
					}
					y += sy;
					i++;
				}
			}
			if(i > maxRadius) {
				return false;
			}
			else {
				return true;
			}
		}

		public int Width { get { return map.Width; } }

		#region GameSystem
		override public int Diagnose() {
			Services.Logger.Debug("GridManager.Diagnose", "----- Total components: " + actors.Count);
			return 0;
		}

		override public void Initialize(World world) {
			CellSize = 32f;
			actors = new List<GridObject>();
			map = null;
			this.world = world;
		}

		override public bool Register(GameComponent component) {
			if(component is GridObject) {
				actors.Add(component as GridObject);
				return true;
			}
			else if(component is Map) {
				map = component as Map;
				return true;
			}
			return false;
		}

		override public void UnRegister(GameComponent component) {
			actors.Remove(component as GridObject);
		}
		#endregion
	}
}

