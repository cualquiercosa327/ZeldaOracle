﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ZeldaOracle.Common.Geometry;
using ZeldaOracle.Common.Graphics;
using ZeldaOracle.Common.Scripting;
using ZeldaOracle.Game.API;
using ZeldaOracle.Game.Entities.Projectiles;

namespace ZeldaOracle.Game.Tiles {

	public class TileColorCubeSlot : Tile, ZeldaAPI.ColorCubeSlot {

		//-----------------------------------------------------------------------------
		// Constructor
		//-----------------------------------------------------------------------------

		public TileColorCubeSlot() {

		}


		//-----------------------------------------------------------------------------
		// Overridden methods
		//-----------------------------------------------------------------------------

		public override void OnCoverComplete(Tile tile) {
			if (tile is TileColorCube) {
				TileColorCube colorCube = (TileColorCube) tile;
				Color = ((TileColorCube) tile).TopColor;
			}
		}

		public override void OnUncoverComplete(Tile tile) {
			if (tile is TileColorCube) {
				Color = PuzzleColor.None;
			}
		}

		public override void OnInitialize() {
		}


		//-----------------------------------------------------------------------------
		// Static Methods
		//-----------------------------------------------------------------------------

		/// <summary>Draws the tile data to display in the editor.</summary>
		public new static void DrawTileData(Graphics2D g, TileDataDrawArgs args) {
			Tile.DrawTileData(g, args);
		}


		//-----------------------------------------------------------------------------
		// Properties
		//-----------------------------------------------------------------------------

		public PuzzleColor Color {
			get { return Properties.GetEnum<PuzzleColor>("color", PuzzleColor.None); }
			set {
				PuzzleColor prevColor = Color;
				Properties.Set("color", (int) value);
				if (prevColor != value) {
					GameControl.FireEvent(this, "color_change", this, value);
				}
			}
		}
		

		//-----------------------------------------------------------------------------
		// Scripting API
		//-----------------------------------------------------------------------------

		/*ZeldaAPI.Color ZeldaAPI.ColorCubeSlot.Color {
			get { return (ZeldaAPI.Color) Color; }
			set { Color = (PuzzleColor) value; }
		}*/
	}
}
