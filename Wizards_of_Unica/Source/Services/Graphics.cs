using SFML.Graphics;
using System.Collections.Generic;

namespace tndwolf.ECS
{
	/// <summary>
	/// Inferface for all the graphics services.
	/// Graphics are used to manage textures and shapes
	/// </summary>
	public interface Graphics {
		string AssetsFolder { get; set;}
		Font DefaultFont { get; set; }
		Texture GetTexture (string name);
		Texture LoadTexture (string file);
	}

	/// <summary>
	/// Empty implementation of the Graphical services
	/// </summary>
	public class NullGraphics: Graphics {
		public Texture res = new Texture (2, 2);
		public string AssetsFolder { get; set;}
		public Font DefaultFont { get; set; }

		public NullGraphics() {
			AssetsFolder = "Assets";
			DefaultFont = null;
		}

		public Texture GetTexture(string file) {
			return res;
		}

		public Texture LoadTexture(string file) {
			return res;
		}
	}

	/// <summary>
	/// Standard implementation of the Graphical services
	/// </summary>
	public class DefaultGraphics: Graphics {
		Dictionary<string, Texture> textures = new Dictionary<string, Texture> ();
		public string AssetsFolder { get; set;}
		public Font DefaultFont { get; set; }

		public DefaultGraphics() {
			AssetsFolder = "Assets";
			DefaultFont = null;
		}

		public Texture GetTexture(string file) {
			return textures [file];
		}

		public Texture LoadTexture(string file) {
			try {
				Texture texture;
				if(textures.TryGetValue(file, out texture) == false) {
					texture = new Texture(AssetsFolder + "/" + file);
					texture.Repeated = true;
					texture.Smooth = false;
					textures[file] = texture;
				}
				return texture;
			} catch {
				return null;
			}
		}

		public Color SampleColor(Sprite sprite) {
			var tmpRes = new Dictionary<Color, int>();
			var img = sprite.Texture.CopyToImage();
			var rect = sprite.TextureRect;
			for(uint y = 0; y < rect.Height; y++) {
				for(uint x = 0; x < rect.Height; x++) {
					var buff = img.GetPixel(x, y);
					if(buff.Equals(Color.Transparent)) {
						if(tmpRes.ContainsKey(buff)) {
							tmpRes[buff] = tmpRes[buff] + 1;
						} else {
							tmpRes[buff] = 1;
						}
					}
				}
			}
			Color res = Color.Black;
			var resRef = 0;
			foreach(var pair in tmpRes) {
				if(pair.Value > resRef) {
					resRef = pair.Value;
					res = pair.Key;
				}
			}
			return res;
		}
	}
}

