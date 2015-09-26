﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ZeldaOracle.Common.Audio;
using ZeldaOracle.Common.Geometry;
using ZeldaOracle.Common.Graphics;
using ZeldaOracle.Game.Control;

namespace ZeldaOracle.Game.Items.Rewards {
	public class RewardRupee : Reward {

		protected int amount;

		//-----------------------------------------------------------------------------
		// Constructors
		//-----------------------------------------------------------------------------

		public RewardRupee(string id, int amount, string message, Sprite sprite) {
			this.id				= id;
			this.animation		= new Animation(sprite);
			this.message		= message;
			this.hasDuration	= true;
			this.holdType		= RewardHoldTypes.Raise;
			this.isCollectibleWithItems	= true;

			this.amount			= amount;
		}
		public RewardRupee(string id, int amount, string message, Animation animation) {
			this.id				= id;
			this.animation		= animation;
			this.message		= message;
			this.hasDuration	= true;
			this.holdType		= RewardHoldTypes.Raise;
			this.isCollectibleWithItems	= true;
		}

		//-----------------------------------------------------------------------------
		// Virtual methods
		//-----------------------------------------------------------------------------

		public override void OnCollect(GameControl gameControl) {
			if (gameControl.HUD.DynamicRupees >= gameControl.Inventory.GetAmmo("rupees").MaxAmount)
				AudioSystem.PlaySound("Pickups/get_rupee");

			gameControl.Inventory.GetAmmo("rupees").Amount += amount;
		}

		//-----------------------------------------------------------------------------
		// Properties
		//-----------------------------------------------------------------------------

	}
}