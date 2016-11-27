// Wizard's Duel, a procedural tactical RPG
// Copyright (C) 2014  Luca Carbone
//
// This program is free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.
//
// This program is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
// GNU General Public License for more details.
//
// You should have received a copy of the GNU General Public License
// along with this program.  If not, see <http://www.gnu.org/licenses/>.

using System;
using System.Collections.Generic;
using SFML.Audio;

namespace tndwolf.ECS {
	public class BackgroundMusic {
		protected struct Loop {
			public Loop(TimeSpan start, TimeSpan end) {
				this.start = start;
				this.end = end;
			}

			public TimeSpan start;
			public TimeSpan end;
		}

		public const string MAIN_LOOP = "MAIN_LOOP";

		string fileName;
		Dictionary<string, Loop> loops = new Dictionary<string, Loop>();
		Sound music;
		string nextLoop;
		long endLoopTime = 0;
		long timeRef = 0;

		public BackgroundMusic(Sound music, string fileName) {
			this.music = music;
			this.fileName = fileName;
			music.Loop = false;
			AddLoop(MAIN_LOOP, 0, 100);//(int)music.Duration.TotalMilliseconds);
			nextLoop = MAIN_LOOP;
		}

		public void AddLoop(string name, int start, int end) {
			var loop = new Loop(TimeSpan.FromMilliseconds(start), TimeSpan.FromMilliseconds(end - start));
			loops[name] = loop;
		}

		public string FileName {
			get { return fileName; }
		}

		public void Play(/*long refTime*/) {
			/// TODO
			/*var loop = loops [nextLoop];
			music.PlayingOffset = loop.start;
			nextLoopTime = (long)loop.end.TotalMilliseconds;
			music.Play ();
			Logger.Info ("BackgroundMusic", "Play","Playing loop " + nextLoop);*/
			//startLoopTime = refTime;
		}

		public void Play(string name, long refTime) {
			Loop next;
			if(loops.TryGetValue(name, out next)) {
				nextLoop = name;
				Services.Logger.Debug("BackgroundMusic.Play", "Next ms " + next.end.TotalMilliseconds);
				endLoopTime = refTime + (long)next.end.TotalMilliseconds;
				Services.Logger.Debug("BackgroundMusic.Play", "Set next change in " + endLoopTime);
				music.PlayingOffset = next.start;
				music.Play();
				Services.Logger.Debug("BackgroundMusic.Play", "Playing loop " + nextLoop + " from " + next.start);
			}
			else {
				Services.Logger.Warn("BackgroundMusic.Play", "No loop found named " + name);
			}
		}

		public void SetNextLoop(string name) {
			Loop next;
			if(loops.TryGetValue(name, out next)) {
				nextLoop = name;
				Services.Logger.Debug("BackgroundMusic.SetNextLoop", "Set next change in " + endLoopTime);
			}
		}

		public void Update(long refTime) {
			var delta = refTime - endLoopTime;
			Services.Logger.Debug("BackgroundMusic.Update", refTime + " vs " + endLoopTime);
			if(delta >= 0) {
				var loop = loops[nextLoop];
				music.PlayingOffset = loop.start;
				timeRef = delta;
				endLoopTime = refTime + (long)loop.end.TotalMilliseconds;
				Services.Logger.Debug("BackgroundMusic.Update", "Playing loop " + nextLoop + " from " + loop.start);
				Services.Logger.Debug("BackgroundMusic.Update", "Tstart " + timeRef + " Tend " + endLoopTime);
			}
		}

		/// <summary>
		/// Gets or sets the music volume, from 0 (mute) to 100 (full).
		/// </summary>
		/// <value>The volume.</value>
		public float Volume {
			get { return music.Volume; }
			set { music.Volume = value; }
		}
	}
}
