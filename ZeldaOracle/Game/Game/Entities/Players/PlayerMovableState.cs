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
using ZeldaOracle.Game.Control;
using ZeldaOracle.Game.Tiles;
using ZeldaOracle.Common.Input.Controls;

namespace ZeldaOracle.Game.Entities.Players {
	public class PlayerMovableState : PlayerState {

		private AnalogStick		analogStick;
		private float			analogAngle;
		private bool			analogMode;				// True if the analog stick is active.
		private InputControl[]	moveButtons;			// The 4 movement controls for each direction.
		private bool[]			moveAxes;				// Which axes the player is moving on.
		protected bool			isMoving;				// Is the player holding down a movement key?
		private Vector2F		motion;					// The vector that's driving the player's velocity.
		private Vector2F		velocityPrev;			// The player's velocity on the previous frame.

		protected int			moveAngle;				// The angle the player is moving in.
		protected float			moveSpeed;				// The top-speed for movement.
		protected float			moveSpeedScale;			// Scales the movement speed to create the actual top-speed.
		protected bool			isSlippery;				// Is the movement acceleration-based?
		protected float			acceleration;			// Acceleration when moving.
		protected float			deceleration;			// Deceleration when not moving.
		protected float			minSpeed;				// Minimum speed threshhold used to jump back to zero when decelerating.
		protected bool			autoAccelerate;			// Should the player still accelerate without holding down a movement key?
		protected int			directionSnapCount;		// The number of intervals movement directions should snap to for acceleration-based movement.
		protected bool			allowMovementControl;	// Is the player allowed to control his movement?

		protected bool			strafing;				// The player can only face one direction.


		//-----------------------------------------------------------------------------
		// Constructors
		//-----------------------------------------------------------------------------

		public PlayerMovableState() {
			// Internal.
			moveAxes	= new bool[] { false, false };
			motion		= Vector2F.Zero;
			isMoving	= false;

			// Controls.
			analogStick = GamePad.GetStick(Buttons.LeftStick);
			analogAngle = 0.0f;
			moveButtons = new InputControl[4];
			moveButtons[Directions.Up]		= Controls.Up;
			moveButtons[Directions.Down]	= Controls.Down;
			moveButtons[Directions.Left]	= Controls.Left;
			moveButtons[Directions.Right]	= Controls.Right;

			// Movement settings.
			analogMode				= false;
			allowMovementControl	= true;
			moveAngle				= Angles.South;
			moveSpeed				= GameSettings.PLAYER_MOVE_SPEED; // 0.5f for swimming, 1.5f for sprinting.
			moveSpeedScale			= 1.0f;
			acceleration			= 0.08f; // 0.02f for ice, 0.08f for swimming, 0.1f for jumping
			deceleration			= 0.05f;
			minSpeed				= 0.05f;
			isSlippery				= false;
			autoAccelerate			= false;
			directionSnapCount		= 0;	// 8 for swimming/jumping, 16 for ice.
			strafing				= false;
		}
		
		
		//-----------------------------------------------------------------------------
		// Movement.
		//-----------------------------------------------------------------------------

		private void UpdateMoveControls() {
			isMoving = false;

			// Check analog stick.
			if (!analogStick.Position.IsZero) {
				analogMode = true;

				analogAngle = analogStick.Position.Direction;

				CheckAnalogStick();
			}
			else {
				analogMode = false;

				// Check movement keys.
				if (!CheckMoveKey(Directions.Left) && !CheckMoveKey(Directions.Right))
					moveAxes[0] = false;	// x-axis
				if (!CheckMoveKey(Directions.Down) && !CheckMoveKey(Directions.Up))
					moveAxes[1] = false;	// y-axis
			}

			// Don't auto-dodge collisions when moving at an angle.
			player.Physics.SetFlags(PhysicsFlags.AutoDodge,
				Angles.IsHorizontal(moveAngle) || Angles.IsVertical(moveAngle));

			// Update movement or acceleration.
			if (allowMovementControl && (isMoving || (autoAccelerate && isSlippery))) {
				if (!isMoving) {
					player.Angle = Directions.ToAngle(player.Direction);
					moveAngle = Directions.ToAngle(player.Direction);
				}

				float scaledSpeed = moveSpeed * moveSpeedScale;
				Vector2F keyMotion = Angles.ToVector(moveAngle) * scaledSpeed; // The velocity we want to move at.
				if (analogMode)
					keyMotion = analogStick.Position * scaledSpeed;

				// Update acceleration-based motion.
				if (isSlippery) {
					// If player velocity has been halted by collisions, mirror that in the motion vector.
					Vector2F velocity = player.Physics.Velocity;
					if (Math.Abs(velocity.X) < Math.Abs(velocityPrev.X) || Math.Sign(velocity.X) != Math.Sign(velocityPrev.X))
						motion.X = velocity.X;
					if (Math.Abs(velocity.Y) < Math.Abs(velocityPrev.Y) || Math.Sign(velocity.Y) != Math.Sign(velocityPrev.Y))
						motion.Y = velocity.Y;

					// Apply acceleration and limit speed.
					motion += keyMotion * acceleration;
					float newLength = motion.Length;
					if (newLength >= scaledSpeed)
						motion.Length = scaledSpeed;

					// TODO: what does this do again?
					if (Math.Abs(newLength - (motion + (keyMotion * 0.08f)).Length) < acceleration * 2.0f) {
						motion += keyMotion * 0.04f;
					}

					// Set the players velocity.
					if (directionSnapCount > 0) {
						// Snap velocity direction.
						float snapInterval = ((float) GMath.Pi * 2.0f) / directionSnapCount;
						float theta = (float) Math.Atan2(-motion.Y, motion.X);
						if (theta < 0)
							theta += (float) Math.PI * 2.0f;
						int angle = (int) ((theta / snapInterval) + 0.5f);
						player.Physics.Velocity = new Vector2F(
							(float)  Math.Cos(angle * snapInterval) * motion.Length,
							(float) -Math.Sin(angle * snapInterval) * motion.Length);
					}
					else {
						// Don't snap velocity direction.
						player.Physics.Velocity = motion;
					}
				}
				else {
					// For non-acceleration based motion, move at regular speed.
					motion = keyMotion;
					Player.Physics.Velocity = motion;
				}
			}
			else {
				// Stop movement, using deceleration for slippery motion.
				if (isSlippery) {
					float length = motion.Length;
					if (length < minSpeed)
						motion = Vector2F.Zero;
					else
						motion.Length = length - deceleration;
					player.Physics.Velocity = motion;
				}
				else {
					motion = Vector2F.Zero;
					Player.Physics.Velocity = Vector2F.Zero;
				}
			}
		}
		
		// Poll the movement key for the given direction, returning true if
		// it is down. This also manages the strafing behavior of movement.
		private bool CheckMoveKey(int dir) {
			if (moveButtons[dir].IsDown() || analogMode) {
				isMoving = true;
			
				if (!moveAxes[(dir + 1) % 2])
					moveAxes[dir % 2] = true;
				if (moveAxes[dir % 2]) {
					moveAngle = dir * 2;

					if (moveButtons[(dir + 1) % 4].IsDown())
						moveAngle = (moveAngle + 1) % 8;
					if (moveButtons[(dir + 3) % 4].IsDown())
						moveAngle = (moveAngle + 7) % 8;

					// Don't affect the facing direction when strafing
					if (!strafing) {
						if (!analogMode) {
							Player.Direction = dir;
							Player.Angle = dir * 2;

							if (moveButtons[(dir + 1) % 4].IsDown())
								Player.Angle = (Player.Angle + 1) % 8;
							if (moveButtons[(dir + 3) % 4].IsDown())
								Player.Angle = (Player.Angle + 7) % 8;
						}
						else {
							if ((analogAngle >= 0f && analogAngle <= 45f) || (analogAngle >= 315f && analogAngle <= 360f))
								player.Direction = Directions.Right;
							else if (analogAngle >= 45f && analogAngle <= 135f)
								player.Direction = Directions.South;
							else if (analogAngle >= 135f && analogAngle <= 225f)
								player.Direction = Directions.Left;
							else
								player.Direction = Directions.North;
						}
					}
				}
				return true;
			}
			return false;
		}

		// Poll the movement key for the given direction, returning true if
		// it is down. This also manages the strafing behavior of movement.
		private void CheckAnalogStick() {
			isMoving = true;

			// Don't affect the facing direction when strafing
			if (!strafing) {
				moveAxes[0] = true;
				moveAxes[1] = true;


				if (analogAngle <= 45f || analogAngle >= 315f)
					player.Direction = Directions.East;
				else if (analogAngle <= 135f)
					player.Direction = Directions.South;
				else if (analogAngle <= 225f)
					player.Direction = Directions.West;
				else
					player.Direction = Directions.North;

				if (analogAngle <= 22.5f || analogAngle >= 337.5f)
					player.Angle = Angles.East;
				else if (analogAngle <= 67.5f)
					player.Angle = Angles.SouthEast;
				else if (analogAngle <= 112.5f)
					player.Angle = Angles.South;
				else if (analogAngle <= 157.5f)
					player.Angle = Angles.SouthWest;
				else if (analogAngle <= 202.5f)
					player.Angle = Angles.West;
				else if (analogAngle <= 247.5f)
					player.Angle = Angles.NorthWest;
				else if (analogAngle <= 292.5f)
					player.Angle = Angles.North;
				else
					player.Angle = Angles.NorthEast;
			}
		}

		//-----------------------------------------------------------------------------
		// Overridden methods
		//-----------------------------------------------------------------------------

		public override void OnBegin() {
			player.Angle	= Directions.ToAngle(player.Direction);
			motion			= player.Physics.Velocity;
		}
		
		public override void OnEnd() {
			player.Angle = Directions.ToAngle(player.Direction);
		}

		public override void Update() {
			UpdateMoveControls();
			velocityPrev = player.Physics.Velocity;
		}

	}
}
