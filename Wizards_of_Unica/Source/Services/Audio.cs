using System.Collections.Generic;
using SFML.Audio;

namespace tndwolf.ECS {
	/// <summary>
	/// Inferface for all the audio services.
	/// Audio are used to manage sounds
	/// </summary>
	public interface Audio {
		string AssetsFolder { get; set; }
		SoundBuffer Load(string soundFile);
		void Play(string soundFile, float volumneCoeff = 1.0f);
		float Volume { get; set; }
	}

	/// <summary>
	/// Standard implementation of the Audio services
	/// </summary>
	public class DefaultAudio: Audio {
		Dictionary<string, SoundBuffer> sounds = new Dictionary<string, SoundBuffer>();
		public string AssetsFolder { get; set; }

		public DefaultAudio() {
			AssetsFolder = "Assets";
			BackgroundMusic = null;
			Volume = 10f;
		}

		public BackgroundMusic BackgroundMusic { get; set; }

		/// <summary>
		/// Preloads a specific sound effect file.
		/// </summary>
		/// <param name="file">File.</param>
		public SoundBuffer Load(string file) {
			try {
				var sound = new SoundBuffer(AssetsFolder + "/" + file);
				sounds[file] = sound;
				return sound;
			}
			catch {
				return null;
			}
		}

		/// <summary>
		/// Loads the background music.
		/// </summary>
		/// <returns>The background music or null if unsuccessfull.</returns>
		/// <param name="file">File.</param>
		public BackgroundMusic LoadBackgroundMusic(string file) {
			var sound = Load(file);
			if(sound != null) {
				BackgroundMusic = new BackgroundMusic(new Sound(sound), file);
			}
			return BackgroundMusic;
		}

		/// <summary>
		/// Play the specified sound.
		/// </summary>
		/// <param name="soundFile">Sound file.</param>
		public void Play(string soundFile, float volumneCoeff = 1.0f) {
			SoundBuffer sound;
			if(!sounds.TryGetValue(soundFile, out sound)) {
				sound = Load(soundFile);
			}
			var snd = new Sound(sound);
			snd.Volume = Volume * volumneCoeff;
			snd.Play();
		}

		public float Volume { get; set; }
	}
}

