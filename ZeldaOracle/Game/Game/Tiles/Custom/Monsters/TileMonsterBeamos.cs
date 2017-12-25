﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ZeldaOracle.Common.Geometry;
using ZeldaOracle.Common.Graphics;
using ZeldaOracle.Common.Scripting;
using ZeldaOracle.Game.Entities;
using ZeldaOracle.Game.Entities.Effects;
using ZeldaOracle.Game.Entities.Projectiles;
using ZeldaOracle.Game.Entities.Projectiles.MonsterProjectiles;
using ZeldaOracle.Game.Entities.Projectiles.Seeds;

namespace ZeldaOracle.Game.Tiles {

	public class TileMonsterBeamos : Tile {

		private int shootTimer;
		private bool shooting;
		private int canShoot;
		private int angle;
		private int rotateTimer;
		private int shootAngle;


		//-----------------------------------------------------------------------------
		// Constructor
		//-----------------------------------------------------------------------------

		public TileMonsterBeamos() {
		}


		//-----------------------------------------------------------------------------
		// Internal methods
		//-----------------------------------------------------------------------------

		private void BeginShoot() {
			shooting = true;
			shootTimer = 0;

			// Determine shoot vector
			Vector2F vectorToPlayer = RoomControl.Player.Position - Center;
			int shootAngleCount = GameSettings.MONSTER_BEAMOS_SHOOT_ANGLE_COUNT;
			float radians = (float) Math.Atan2((double) -vectorToPlayer.Y, (double) vectorToPlayer.X);
			if (radians < 0.0f)
				radians += GMath.TwoPi;
			shootAngle = (int) GMath.Round((radians * shootAngleCount) / GMath.TwoPi);
			shootAngle = GMath.Wrap(shootAngle, shootAngleCount);
		}

		private void EndShoot() {
			Graphics.ImageVariant = GameData.VARIANT_BLUE;
			shooting = false;
			canShoot = 3; // Must rotate 3 times before shooting again
		}

		private void ShootBeam(bool flicker) {
			Vector2F vectorToPlayer = RoomControl.Player.Position - Center;
			int angleToPlayer = Angles.NearestFromVector(vectorToPlayer);
			
			int shootAngleCount = GameSettings.MONSTER_BEAMOS_SHOOT_ANGLE_COUNT;
			float shootRadians = (shootAngle * GMath.TwoPi) / (float) shootAngleCount;
			Vector2F shootVector = new Vector2F(
				(float) Math.Cos((double) shootRadians),
				(float) -Math.Sin((double) shootRadians));
			
			// Create the projectile
			BeamProjectile projectile = new BeamProjectile();
			projectile.Flickers	= flicker;
			projectile.Owner	= null;
			projectile.Source	= this;
			projectile.Angle	= angle;
			projectile.Physics.Velocity	= shootVector *
				GameSettings.MONSTER_BEAMOS_SHOOT_SPEED;
			RoomControl.SpawnEntity(projectile, Center);
		}


		//-----------------------------------------------------------------------------
		// Overridden methods
		//-----------------------------------------------------------------------------
		
		public override void OnInitialize() {
			shootTimer	= 0;
			shooting	= false;
			angle		= Angles.Up;
			rotateTimer	= 15;
			Graphics.PlayAnimation(GameData.ANIM_MONSTER_BEAMOS);
			Graphics.ImageVariant = GameData.VARIANT_BLUE;
			Graphics.SubStripIndex = angle;
		}

		public override void Update() {
			base.Update();

			Vector2F vectorToPlayer = RoomControl.Player.Position - Center;
			int angleToPlayer = Angles.NearestFromVector(vectorToPlayer);
			
			// Update shooting
			if (shooting) {
				shootTimer++;

				// 14 frames before first flash
				// shoot happens at last frame of second flash
				// 15 seconds after last flash
				if (shootTimer >= 14) {
					int flashIndex = (shootTimer - 14) / 4;

					if (shootTimer >= 21) {
						int shootIndex = shootTimer - 21;
						if (shootIndex < 10) {
							// Shoot the beam!
							ShootBeam(((shootTimer - 21) % 2) == 1);
						}
					}

					if (flashIndex < 4) {
						if (flashIndex % 2 == 0)
							Graphics.ImageVariant = GameData.VARIANT_HURT;
						else
							Graphics.ImageVariant = GameData.VARIANT_BLUE;
					}
					else
						EndShoot();
					Graphics.SubStripIndex = angle;
				}
			}
			else {
				// Update rotation
				rotateTimer--;
				if (rotateTimer <= 0) {
					angle = (angle + 1) % Angles.AngleCount;
					rotateTimer = (angle % 2 == 0 ? 15 : 25);
					Graphics.SubStripIndex = angle;

					if (canShoot > 0)
						canShoot--;
				}
				
				// Check for shooting
				if (canShoot == 0 && angleToPlayer == angle)
					BeginShoot();
			}
		}


		//-----------------------------------------------------------------------------
		// Static Methods
		//-----------------------------------------------------------------------------

		/// <summary>Draws the tile data to display in the editor.</summary>
		public new static void DrawTileData(Graphics2D g, TileDataDrawArgs args) {
			Tile.DrawTileData(g, args);
		}
	}
}