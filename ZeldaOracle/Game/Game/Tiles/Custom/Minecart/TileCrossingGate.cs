﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ZeldaOracle.Common.Audio;
using ZeldaOracle.Common.Geometry;
using ZeldaOracle.Common.Graphics;
using ZeldaOracle.Common.Scripting;
using ZeldaOracle.Game.Entities;
using ZeldaOracle.Game.Entities.Projectiles;

namespace ZeldaOracle.Game.Tiles {
	
	public class TileCrossingGate : Tile, ZeldaAPI.CrossingGate {
		
		private Tile dummySolidTile;


		//-----------------------------------------------------------------------------
		// Constructor
		//-----------------------------------------------------------------------------

		public TileCrossingGate() {
			Graphics.SyncPlaybackWithRoomTicks = false;
		}


		//-----------------------------------------------------------------------------
		// Crossing Gate methods
		//-----------------------------------------------------------------------------

		public void Raise() {
			if (!IsRaised) {
				Properties.Set("raised", true);
				dummySolidTile.IsSolid = false;
				Graphics.PlayAnimation(GameData.ANIM_TILE_CROSSING_GATE_RAISE);
				AudioSystem.PlaySound(GameData.SOUND_CROSSING_GATE);
			}
		}
		
		public void Lower() {
			if (IsRaised) {
				Properties.Set("raised", false);
				dummySolidTile.IsSolid = true;
				Graphics.PlayAnimation(GameData.ANIM_TILE_CROSSING_GATE_LOWER);
				AudioSystem.PlaySound(GameData.SOUND_CROSSING_GATE);
			}
		}


		//-----------------------------------------------------------------------------
		// Overridden methods
		//-----------------------------------------------------------------------------

		public override void OnInitialize() {
			if (IsRaised)
				Graphics.PlayAnimation(GameData.ANIM_TILE_CROSSING_GATE_RAISE);
			else
				Graphics.PlayAnimation(GameData.ANIM_TILE_CROSSING_GATE_LOWER);
			
			bool isFacingLeft = Properties.GetBoolean("face_left", false);

			Graphics.AnimationPlayer.SkipToEnd();
			Graphics.SubStripIndex = (isFacingLeft ? 1 : 0);

			CollisionModel = (isFacingLeft ? GameData.MODEL_EDGE_W : GameData.MODEL_EDGE_E);

			Point2I trackLocation = Location;
			if (isFacingLeft)
				trackLocation.X -= 1;
			else
				trackLocation.X += 1;

			// Create the dummy tile to serve as the solid
			dummySolidTile = Tile.CreateTile(new TileData());
			dummySolidTile.CollisionModel	= new CollisionModel(new Rectangle2I(0, 0, 16, 8));
			dummySolidTile.ClingWhenStabbed	= false;
			dummySolidTile.SolidType		= TileSolidType.Solid;
			dummySolidTile.IsSolid			= !IsRaised;
			RoomControl.PlaceTile(dummySolidTile, trackLocation, Layer);
		}


		//-----------------------------------------------------------------------------
		// Properties
		//-----------------------------------------------------------------------------

		public bool IsRaised {
			get { return Properties.GetBoolean("raised", false); }
		}
	}
}