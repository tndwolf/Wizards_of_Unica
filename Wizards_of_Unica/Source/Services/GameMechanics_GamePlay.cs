using System;
using System.Collections.Generic;
using SFML.Graphics;

namespace tndwolf.ECS {
	public partial class GameMechanics {
		public const string DAMAGE_TYPE_FIRE = "FIRE";
		public const string DAMAGE_TYPE_PHYSICAL = "PHYSICAL";
		public const string DAMAGE_TYPE_UNTYPED = "UNTYPED";
		public const int ROUND_LENGTH = 10;

		/// <summary>
		/// The attacker entity performs a basic (physical) attack on defender.
		/// Returns the amount of damage suffered by the defender.
		/// </summary>
		/// <param name="attacker">Attacker.</param>
		/// <param name="defender">Defender.</param>
		public int Attack(int attacker, int defender) {
			//Services.Logger.Debug("GameMechanics.Attack", attacker + " vs " + defender);
			Action action = delegate () {
				var aPos = GetPosition(attacker);
				var dPos = GetPosition(defender);
				var dx = Math.Sign(dPos[0] - aPos[0]);
				var dy = Math.Sign(dPos[1] - aPos[1]);
				SetFacing(attacker, dx);
				SetAnimation(attacker, "ATTACK");
				//Services.Logger.Debug("GameMechanics.Attack", attacker + " is attacking " + defender);
				Damage(attacker, defender, 1, DAMAGE_TYPE_PHYSICAL);
				//Push(defender, dx, dy);
			};
			turnManager.DoAndWait(action);
			return 1;
		}

		public int Damage(int actor, int target, int howMuch, string damageType) {
			var res = howMuch;
			var taBuff = World.GetComponent<TurnActor>(target);
			if(taBuff != null) {
				Services.Logger.Debug("GameMechanics.Damage", actor + " -> " + target + " by " + howMuch);
				taBuff.Health -= howMuch;
				Services.Logger.Debug("GameMechanics.Damage", "Final health " + taBuff.Health + "/" + taBuff.MaxHealth);
				if(taBuff.Health <= 0) {
					Kill(target);
				}
				if(target == Player) {
					PlayerPortrait.Progress.Current = (float)(taBuff.MaxHealth - taBuff.Health) / (float)taBuff.MaxHealth * 100f;
				}
			}
			else {
				Services.Logger.Debug("GameMechanics.Damage", target + " not found");
			}
			return res;
		}

		public int DelayDamage(int millis, int actor, int target, int howMuch, string damageType) {
			Action action = delegate () {
				var res = howMuch;
				var taBuff = World.GetComponent<TurnActor>(target);
				if(taBuff != null) {
					Services.Logger.Debug("GameMechanics.Damage", actor + " -> " + target + " by " + howMuch);
					taBuff.Health -= howMuch;
					Services.Logger.Debug("GameMechanics.Damage", "Final health " + taBuff.Health + "/" + taBuff.MaxHealth);
					if(taBuff.Health <= 0) {
						Kill(target);
					}
					if(target == Player) {
						PlayerPortrait.Progress.Current = (float)(taBuff.MaxHealth - taBuff.Health) / (float)taBuff.MaxHealth * 100f;
					}
				}
				else {
					Services.Logger.Debug("GameMechanics.Damage", target + " not found");
				}
			};
			turnManager.WaitAndDo(action, millis);
			return 1;
		}


		/// <summary>
		/// Returns true if the entity is moving or performing a non-blocking animation.
		/// </summary>
		/// <returns><c>true</c>, if acting, <c>false</c> otherwise.</returns>
		/// <param name="entity">Entity.</param>
		public bool IsActing(int entity) {
			var goBuff = World.GetComponent<GridObject>(entity);
			var o2Buff = World.GetComponent<Object2D>(entity);
			return (goBuff != null && goBuff.HasToMove) || (o2Buff != null && o2Buff.IsAnimationBlocking);
		}

		public bool IsEnemy(int actor, int target) {
			var aBuff = World.GetComponent<TurnActor>(actor);
			var tBuff = World.GetComponent<TurnActor>(target);
			if(aBuff != null && tBuff != null) {
				return aBuff.Faction != tBuff.Faction;
			}
			else {
				return false;
			}
		}

		/// <summary>
		/// Returns true if the grid is walkable.
		/// </summary>
		/// <returns><c>true</c>, if walkable, <c>false</c> otherwise.</returns>
		/// <param name="x">The x coordinate.</param>
		/// <param name="y">The y coordinate.</param>
		public bool IsWalkable(int x, int y) {
			return gridManager.IsWalkable(x, y);
		}

		/// <summary>
		/// Kills the specified entity. This is an "in-game" kill and
		/// thus will execute all the proper activities like running the
		/// onDeath script of an actor.
		/// </summary>
		/// <param name="entity">Entity.</param>
		public void Kill(int entity) {
			var taBuff = World.GetComponent<TurnActor>(entity);
			if(taBuff != null) {
				Execute(taBuff.OnDeath);
			}
			if(entity != Player) {
				//World.Delete(entity);
				//Services.Logger.Error("GameMechanics.Kill", "Spawning DeathBehaviour");
				World.Add(new DeathBehavior(entity));
			}
		}

		/// <summary>
		/// Tries to move the specified entity according to the movement rules.
		/// </summary>
		/// <param name="entity">Entity.</param>
		/// <param name="dx">Dx.</param>
		/// <param name="dy">Dy.</param>
		public bool Move(int entity, int dx, int dy) {
			Action action;
			if(entity == Player) {
				action = delegate () {
					gridManager.Move(entity, dx, dy);
					var center = GetPosition(entity);
					gridManager.CalculateFoV(center[0], center[1], GridManager.FOV_UPDATE_RADIUS - dx);
				};
			}
			else {
				action = delegate () {
					gridManager.Move(entity, dx, dy);
				};
			}
			turnManager.DoAndWait(action);
			return true;
		}

		/// <summary>
		/// Pushes the specified entity according to the push rules.
		/// </summary>
		/// <param name="entity">Entity.</param>
		/// <param name="dx">Dx.</param>
		/// <param name="dy">Dy.</param>
		public bool Push(int entity, int dx, int dy) {
			Action action = delegate () {
				gridManager.Move(entity, dx, dy, true);
			};
			turnManager.DoAndWait(action);
			return true;
		}

		public void RunDefaultAI(int entity) {
			//Action action = delegate () {
			var delta = GetRelativePosition(entity, Player);
			Normalize(ref delta);
			gridManager.Move(entity, delta[0], delta[1]);
			//};
			//turnManager.DoAndWait(action);
		}

		public int SelectedGuiSkill {
			get { return 0; }
			set {
				var playerTmp = World.GetComponent<TurnActor>(Player);
				var selectedCount = 0;
				foreach(var portrait in playerPortraits) {
					portrait.Progress.BorderColor = UNSELECTED_BORDER_COLOR;
				}
				if(value < playerPortraits.Count) {
					Services.Logger.Debug("GameMechanics.SelectedSkill", value);
					if(playerPortraits[value].Progress.BorderColor.Equals(UNSELECTED_BORDER_COLOR)) {
						var skill = playerTmp.GetSkill(playerPortraits[value].Name);
						if(skill.RoundsToGo < 1) {
							Services.Logger.Debug("GameMechanics.SelectedSkill", "Selected " + skill.Name);
							playerPortraits[value].Progress.BorderColor = SELECTED_BORDER_COLOR;
							selectedCount++;
						}
					}
					else {
						Services.Logger.Debug("GameMechanics.SelectedSkill", "Unselected");
						playerPortraits[value].Progress.BorderColor = UNSELECTED_BORDER_COLOR;
					}
				}
				if(selectedCount == 0) {
					try {
						playerPortraits[1].Progress.BorderColor = SELECTED_BORDER_COLOR;
					}
					finally { }
				}
			}
		}

		public Skill SelectedSkill {
			get {
				var playerTmp = World.GetComponent<TurnActor>(Player);
				var skillNames = new List<string>();
				foreach(var portrait in playerPortraits) {
					if(portrait.Progress.BorderColor.Equals(SELECTED_BORDER_COLOR))
						skillNames.Add(portrait.Name);
				}
				return playerTmp.GetSkill(skillNames);
			}
		}

		public bool TrySkill(string skillId, int actor = World.INVALID_ENTITY, int target = World.INVALID_ENTITY, int targetX = -1, int targetY = -1) {
			var taBuff = World.GetComponent<TurnActor>(actor);
			if(taBuff != null) {
				var skill = taBuff.GetSkill(skillId);
				if(skill != null) {
					return TrySkill(skill, actor, target, targetX, targetY);
				}
			}
			return false;
		}

		public bool TrySkill(Skill skill, int actor = World.INVALID_ENTITY, int target = World.INVALID_ENTITY, int targetX = -1, int targetY = -1) {
			if(actor != World.INVALID_ENTITY) Actor = actor;
			if(targetX >= 0 && targetY >= 0) {
				TargetX = targetX;
				TargetY = targetY;
			}
			if(target != World.INVALID_ENTITY) {
				Target = target;
			}
			else {
				var tmp = GetAllAt(targetX, targetY);
				Target = (tmp.Length != 0) ? tmp[0] : World.INVALID_ENTITY;
			}
			var taBuff = World.GetComponent<TurnActor>(actor);
			if(taBuff != null && skill != null) {
				var res = Execute(skill.OnUsed);
				try {
					if((bool)res == true) {
						skill.RoundsToGo = skill.CoolDown+1;
						return true;
					}
				}
				catch {
					return false;
				}
			}
			return false;
		}
	}
}
