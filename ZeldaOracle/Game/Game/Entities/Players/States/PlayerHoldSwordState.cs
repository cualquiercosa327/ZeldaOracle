﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ZeldaOracle.Common.Geometry;
using ZeldaOracle.Common.Graphics;
using ZeldaOracle.Game.Entities.Collisions;
using ZeldaOracle.Game.Entities.Effects;
using ZeldaOracle.Game.Entities.Units;
using ZeldaOracle.Game.Items;
using ZeldaOracle.Game.Main;
using ZeldaOracle.Game.Tiles;

namespace ZeldaOracle.Game.Entities.Players.States {
	public class PlayerHoldSwordState : PlayerState {
		
		// Collision boxes for the the sword.
		private readonly Rectangle2I[] SWORD_COLLISION_BOXES = {
			new Rectangle2I(8 - 2, 0, 14, 8),
			new Rectangle2I(-8, -8 - 12, 8, 14),
			new Rectangle2I(-8 - 12, 0, 14, 8),
			new Rectangle2I(-1, 8, 8, 14)
		};


		private PlayerState nextState;
		private Animation weaponAnimation;
		private ItemWeapon weapon;
		private int chargeTimer;
		private int direction;
		protected UnitTool playerTool;


		//-----------------------------------------------------------------------------
		// Constants
		//-----------------------------------------------------------------------------

		private const int ChargeTime = 40;


		//-----------------------------------------------------------------------------
		// Constructors
		//-----------------------------------------------------------------------------

		public PlayerHoldSwordState() {
			weaponAnimation	= GameData.ANIM_SWORD_HOLD;
			nextState		= null;
			weapon			= null;
			chargeTimer		= 0;
			direction		= Directions.Right;
			playerTool		= null;
		}
		
		
		//-----------------------------------------------------------------------------
		// Internal methods
		//-----------------------------------------------------------------------------

		private void StabTile(Tile tile) {
			if (player.IsOnGround) {
				tile.OnSwordHit();

				// Create cling effect.
				if (!tile.IsDestroyed) {
					Effect clingEffect = new Effect(GameData.ANIM_EFFECT_CLING_LIGHT, DepthLayer.EffectCling);
					Vector2F pos = player.Center + (13 * Directions.ToVector(direction));
					player.RoomControl.SpawnEntity(clingEffect, pos);
				}
			}
			player.SwordStabState.Weapon = weapon;
			player.BeginState(player.SwordStabState);
		}


		//-----------------------------------------------------------------------------
		// Overridden methods
		//-----------------------------------------------------------------------------

		public override void OnBegin(PlayerState previousState) {

			// The player can hold his sword while ledge jumping.

			if (!(previousState is PlayerLedgeJumpState))
				chargeTimer = 0;

			if (weapon.IsEquipped && weapon.IsButtonDown()) {
				player.Movement.IsStrafing		= true;
				player.Movement.MoveCondition	= PlayerMoveCondition.FreeMovement;
				player.Graphics.PlayAnimation(GameData.ANIM_PLAYER_DEFAULT);
				
				player.EquipTool(player.ToolSword);

				direction = Player.Direction;
				if (!(previousState is PlayerLedgeJumpState))
					player.ToolSword.PlayAnimation(weaponAnimation);

				player.ToolSword.AnimationPlayer.SubStripIndex = direction;
			}
			else {
				if (chargeTimer >= ChargeTime)
					player.BeginState(player.SpinSwordState);
				else
					player.BeginNormalState();
			}
		}
		
		public override void OnEnd(PlayerState newState) {
			player.Movement.IsStrafing		= false;
			player.Movement.MoveCondition	= PlayerMoveCondition.FreeMovement;
			
			// The player can hold his sword while ledge jumping.
			if (!(newState is PlayerLedgeJumpState))
				player.UnequipTool(player.ToolSword);
		}

		public override void Update() {
			base.Update();

			player.Direction = direction;

			Rectangle2I box = SWORD_COLLISION_BOXES[direction];
			box.Point += (Point2I) player.CenterOffset;
			player.ToolSword.CollisionBox = box;

			// Charge up the sword.
			chargeTimer++;
				if (chargeTimer == ChargeTime) {
				player.ToolSword.AnimationPlayer.Animation = GameData.ANIM_SWORD_CHARGED;
				// TODO: play charge sound.
			}
			
			// Check for tiles to stab.
			CollisionInfo collisionInfo = player.Physics.CollisionInfo[player.Direction];
			Tile tile = player.Physics.GetMeetingSolidTile(player.Position, player.Direction);
			if (tile != null && player.Movement.IsMoving && collisionInfo.Type == CollisionType.Tile)
				StabTile(tile);

			// Release the sword button (spin if charged).
			else if (!weapon.IsEquipped || !weapon.IsButtonDown()) {
				if (chargeTimer >= ChargeTime)
					player.BeginState(player.SpinSwordState);
				else
					player.BeginNormalState();
			}
		}


		//-----------------------------------------------------------------------------
		// Properties
		//-----------------------------------------------------------------------------

		public PlayerState NextState {
			get { return nextState; }
			set { nextState = value; }
		}

		public Animation WeaponAnimation {
			get { return weaponAnimation; }
			set { weaponAnimation = value; }
		}

		public ItemWeapon Weapon {
			get { return weapon; }
			set { weapon = value; }
		}
	}
}
