namespace tndwolf.ECS {
	class AdaptiveAudioSystem: GameSystem {
		LevelActor actor = null;

		override public int Diagnose() {
			Services.Logger.Debug("AdaptiveAudioSystem.Diagnose", "----- LevelActor: " + actor);
			return 0;
		}

		override public void Initialize(World world) {
			try {
				actor = world.GetComponents<LevelActor>()[0] as LevelActor;
			}
			catch {
				actor = null;
			}
		}

		override public bool Register(GameComponent component) {
			if(component is LevelActor) {
				actor = component as LevelActor;
				return true;
			}
			return false;
		}

		override public void UnRegister(GameComponent component) {
			if(actor == component) actor = null;
		}

		override public void Update(World world) {
			if(actor != null) {
				//* TODO
				actor.VisibleThreat = 0;
				/*foreach(var e in Simulator.Instance.world.entities.Values) {
					actor.VisibleThreatLevel += (e.Visible == true && e.Dressing == false) ? e.Threat : 0;
				}*/
				foreach(var loop in actor.MusicLoops) {
					//Logger.Debug AreaAI "UpdateMusic", "threath level " + this.VisibleThreatLevel.ToString() + " vs music level " + loop.MaxThreat.ToString());
					if(actor.VisibleThreat <= loop.MaxThreat) {
						var audio = Services.Audio as DefaultAudio;
						//audio.BackgroundMusic.SetNextLoop(loop.ID);
						return;
					}
				}//*/
			}
		}
	}
}
