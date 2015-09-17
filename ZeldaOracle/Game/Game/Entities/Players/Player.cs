﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ZeldaOracle.Common.Content;
using ZeldaOracle.Common.Geometry;
using ZeldaOracle.Common.Graphics;
using ZeldaOracle.Common.Input;
using ZeldaOracle.Game.Main;
using ZeldaOracle.Game.Entities.Effects;
using ZeldaOracle.Game.Entities.Projectiles;
using ZeldaOracle.Game.Items;
using ZeldaOracle.Game.Items.Weapons;
using ZeldaOracle.Game.Tiles;


//States:
//	- None (movement, item control)
//-------------------------------------
//	- Minecart (item control)
//	- Carrying Item (movement, throw)
//	- Swimming (movement, diving)
//-------------------------------------
//	- Ledge Jump
//	- Busy
//	- [SwitchHook]
//	- [SwingingItem]
//	- Die


namespace ZeldaOracle.Game.Entities.Players {
	
	public class Player : Unit {
	
		private int				direction;
		private int				angle;
		private Item[]			equippedItems; // TODO: move this to somewhere else.
		private bool			syncAnimationWithDirection; // TODO: better name for this.
		private bool			checkGroundTiles;
		private bool			autoRoomTransition; // The player doesn't need to be moving to transition.
		private Vector2F		roomEnterPosition; // The position the player was at when he entered the room.

		private PlayerState				state;
		private PlayerNormalState		stateNormal;
		private PlayerJumpState			stateJump;
		private PlayerSwimState			stateSwim;
		private PlayerLedgeJumpState	stateLedgeJump;



		//-----------------------------------------------------------------------------
		// Constructors
		//-----------------------------------------------------------------------------

		public Player() {
			direction		= Directions.Down;
			angle			= Directions.ToAngle(direction);
			equippedItems	= new Item[2] { null, null };
			syncAnimationWithDirection = true;
			checkGroundTiles	= true;
			autoRoomTransition	= false;

			// Physics.
			Physics.CollideWithWorld = true;
			Physics.HasGravity = true;
			Physics.CollideWithRoomEdge = true;

			// DEBUG: equip a bow item.
			equippedItems[0] = new ItemBow();
			equippedItems[1] = new ItemFeather();

			state			= null;
			stateNormal		= new PlayerNormalState();
			stateJump		= new PlayerJumpState();
			stateSwim		= new PlayerSwimState();
			stateLedgeJump	= new PlayerLedgeJumpState();
			

			Graphics.ShadowDrawOffset = new Point2I(0, -2);
			Health = 4 * 3;
			MaxHealth = 4 * 3;
		}


		//-----------------------------------------------------------------------------
		// Player states
		//-----------------------------------------------------------------------------

		public void Jump() {
			if (state is PlayerNormalState) {
				BeginState(stateJump);
			}
				//((PlayerNormalState) state).Jump();
		}

		public void BeginState(PlayerState newState) {
			if (state != newState) {
				if (state != null)
					state.End();
				newState.Begin(this);
				state = newState;
			}
		}

		private void CheckTiles() {
			if (IsOnGround) {
				Point2I origin = (Point2I) position - new Point2I(0, 2);
				Point2I location = origin / new Point2I(GameSettings.TILE_SIZE, GameSettings.TILE_SIZE);
				if (!RoomControl.IsTileInBounds(location))
					return;

				for (int i = 0; i < RoomControl.Room.LayerCount; i++) {
					Tile tile = RoomControl.GetTile(location, i);
					if (tile != null) {
					
						if (state != stateSwim && tile.Flags.HasFlag(TileFlags.Water)) {
							BeginState(stateSwim);
							
							// Create a splash effect.
							Effect splash = new Effect(GameData.ANIM_EFFECT_WATER_SPLASH);
							splash.Position = position - new Vector2F(0, 4);
							RoomControl.SpawnEntity(splash);

							return;
						}
					}
				}
			}
		}

		public void UpdateEquippedItems() {
			for (int i = 0; i < equippedItems.Length; i++) {
				if (equippedItems[i] != null) {
					equippedItems[i].Player = this;
					if (i == 0 && Controls.A.IsPressed())
						equippedItems[i].OnButtonPress();
					else if (i == 1 && Controls.B.IsPressed())
						equippedItems[i].OnButtonPress();
					//equippedItems[i].Update();
				}
			}
		}


		//-----------------------------------------------------------------------------
		// Overridden methods
		//-----------------------------------------------------------------------------
		
		public override void Initialize() {
			base.Initialize();

			// Play the default player animation.
			Graphics.PlayAnimation(GameData.ANIM_PLAYER_DEFAULT);

			BeginState(stateNormal);
			//BeginState(new PlayerSwimState());
		}

		public override void OnEnterRoom() {
			state.OnEnterRoom();
		}

		public override void OnLeaveRoom() {
			state.OnLeaveRoom();
		}

		public override void Update() {

			if (checkGroundTiles)
				CheckTiles();

			// Update the current player state.
			state.Update();

			if (syncAnimationWithDirection)
				Graphics.SubStripIndex = direction;

			// Update superclass.
			base.Update();
		}

		public override void Draw(Graphics2D g) {
			base.Draw(g);
		}

		
		//-----------------------------------------------------------------------------
		// Properties
		//-----------------------------------------------------------------------------

		public int Angle {
			get { return angle; }
			set { angle = value; }
		}
		
		public int Direction {
			get { return direction; }
			set { direction = value; }
		}
		
		public bool SyncAnimationWithDirection {
			get { return syncAnimationWithDirection; }
			set { syncAnimationWithDirection = value; }
		}
		
		public bool CheckGroundTiles {
			get { return checkGroundTiles; }
			set { checkGroundTiles = value; }
		}
		
		public bool AutoRoomTransition {
			get { return autoRoomTransition; }
			set { autoRoomTransition = value; }
		}
		
		public Vector2F RoomEnterPosition {
			get { return roomEnterPosition; }
			set { roomEnterPosition = value; }
		}
		
		public PlayerNormalState NormalState {
			get { return stateNormal; }
		}
		
		public PlayerSwimState SwimState {
			get { return stateSwim; }
		}
		
		public PlayerJumpState JumpState {
			get { return stateJump; }
		}
		
		public PlayerLedgeJumpState LedgeJumpState {
			get { return stateLedgeJump; }
		}
	}
}
