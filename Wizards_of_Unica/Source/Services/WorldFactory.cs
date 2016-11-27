// Wizard's Duel, a procedural tactical RPG
// Copyright (C) 2015  Luca Carbone
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
using System.Xml;
using System.Text.RegularExpressions;
using tndwolf.Utils;

namespace tndwolf.ECS {
	public class BlockExit {
		public int X;
		public int Y;
		public string Direction;
		// N, S, W, E, no need for an enum
		public bool CanBeClosed;

		public string FlipDirection() {
			switch(Direction) {
				case "N":
					return "S";
				case "S":
					return "N";
				default:
					return Direction;
			}
		}

		public string FlipMirrorDirection() {
			switch(Direction) {
				case "N":
					return "S";
				case "S":
					return "N";
				case "W":
					return "E";
				case "E":
					return "W";
				default:
					return Direction;
			}
		}

		public string MirrorDirection() {
			switch(Direction) {
				case "W":
					return "E";
				case "E":
					return "W";
				default:
					return Direction;
			}
		}

		public string RotateDirectionCCW() {
			switch(Direction) {
				case "N":
					return "W";
				case "S":
					return "E";
				case "W":
					return "S";
				case "E":
					return "N";
				default:
					return Direction;
			}
		}

		public string RotateDirectionCW() {
			switch(Direction) {
				case "N":
					return "E";
				case "S":
					return "W";
				case "W":
					return "N";
				case "E":
					return "S";
				default:
					return Direction;
			}
		}
	}

	public class BlockObject {
		public string TemplateId;
		public int X;
		public int Y;
		public float Probability;
		public string[] Variations;
		public bool IsTrigger = false;
		public int Width; // used for triggers
		public int Height; // used for triggers
	}

	public class ConstructionBlock {
		private char[,] block;
		private List<BlockExit> exits = new List<BlockExit>();
		private List<BlockObject> objects = new List<BlockObject>();

		public ConstructionBlock(string block, int width, int height, string id) {
			width = (width < 1) ? 1 : width;
			height = (height < 1) ? 1 : height;
			block = (block.Length < 1) ? "#" : block;
			this.block = new char[width, height];
			for(int y = 0; y < Height; y++) {
				for(int x = 0; x < Width; x++) {
					this.block[x, y] = block[x + width * y];
				}
			}
			ID = id;
			EndPoint = null;
			StartPoint = null;
		}

		public void AddExit(int x, int y, string direction, bool canBeClosed) {
			if(x < Width && x >= 0 && y < Height && y >= 0 &&
				(direction == "N" || direction == "S" || direction == "W" || direction == "E")) {
				var exit = new BlockExit { X = x, Y = y, Direction = direction, CanBeClosed = canBeClosed };
				exits.Add(exit);
			}
		}

		public void AddObject(string oid, int x, int y, float probability, string[] variations) {
			if(x < Width && x >= 0 && y < Height && y >= 0) {
				var obj = new BlockObject { TemplateId = oid, X = x, Y = y, Probability = probability, Variations = variations };
				obj.IsTrigger = false;
				objects.Add(obj);
			}
		}

		public void AddTrigger(string script, int x, int y, int width, int height, string[] variations) {
			//Services.Logger.Error("ConstructionBlock.AddTrigger", "Creating trigger: " + script);
			if(x < Width && x >= 0 && y < Height && y >= 0) {
				var obj = new BlockObject { TemplateId = script, X = x, Y = y, Probability = 1f, Variations = variations };
				obj.IsTrigger = true;
				obj.Width = width;
				obj.Height = height;
				objects.Add(obj);
			}
		}

		// TODO apply transformation (rotation, flip etc...)
		public BlockObject EndPoint { get; set; }

		public List<BlockExit> Exits {
			get { return exits; }
		}

		public int ExitsCount {
			get { return exits.Count; }
		}

		public ConstructionBlock Flip() {
			string bblock = "";
			for(int y = Height - 1; y >= 0; y--) {
				for(int x = 0; x < Width; x++) {
					bblock += block[x, y];
				}
			}
			var res = new ConstructionBlock(bblock, Width, Height, ID);
			foreach(var exit in exits) {
				res.AddExit(exit.X, Height - 1 - exit.Y, exit.FlipDirection(), exit.CanBeClosed);
			}
			foreach(var obj in objects) {
				if(Array.IndexOf(obj.Variations, "F") > -1) {
					if(obj.IsTrigger) {
						res.AddTrigger(obj.TemplateId, obj.X, Height - 1 - obj.Y, obj.Width, obj.Height, new string[] { });
					}
					else {
						res.AddObject(obj.TemplateId, obj.X, Height - 1 - obj.Y, obj.Probability, new string[] { });
					}
				}
			}
			return res;
		}

		public ConstructionBlock FlipMirror() {
			string bblock = "";
			for(int y = Height - 1; y >= 0; y--) {
				for(int x = Width - 1; x >= 0; x--) {
					bblock += block[x, y];
				}
			}
			var res = new ConstructionBlock(bblock, Width, Height, ID);
			foreach(var exit in exits) {
				res.AddExit(Width - 1 - exit.X, Height - 1 - exit.Y, exit.FlipMirrorDirection(), exit.CanBeClosed);
			}
			foreach(var obj in objects) {
				if(Array.IndexOf(obj.Variations, "FM") > -1) {
					if(obj.IsTrigger) {
						res.AddTrigger(obj.TemplateId, Width - 1 - obj.X, Height - 1 - obj.Y, obj.Width, obj.Height, new string[] { });
					}
					else {
						res.AddObject(obj.TemplateId, Width - 1 - obj.X, Height - 1 - obj.Y, obj.Probability, new string[] { });
					}
				}
			}
			return res;
		}

		public string ID { get; set; }

		public BlockExit GetAccess(string exitDirection) {
			foreach(var exit in exits) {
				if(exitDirection == "N" && exit.Direction == "S")
					return exit;
				else if(exitDirection == "S" && exit.Direction == "N")
					return exit;
				else if(exitDirection == "W" && exit.Direction == "E")
					return exit;
				else if(exitDirection == "E" && exit.Direction == "W")
					return exit;
			}
			return null;
		}

		public string GetTile(int x, int y) {
			return block[x, y].ToString();
		}

		public int Height {
			get { return block.GetLength(1); }
		}

		public ConstructionBlock Mirror() {
			string bblock = "";
			for(int y = 0; y < Height; y++) {
				for(int x = Width - 1; x >= 0; x--) {
					bblock += block[x, y];
				}
			}
			var res = new ConstructionBlock(bblock, Width, Height, ID);
			foreach(var exit in exits) {
				res.AddExit(Width - 1 - exit.X, exit.Y, exit.MirrorDirection(), exit.CanBeClosed);
			}
			foreach(var obj in objects) {
				if(Array.IndexOf(obj.Variations, "M") > -1) {
					if(obj.IsTrigger) {
						res.AddTrigger(obj.TemplateId, Width - 1 - obj.X, obj.Y, obj.Width, obj.Height, new string[] { });
					}
					else {
						res.AddObject(obj.TemplateId, Width - 1 - obj.X, obj.Y, obj.Probability, new string[] { });
					}
				}
			}
			return res;
		}

		public List<BlockObject> Objects {
			get { return objects; }
		}

		public int Occurrencies { get; set; }

		public ConstructionBlock RotateCCW() {
			var newWidth = block.GetLength(1);
			var newHeight = block.GetLength(0);
			char[,] newMatrix = new char[newWidth, newHeight];
			for(var oy = 0; oy < newWidth; oy++) {
				for(var ox = 0; ox < newHeight; ox++) {
					var nx = oy;
					var ny = newHeight - ox - 1;
					newMatrix[nx, ny] = block[ox, oy];
				}
			}
			////////
			string bblock = "";
			for(int y = 0; y < newHeight; y++) {
				for(int x = 0; x < newWidth; x++) {
					bblock += newMatrix[x, y];
				}
			}
			var res = new ConstructionBlock(bblock, newWidth, newHeight, ID);
			foreach(var exit in exits) {
				var nx = exit.Y;
				var ny = newHeight - exit.X - 1;
				res.AddExit(nx, ny, exit.RotateDirectionCCW(), exit.CanBeClosed);
			}
			foreach(var obj in objects) {
				if(Array.IndexOf(obj.Variations, "CCW") > -1) {
					var nx = obj.Y;
					var ny = newHeight - obj.X - 1;
					if(obj.IsTrigger) {
						res.AddTrigger(obj.TemplateId, nx, ny, obj.Width, obj.Height, new string[] { });
					}
					else {
						res.AddObject(obj.TemplateId, nx, ny, obj.Probability, new string[] { });
					}
				}
			}
			return res;
		}

		public ConstructionBlock RotateCW() {
			var newWidth = block.GetLength(1);
			var newHeight = block.GetLength(0);
			char[,] newMatrix = new char[newWidth, newHeight];
			for(var oy = 0; oy < newWidth; oy++) {
				for(var ox = 0; ox < newHeight; ox++) {
					var nx = newWidth - oy - 1;
					var ny = ox;
					newMatrix[nx, ny] = block[ox, oy];
				}
			}
			////////
			string bblock = "";
			for(int y = 0; y < newHeight; y++) {
				for(int x = 0; x < newWidth; x++) {
					bblock += newMatrix[x, y];
				}
			}
			var res = new ConstructionBlock(bblock, newWidth, newHeight, ID);
			foreach(var exit in exits) {
				var nx = newWidth - exit.Y - 1;
				var ny = exit.X;
				res.AddExit(nx, ny, exit.RotateDirectionCW(), exit.CanBeClosed);
			}
			foreach(var obj in objects) {
				if(Array.IndexOf(obj.Variations, "CW") > -1) {
					var nx = newWidth - obj.Y - 1;
					var ny = obj.X;
					if(obj.IsTrigger) {
						res.AddTrigger(obj.TemplateId, nx, ny, obj.Width, obj.Height, new string[] { });
					}
					else {
						res.AddObject(obj.TemplateId, nx, ny, obj.Probability, new string[] { });
					}
				}
			}
			return res;
		}

		// TODO apply transformation (rotation, flip etc...)
		public BlockObject StartPoint { get; set; }

		override public string ToString() {
			var res = string.Format("<block width=\"{0}\" height=\"{1}\">\n", Width, Height);
			for(int y = 0; y < Height; y++) {
				for(int x = 0; x < Width; x++) {
					res += block[x, y].ToString();
				}
				res += "\n";
			}
			foreach(var exit in exits) {
				res += string.Format("<exit x=\"{0}\" y=\"{1}\" direction=\"{2}\"/>\n", exit.X, exit.Y, exit.Direction);
			}
			foreach(var obj in objects) {
				res += string.Format("<object ref=\"{0}\" x=\"{1}\" y=\"{2}\" probability=\"{3}\"/>\n", obj.TemplateId, obj.X, obj.Y, obj.Probability);
			}
			res += "</block>";
			return res;
		}

		public int Width {
			get { return block.GetLength(0); }
		}
	}

	public class BufferLevel {
		public const int MAX_ITERATIONS = 100;

		private int blockCount = 0;
		private RandomList<BlockExit> exits = new RandomList<BlockExit>();
		private int maxX = 0;
		private int maxY = 0;
		private int minX = 0;
		private int minY = 0;

		public BufferLevel(int w, int h, string defaultTile) {
			minX = w - 1;
			minY = h - 1;
			var data = new string[w, h];
			for(int y = 0; y < h; y++) {
				for(int x = 0; x < w; x++) {
					data[x, y] = defaultTile;
				}
			}
			Data = data;
			DefaultTile = defaultTile;
			IsUsed = new bool[w, h];
			UsedArea = 0;
			Objects = new List<BlockObject>();
			StartCell = new BlockObject { TemplateId = "start", Probability = 1f, X = 0, Y = 0 };
			EndCell = new BlockObject { TemplateId = "end", Probability = 1f, X = 0, Y = 0 };
		}

		public bool AddBlock(ConstructionBlock next) {
			// search for a valid exit
			BlockExit access;
			BlockExit exit;
			var iter = 0;
			var x = 0;
			var y = 0;
			do {
				exit = exits.Random();
				while(IsOpenExit(exit.X, exit.Y, exit.Direction) == false && iter++ < MAX_ITERATIONS) {
					if(exit.CanBeClosed) {
						// the exit was closed, we can just close it and remove it
						//Data [exit.X, exit.Y] = CloseTile;
						//exits.Remove (exit);
					}
					exit = exits.Random();
				}
				access = next.GetAccess(exit.Direction);
				if(access != null) {
					x = exit.X - access.X;
					y = exit.Y - access.Y;
					x += (access.Direction == "W") ? 1 : (access.Direction == "E") ? -1 : 0;
					y += (access.Direction == "N") ? 1 : (access.Direction == "S") ? -1 : 0;
					if(CanPlaceBlock(next, x, y) == false)
						access = null;
				}
			} while(access == null && iter++ < MAX_ITERATIONS);
			if(iter < MAX_ITERATIONS) {
				// BEFORE placing the block remove the used exit
				exits.Remove(exit);
				PlaceBlock(next, x, y);
				return true;
			}
			return false;
		}

		/// <summary>
		/// Places a random block inside the level, using the still open exits. If <c>false</c> is returned
		/// it means that no blocks can be added animaore (within a tolerance level).
		/// </summary>
		/// <returns><c>true</c>, if random block was added, <c>false</c> otherwise.</returns>
		/// <param name="blocks">Blocks.</param>
		public bool AddRandomBlock(RandomList<ConstructionBlock> blocks) {
			var exit = exits.Random();
			var iter = 0;
			var x = 0;
			var y = 0;
			if(exit != null) {
				// search for a valid exit
				while(IsOpenExit(exit.X, exit.Y, exit.Direction) == false && iter++ < MAX_ITERATIONS) {
					if(exit.CanBeClosed) {
						// the exit was closed, we can just close it and remove it
						//Data [exit.X, exit.Y] = CloseTile;
						//exits.Remove (exit);
					}
					exit = exits.Random();
				}
				// got one, now search for a block with a valid connection
				ConstructionBlock next = null;
				BlockExit access = null;
				do {
					next = blocks.Random();
					access = next.GetAccess(exit.Direction);
					if(access != null) {
						x = exit.X - access.X;
						y = exit.Y - access.Y;
						x += (access.Direction == "W") ? 1 : (access.Direction == "E") ? -1 : 0;
						y += (access.Direction == "N") ? 1 : (access.Direction == "S") ? -1 : 0;
						if(CanPlaceBlock(next, x, y) == false)
							access = null;
					}
				} while(access == null && iter++ < MAX_ITERATIONS);
				// found a good block? place it
				if(iter < MAX_ITERATIONS) {
					// BEFORE placing the block remove the used exit
					exits.Remove(exit);
					PlaceBlock(next, x, y);
					return true;
				}
			}
			return false;
		}

		public bool CanPlaceBlock(ConstructionBlock block, int x, int y) {
			for(int j = y; j < y + block.Height; j++) {
				for(int i = x; i < x + block.Width; i++) {
					if(IsValid(i, j) == false || IsUsed[i, j] == true) {
						return false;
					}
				}
			}
			return true;
		}

		public void CellularAutomata(string rule, string result, int iterations = 1) {
			for(var n = 0; n < iterations; n++) {
				for(var y = 1; y < Height - 1; y++) {
					for(var x = 1; x < Width - 1; x++) {
						if(Objects.Find(o => o.X == x && o.Y == y) != null) {
							// skip cell if some object was on it
							continue;
						}
						if(Data[x - 1, y] == "." && Data[x + 1, y] == "." && Data[x, y - 1] == "#" && Data[x, y + 1] == "#") {
							Data[x, y] = ".";
						}
						else if(Data[x - 1, y] == "#" && Data[x + 1, y] == "#" && Data[x, y - 1] == "." && Data[x, y + 1] == ".") {
							Data[x, y] = ".";
						}
					}
				}
			}
		}

		public int CountNeighbors(int x, int y, string toCount) {
			var res = 0;
			/*
			for (var j = y-1; j < y+1; j++) {
				for (var i = x-1; i < x+1; i++) {
					if (j == y && x == i)
						continue;
					if (IsValid (i, j) && Data[i, j] == toCount) {
						res++;
					}
				}
			}//*/
			//*
			if(IsValid(x, y, 1)) {
				if(Data[x - 1, y] == toCount)
					res++;
				if(Data[x + 1, y] == toCount)
					res++;
				if(Data[x, y - 1] == toCount)
					res++;
				if(Data[x, y + 1] == toCount)
					res++;
			}//*/
			return res;
		}

		public void CloseExits() {
			foreach(var exit in exits.Data.Keys) {
				if(exit.CanBeClosed) {
					Data[exit.X, exit.Y] = CloseTile;
				}
			}
			exits.Clear();
		}

		public string CloseTile { get; set; }

		public BlockObject EndCell { get; set; }

		public bool IsOpenExit(int x, int y, string direction) {
			if(IsValid(x, y, 1)) {
				switch(direction) {
					case "N":
						if(IsUsed[x, y - 1] == false)
							return true;
						break;
					case "S":
						if(IsUsed[x, y + 1] == false)
							return true;
						break;
					case "W":
						if(IsUsed[x - 1, y] == false)
							return true;
						break;
					case "E":
						if(IsUsed[x + 1, y] == false)
							return true;
						break;
				}
			}
			return false;
		}

		public string[,] Data { get; set; }

		public string DefaultTile { get; set; }

		public void Dump() {
			Console.Write("\n");
			for(int y = 0; y < Height; y++) {
				for(int x = 0; x < Width; x++) {
					if(Data[x, y] != null)
						Console.Write(Data[x, y]);
					else
						Console.Write("X");
				}
				Console.Write("\n");
			}
			Console.WriteLine("Placed blocks: " + blockCount);
			Console.WriteLine("Total area: " + UsedArea);
			Console.WriteLine("Width: " + Width);
			Console.WriteLine("Height: " + Height);
			Console.WriteLine("Unused exits: " + exits.Count);
		}

		public int Height { get { return Data.GetLength(1); } }

		public bool[,] IsUsed { get; set; }

		public bool IsValid(int x, int y, int tolerance = 0) {
			return (
				x - tolerance >= 0 &&
				x + tolerance < Data.GetLength(0) &&
				y - tolerance >= 0 &&
				y + tolerance < Data.GetLength(1)
			);
		}

		public List<BlockObject> Objects {
			get;
			set;
		}

		public void PlaceBlock(ConstructionBlock block, int x, int y) {
			for(int j = 0; j < block.Height; j++) {
				for(int i = 0; i < block.Width; i++) {
					//if (i + x >= 0 && i + x < maxX && j + y >= 0 && j + y < maxY && IsUsed [i + x, j + y] == false) {
					if(IsValid(i + x, j + y) && IsUsed[i + x, j + y] == false) {
						Data[i + x, j + y] = block.GetTile(i, j);
						IsUsed[i + x, j + y] = true;
						UsedArea++;
					}
				}
			}
			foreach(var exit in block.Exits) {
				var i = exit.X + x;
				var j = exit.Y + y;
				if(IsOpenExit(i, j, exit.Direction)) {
					var nexit = new BlockExit { X = i, Y = j, Direction = exit.Direction, CanBeClosed = exit.CanBeClosed };
					exits.Add(nexit);
				}
			}
			foreach(var obj in block.Objects) {
				var nobj = new BlockObject();
				nobj.TemplateId = obj.TemplateId;
				nobj.Probability = obj.Probability;
				nobj.X = obj.X + x;
				nobj.Y = obj.Y + y;
				nobj.IsTrigger = obj.IsTrigger;
				nobj.Width = obj.Width;
				nobj.Height = obj.Height;
				Objects.Add(nobj);
			}
			if(block.StartPoint != null) {
				StartCell.X = block.StartPoint.X + x;
				StartCell.Y = block.StartPoint.Y + y;
			}
			if(block.EndPoint != null) {
				EndCell.X = block.EndPoint.X + x;
				EndCell.Y = block.EndPoint.Y + y;
			}
			blockCount++;
			minX = (x < minX) ? (x > 0) ? x : 0 : minX;
			minY = (y < minY) ? (y > 0) ? y : 0 : minY;
			maxX = (x + block.Width > maxX) ? x + block.Width : maxX;
			maxY = (y + block.Height > maxY) ? y + block.Height : maxY;
		}

		public BlockObject StartCell { get; set; }

		public void Trim() {
			var halfTrimSize = 3;
			var trimSize = halfTrimSize * 2;
			var dx = maxX - minX + trimSize;
			var dy = maxY - minY + trimSize;
			var data = new string[dx, dy];
			var used = new bool[dx, dy];
			for(var y = 0; y < dy; y++) {
				for(var x = 0; x < dx; x++) {
					try {
						data[x, y] = Data[x + minX - halfTrimSize, y + minY - halfTrimSize];
						used[x, y] = IsUsed[x + minX - halfTrimSize, y + minY - halfTrimSize];
						if(y == 0 || x == 0 || y == dy - 1 || x == dx - 1) {
							data[x, y] = DefaultTile;
						}
					}
					catch {
						// XXX this happens when the trim goes beyond the starting level size
						// I keep it because of edges, to not end a level abruptly
						// No need to adjust final Width and Height... they are dynamic anyway
						data[x, y] = DefaultTile;
						Services.Logger.Debug("TestLevel.Trim", "invalid: " + (x + minX - halfTrimSize) + " " + (y + minY - halfTrimSize));
						Services.Logger.Debug("TestLevel.Trim", "vs: " + Width + " " + Height);
						Services.Logger.Debug("TestLevel.Trim", "max: " + maxX + " " + maxY);
						Services.Logger.Debug("TestLevel.Trim", "min: " + minX + " " + minY);
					}
				}
			}

			foreach(var obj in Objects) {
				obj.X = obj.X - minX + halfTrimSize;
				obj.Y = obj.Y - minY + halfTrimSize;
			}
			StartCell.X = StartCell.X - minX + halfTrimSize;
			StartCell.Y = StartCell.Y - minY + halfTrimSize;
			EndCell.X = EndCell.X - minX + halfTrimSize;
			EndCell.Y = EndCell.Y - minY + halfTrimSize;

			Data = data;
			IsUsed = used;
			minX = 0;
			maxX = dx;
			minY = 0;
			maxY = dy;
		}

		public SFML.Graphics.Image ToImage() {
			var res = new SFML.Graphics.Image((uint)Width, (uint)Height);
			for(uint y = 0; y < Height; y++) {
				for(uint x = 0; x < Width; x++) {
					switch(Data[x, y]) {
						case "#":
							res.SetPixel(x, y, SFML.Graphics.Color.Black);
							break;
						case ".":
							res.SetPixel(x, y, SFML.Graphics.Color.White);
							break;
						default:
							res.SetPixel(x, y, SFML.Graphics.Color.Cyan);
							break;
					}
				}
			}
			res.SetPixel((uint)StartCell.X, (uint)StartCell.Y, SFML.Graphics.Color.Green);
			res.SetPixel((uint)EndCell.X, (uint)EndCell.Y, SFML.Graphics.Color.Red);
			return res;
		}

		public int UsedArea { get; protected set; }

		public int Width { get { return Data.GetLength(0); } }
	}

	public class WorldFactory {
		private XmlDocument xdoc;
		private RandomList<ConstructionBlock> blocks = new RandomList<ConstructionBlock>();
		private Dictionary<string, ConstructionBlock> blocksById = new Dictionary<string, ConstructionBlock>();
		private List<string> endBlocks = new List<string>();
		private List<string> startBlocks = new List<string>();

		public void BuildBlock(XmlNode xblock) {
			var data = Regex.Replace(xblock.InnerText, @"\s+", "");
			var id = XmlParser.GetString("id", xblock);
			var block = new ConstructionBlock(
							data,
							XmlParser.GetInt("width", xblock),
							XmlParser.GetInt("height", xblock),
							XmlParser.GetString("id", xblock)
						);
			var children = xblock.ChildNodes;
			for(int i = 0; i < children.Count; i++) {
				switch(children[i].Name) {
					case "end":
						block.EndPoint = new BlockObject {
							TemplateId = "end",
							X = XmlParser.GetInt("x", children[i]),
							Y = XmlParser.GetInt("y", children[i]),
							Probability = 1f
						};
						break;

					case "exit":
						block.AddExit(
							XmlParser.GetInt("x", children[i]),
							XmlParser.GetInt("y", children[i]),
							XmlParser.GetString("direction", children[i]),
							XmlParser.GetBool("canBeClosed", children[i], false)
						);
						break;

					case "object":
						block.AddObject(
							XmlParser.GetString("ref", children[i]),
							XmlParser.GetInt("x", children[i]),
							XmlParser.GetInt("y", children[i]),
							XmlParser.GetFloat("probability", children[i]),
							XmlParser.GetStringArray("variations", children[i]).ToArray()
						);
						break;
					
					case "trigger":
						block.AddTrigger(
							children[i].InnerText,
							XmlParser.GetInt("x", children[i]),
							XmlParser.GetInt("y", children[i]),
							XmlParser.GetInt("width", children[i]),
							XmlParser.GetInt("height", children[i]),
							XmlParser.GetStringArray("variations", children[i]).ToArray()
						);
						break;

					case "start":
						block.StartPoint = new BlockObject {
							TemplateId = "start",
							X = XmlParser.GetInt("x", children[i]),
							Y = XmlParser.GetInt("y", children[i]),
							Probability = 1f
						};
						break;

					default:
						break;
				}
			}
			var pb = XmlParser.GetInt("occurs", xblock);
			blocks.Add(block, pb);
			blocksById.Add(block.ID, block);
			Services.Logger.Debug("WorldFactory.BuildBlock", "Built block:\n" + block);
			var variations = XmlParser.GetStringArray("variations", xblock).ToArray();
			if(Array.IndexOf(variations, "CCW") >= 0) {
				var ccwblock = block.RotateCCW();
				blocks.Add(ccwblock, pb);
				Services.Logger.Debug("WorldFactory.BuildBlock", "Built block CCW:\n" + ccwblock);
			}
			if(Array.IndexOf(variations, "CW") >= 0) {
				var cwblock = block.RotateCW();
				blocks.Add(cwblock, pb);
				Services.Logger.Debug("WorldFactory.BuildBlock", "Built block CW:\n" + cwblock);
			}
			if(Array.IndexOf(variations, "F") >= 0) {
				var fblock = block.Flip();
				blocks.Add(fblock, pb);
				Services.Logger.Debug("WorldFactory.BuildBlock", "Built block F:\n" + fblock);
			}
			if(Array.IndexOf(variations, "FM") >= 0) {
				var fmblock = block.FlipMirror();
				blocks.Add(fmblock, pb);
				Services.Logger.Debug("WorldFactory.BuildBlock", "Built block FM:\n" + fmblock);
			}
			if(Array.IndexOf(variations, "M") >= 0) {
				var mblock = block.Mirror();
				blocks.Add(mblock, pb);
				Services.Logger.Debug("WorldFactory.BuildBlock", "Built block M:\n" + mblock);
			}
		}

		public LevelActor BuildAI(XmlNode xblock) {
			// TODO set the correct ID
			var res = new LevelActor(-1);
			// Enemies
			var enemies = xdoc.SelectNodes("//enemy");
			for(int e = 0; e < enemies.Count; e++) {
				/*
				var bp = new EnemyBlueprint(
							 XmlParser.GetString(, "blueprint", enemies[e]),
							 XmlParser.GetInt("threat", enemies[e])
						 );
				var type = XmlParser.GetString("type", enemies[e]);
				switch(type) {
					case "MOB":
						bp.EnemyType = EnemyType.MOB;
						break;
					case "LIGHTNING_BRUISER":
						bp.EnemyType = EnemyType.LIGHTNING_BRUISER;
						break;
					case "GLASS_CANNON":
						bp.EnemyType = EnemyType.GLASS_CANNON;
						break;
					case "MIGHTY_GLACIER":
						bp.EnemyType = EnemyType.MIGHTY_GLACIER;
						break;
					case "STONE_WALL":
						bp.EnemyType = EnemyType.STONE_WALL;
						break;
					case "CASTER":
						bp.EnemyType = EnemyType.CASTER;
						break;
					default:
						break;
				}
				res.enemyBlueprints.Add(bp);//*/
			}
			var xenemies = xdoc.SelectSingleNode("//enemies");
			res.Progression = XmlParser.GetIntArray("threatProgression", xenemies);
			res.MaxThreat = XmlParser.GetInt("maxThreat", xenemies, 255);
			res.AlwaysIncreaseThreat = XmlParser.GetBool("alwaysIncreaseThreat", xenemies);
			// TODO Music
			var music = new BackgroundMusic(
				null,//Services.Audio.Load(XmlParser.GetString("file", xdoc.SelectSingleNode("//backgroundmusic"))),
				null//XmlParser.GetString("file", xdoc.SelectSingleNode("//backgroundmusic")))
			);
			var xloops = xdoc.SelectNodes("//loop");
			for(int i = 0; i < xloops.Count; i++) {
				music.AddLoop(
					XmlParser.GetString("name", xloops[i]),
					XmlParser.GetInt("start", xloops[i]),
					XmlParser.GetInt("end", xloops[i])
				);
				var loop = new MusicLoop {
					ID = XmlParser.GetString("name", xloops[i]),
					MaxThreat = XmlParser.GetInt("maxThreat", xloops[i])
				};
				res.MusicLoops.Add(loop);
			}
			res.MusicLoops.Sort((l1, l2) => l1.MaxThreat.CompareTo(l2.MaxThreat));
			return res;
		}

		public string DefaultTile { get; set; }

		/// <summary>
		/// Gets one of the possible end block at random.
		/// </summary>
		/// <value>The end block.</value>
		protected string EndBlock {
			get {
				return endBlocks[Services.Rng.Next(endBlocks.Count)];
			}
		}

		public string[,] Generate(Map res) {
			// TODO insert level actor inside world
			//res.AI = BuildAI(xdoc.DocumentElement);

			// Generate the map
			BufferLevel level = null;
			while(level == null) {
				var iter = 0;
				level = new BufferLevel(MaxWidth, MaxHeight, DefaultTile);
				level.CloseTile = DefaultTile;
				// Place the first block
				var last = blocksById[StartBlock];
				var x = Services.Rng.Next(1, MaxWidth);
				var y = Services.Rng.Next(1, MaxHeight);
				level.PlaceBlock(last, x, y);
				// place the other blocks
				while(level.UsedArea < MinArea && iter < 1000) {
					if(level.AddRandomBlock(blocks) == false) {
						level = null;
						break;
					}
				}
				if(level != null && level.AddBlock(blocksById[EndBlock]) == false) {
					level = null;
					Services.Logger.Debug("WorldFactory.Generate", "Invalid level, regenerating...");
				}
			}
			level.CloseExits();
			level.Trim();
			level.CellularAutomata("asd", "res", 1);
			//level.Dump ();
			var img = level.ToImage();
			img.SaveToFile(Services.Logger.LogsDirectory + "level.png");

			var levelDataString = "";
			for(var y = 0; y < level.Data.GetLength(1); y++) {
				for(var x = 0; x < level.Data.GetLength(0); x++) {
					levelDataString += level.Data[x, y];
				}
			}

			res.SetMap(level.Width, level.Height, level.DefaultTile[0], levelDataString);
			//res.EndCell = new SFML.Window.Vector2i(level.EndCell.X, level.EndCell.Y);
			res.StartCell = new SFML.Window.Vector2i(level.StartCell.X, level.StartCell.Y);
			var o = 0;
			foreach(var obj in level.Objects) {
				if(Services.Rng.NextDouble() < obj.Probability) {
					if(obj.IsTrigger) {
						Services.GameMechanics.CreateTrigger(obj.TemplateId, obj.X, obj.Y, obj.Width, obj.Height);
					}
					else {
						//Services.Logger.Debug("WorldFactory.Generate", "Crating object " + obj.ID + " at " + obj.X + "," + obj.Y);
						Services.GameMechanics.Create(obj.TemplateId, obj.X, obj.Y);
					}
					o++;
				}
			}

			return level.Data;
		}

		public void Initialize(string levelDefinitionFile) {
			xdoc = new XmlDocument();
			xdoc.Load(levelDefinitionFile);
			try {
				var xlevel = xdoc.SelectSingleNode("//level");
				MaxWidth = XmlParser.GetInt("maxWidth", xlevel);
				MaxHeight = XmlParser.GetInt("maxHeight", xlevel);
				MinArea = XmlParser.GetInt("minArea", xlevel);
				endBlocks = new List<string>(XmlParser.GetStringArray("end", xlevel));
				startBlocks = new List<string>(XmlParser.GetStringArray("start", xlevel));
				var xblocks = xdoc.SelectNodes("//block");
				for(var i = 0; i < xblocks.Count; i++) {
					BuildBlock(xblocks[i]);
				}
				var xtiles = xdoc.SelectSingleNode("//tiles");
				DefaultTile = XmlParser.GetString("default", xtiles);
			}
			catch(Exception ex) {
				Services.Logger.Error("WorldFactory.Initialize", ex.ToString());
			}
		}

		public void LoadTilemask(Map worldView) {
			var tilemasks = xdoc.SelectNodes("//tileMasks");
			for(int n = 0; n < tilemasks.Count; n++) {
				var tilemask = tilemasks[n];
				var parentLayer = XmlParser.GetInt("parent", tilemask);
				var texture = XmlParser.GetString("texture", tilemask);
				// load the default rule
				var rule = new Map.Rule();
				rule.x = XmlParser.GetIntArray("defaultX", tilemask).ToArray();
				rule.y = XmlParser.GetIntArray("defaultY", tilemask).ToArray();
				rule.w = XmlParser.GetInt("defaultW", tilemask);
				rule.h = XmlParser.GetInt("defaultH", tilemask);
				rule.texture = texture;
				worldView.AddRule(rule, parentLayer);

				// load all the rules
				var rules = tilemask.SelectNodes("./tile");
				for(var i = 0; i < rules.Count; i++) {
					try {
						var xrule = rules.Item(i);
						var type = XmlParser.GetString("type", xrule);
						if(type == "ROW") {
							rule = new Map.RowRule();
						}
						else if(type == "GRID") {
							rule = new Map.GridRule();
						}
						else {
							rule = new Map.Rule();
						}
						rule.x = XmlParser.GetIntArray("x", xrule).ToArray();
						rule.y = XmlParser.GetIntArray("y", xrule).ToArray();
						rule.dx = XmlParser.GetInt("dx", xrule);
						rule.dy = XmlParser.GetInt("dy", xrule);
						rule.w = XmlParser.GetInt("w", xrule);
						rule.h = XmlParser.GetInt("h", xrule);
						rule.z = XmlParser.GetInt("z", xrule);
						rule.maxX = XmlParser.GetInt("maxX", xrule);
						rule.maxX = (rule.maxX < 1) ? rule.w + rule.x[rule.x.Length - 1] : rule.maxX;
						rule.maxY = XmlParser.GetInt("maxY", xrule);
						rule.maxY = (rule.maxY < 1) ? rule.h + rule.y[rule.y.Length - 1] : rule.maxY;
						rule.texture = XmlParser.GetString("texture", xrule, texture);
						var conditions = xrule.ChildNodes;
						for(var j = 0; j < conditions.Count; j++) {
							var xcondition = conditions.Item(j);
							var condition = new Map.Rule.Condition() {
								dx = XmlParser.GetInt("dx", xcondition),
								dy = XmlParser.GetInt("dy", xcondition),
								value = XmlParser.GetString("value", xcondition)[0]
							};
							rule.conditions.Add(condition);
							//Services.Logger.Debug("WorldFactory.LoadTilemask", "Adding Condition: " + condition.ToString());
						}
						//Services.Logger.Debug("WorldFactory.LoadTilemask", "Adding Rule: " + rule.ToString());
						worldView.AddRule(rule, parentLayer);
					}
					catch(Exception ex) {
						Services.Logger.Warn("WorldFactory.LoadTilemask", /*rules.Item(i).ToString() + "; " +*/ ex.ToString());
					}
				}
			}
		}

		protected int MaxHeight { get; set; }

		protected int MaxWidth { get; set; }

		protected int MinArea { get; set; }

		/// <summary>
		/// Gets one of the possible start block at random.
		/// </summary>
		/// <value>The start block.</value>
		protected string StartBlock {
			get {
				return startBlocks[Services.Rng.Next(startBlocks.Count)];
			}
		}
	}
}

