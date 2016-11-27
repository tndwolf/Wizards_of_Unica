using System;
using System.Collections.Generic;

namespace tndwolf.ECS {
	#region RealTimeActions
	/// <summary>
	/// Interface for Actions that are performed in real time by the turn manager
	/// during the same turn
	/// </summary>
	public interface RealTimeAction {
		int Run(int deltaTimeMillis);
		int TTL { get; set; }
	}

	/// <summary>
	/// Wait and perform Action. If wait time is 0 then the Action is
	/// immediately performed
	/// </summary>
	public class WaitAndPerformAction: RealTimeAction {
		protected Action action;

		public WaitAndPerformAction(int waitMillis, Action action) {
			this.action = action;
			TTL = waitMillis;
		}

		public int Run(int deltaTimeMillis) {
			TTL -= deltaTimeMillis;
			if(TTL <= 0) {
				action.Invoke();
			}
			return TTL;
		}

		public int TTL { get; set; }
	}

	/// <summary>
	/// Wait for a few milliseconds before moving to next action.
	/// </summary>
	public class WaitAction: RealTimeAction {
		public WaitAction(int waitMillis) {
			TTL = waitMillis;
		}

		public int Run(int deltaTimeMillis) {
			TTL -= deltaTimeMillis;
			return TTL;
		}

		public int TTL { get; set; }
	}
	#endregion

	public class TurnManager: GameSystem {
		public enum PlayerAction {
			SKILL,
			MOVE,
			UNDEFINED
		}

		List<TurnActor> actors = new List<TurnActor>();
		TurnActor currentActor = null;
		List<RealTimeAction> executionQueue = new List<RealTimeAction>();
		bool flagPlayerUpdate = true;
		int initiativeCount = 0;
		PlayerAction nextPlayerAction = PlayerAction.UNDEFINED;
		int nextMoveX = 0;
		int nextMoveY = 0;

		/// <summary>
		/// Performs an action then waits a number of milliseconds.
		/// Note that all actions are appended to the queue, this mean that the
		/// manager will perform the other actions in the queue before executing
		/// and waiting
		/// </summary>
		/// <param name="action">Action.</param>
		/// <param name="waitMillis">Wait millis.</param>
		public void DoAndWait(Action action, int waitMillis = 0) {
			executionQueue.Add(new WaitAndPerformAction(0, action));
			executionQueue.Add(new WaitAction(waitMillis));
		}

		public void SetNextPlayerAction(PlayerAction action, int moveX, int moveY) {
			nextPlayerAction = action;
			nextMoveX = moveX;
			nextMoveY = moveY;
		}

		override public void Update(World world) {
			//Services.Logger.Debug("TurnManager.Update", "Checkpoint 1");
			if(UpdateRealTimeQueue(world) == false) {
				return;
			}
			var runNext = actors.Count;
			//Services.Logger.Debug("TurnManager.Update", "Checkpoint 2");
			while(runNext > 0 && executionQueue.Count == 0) {
				if(currentActor != null && !Services.GameMechanics.IsActing(currentActor.Entity)) {
					//Services.Logger.Debug("TurnManager.Update", "Checkpoint 2.1");
					// HACK in general UpdateEntity also increase the cooldown of skills
					// for players also, but only once, so during player "thinking" the
					// flag is set to false
					flagPlayerUpdate = UpdateEntity(currentActor, world);
					runNext -= (flagPlayerUpdate == true) ? 2 : 1;
					//Services.Logger.Debug("TurnManager.Update", "Ran: " + currentActor.Entity + " runNext: " + runNext);
				}
				else {
					runNext--;
				}
				// Note that the lower entity ID was created first, so for the same
				// level of initiative it should go first, always true
				actors.Sort((a1, a2) => {
					var res = a1.Initiative.CompareTo(a2.Initiative);
					return (res == 0) ? a1.Entity.CompareTo(a2.Entity) : res;
				});
				if(actors.Count > 0) {
					//Services.Logger.Debug("TurnManager.Update", "Checkpoint 2.2");
					currentActor = actors[0];
					initiativeCount = currentActor.Initiative;
				}
				//Services.Logger.Debug("TurnManager.Update", "Checkpoint 3");
			}
			//Services.Logger.Debug("TurnManager.Update", "Checkpoint 4");
		}

		/// <summary>
		/// Updates the entity.
		/// </summary>
		/// <returns><c>true</c>, if entity was updated, <c>false</c> otherwise.</returns>
		/// <param name="currentActor">Current actor.</param>
		/// <param name="world">World.</param>
		protected bool UpdateEntity(TurnActor currentActor, World world) {
			var res = false;
			Services.GameMechanics.Actor = currentActor.Entity;
			Services.GameMechanics.Target = World.INVALID_ENTITY;
			Services.GameMechanics.TargetX = World.INVALID_ENTITY;
			Services.GameMechanics.TargetY = World.INVALID_ENTITY;
			if(flagPlayerUpdate) {
				foreach(var skill in currentActor.Skills) {
					skill.RoundsToGo--;
				}
			}
			if(currentActor.IsPlayer) {
				if (flagPlayerUpdate) Services.GameMechanics.UpdateGui(); // HACK bad to do it here...
				flagPlayerUpdate = false;
				switch(nextPlayerAction) {
					case PlayerAction.MOVE:
						var gridManager = world.GetSystem<GridManager>();
						var willMove = gridManager.Move(
							currentActor.Entity,
							nextMoveX,
							nextMoveY
						);
						var center = Services.GameMechanics.GetPosition(currentActor.Entity);
						gridManager.CalculateFoV(center[0], center[1], GridManager.FOV_UPDATE_RADIUS - nextMoveX);
						if(willMove) {
							if(Services.GameMechanics.PlayerPortrait.Icon.IsIdle) {
								Services.GameMechanics.PlayerPortrait.Icon.Animation = "MOVE";
							}
							currentActor.Initiative += currentActor.Speed;
							res = true;
						}
						break;
					case PlayerAction.SKILL:
						var skill = Services.GameMechanics.SelectedSkill;
						if(skill != null) {
							Services.Logger.Debug("TurnManager.UpdateEntity", "Casting " + skill.Name);
							res = Services.GameMechanics.TrySkill(skill, currentActor.Entity, World.INVALID_ENTITY, nextMoveX, nextMoveY);
							if(res == true) {
								Services.GameMechanics.SelectedGuiSkill = 1;
								currentActor.Initiative += currentActor.Speed;
							}
						}
						break;
				}
				nextPlayerAction = PlayerAction.UNDEFINED;
			}
			else if(currentActor is LevelActor) {
				currentActor.Update(world);
				currentActor.Initiative += currentActor.Speed;
				res = true;
			}
			else {
				//Services.Logger.Debug("TurnManager.Update", "Doing NPC: " + currentActor.Entity);
				if(currentActor.OnRound != string.Empty) {
					//Services.Logger.Warn("TurnManager.Update", "Executing: " + currentActor.OnRound);
					Services.GameMechanics.Execute(currentActor.OnRound);
				}
				currentActor.Initiative += currentActor.Speed;
				res = true;
			}
			return res;
		}

		/// <summary>
		/// Updates the real time queue.
		/// </summary>
		/// <returns><c>true</c>, if real time queue is empty, <c>false</c> otherwise.</returns>
		/// <param name="world">World.</param>
		protected bool UpdateRealTimeQueue(World world) {
			if(executionQueue.Count > 0) {
				//Services.Logger.Debug("TurnManager.UpdateRealTimeQueue", "Something to process");
				if(executionQueue[0].Run(world.DeltaTime) <= 1) {
					// Finished one action, try the next if it can be done immediately
					// otherwise it will start but return without going forward
					executionQueue.RemoveAt(0);
					UpdateRealTimeQueue(world);
				}
			}
			return executionQueue.Count == 0;
		}

		/// <summary>
		/// Waits a number of milliseconds, then performs an action.
		/// Note that all actions are appended to the queue, this mean that the
		/// manager will perform the other actions in the queue before executing
		/// </summary>
		/// <param name="action">Action.</param>
		/// <param name="waitMillis">Wait millis.</param>
		public void WaitAndDo(Action action, int waitMillis = 0) {
			executionQueue.Add(new WaitAndPerformAction(waitMillis, action));
		}

		/// <summary>
		/// Wait the specified number of milliseconds before executing the next
		/// action in the queue.
		/// Note that all actions are appended to the queue, this mean that the
		/// manager will perform the other actions in the queue before waiting
		/// </summary>
		/// <param name="waitMillis">Wait millis.</param>
		public void Wait(int waitMillis) {
			executionQueue.Add(new WaitAction(waitMillis));
		}

		#region GameSystem
		override public void Initialize(World world) {
			actors = new List<TurnActor>();
			currentActor = null;
			executionQueue = new List<RealTimeAction>();
			initiativeCount = 0;
		}

		override public int Diagnose() {
			Services.Logger.Debug("TurnManager.Diagnose", "----- Total components: " + actors.Count);
			Services.Logger.Debug("TurnManager.Diagnose", "----- Total executions: " + executionQueue.Count);
			return 0;
		}

		override public bool Register(GameComponent component) {
			if(component is TurnActor) {
				var actor = component as TurnActor;
				actor.Initiative += initiativeCount++;
				actors.Add(actor);
				actors.Sort((a1, a2) => a1.Initiative.CompareTo(a2.Initiative));
				currentActor = actors[0];
				initiativeCount = currentActor.Initiative;
				return true;
			}
			return false;
		}

		override public void UnRegister(GameComponent component) {
			var refComponent = component as TurnActor;
			actors.Remove(refComponent);
			if(currentActor == refComponent) {
				currentActor = null;
			}
		}
		#endregion
	}
}

