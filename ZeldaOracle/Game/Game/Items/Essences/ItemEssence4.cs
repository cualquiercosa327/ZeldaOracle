﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ZeldaOracle.Common.Geometry;
using ZeldaOracle.Common.Graphics;

namespace ZeldaOracle.Game.Items.Essences {
	public class ItemEssence4 : ItemEssence {

		//-----------------------------------------------------------------------------
		// Constructor
		//-----------------------------------------------------------------------------

		public ItemEssence4() : base() {
			this.id = "item_essence_4";
			this.name = new string[] { "Burning Flame" };
			this.description = new string[] { "It reignights a hero's passion deep in wavering hearts." };
			this.slot = 3;
			this.sprite = new Sprite[] { new Sprite(GameData.SHEET_ITEMS_LARGE, new Point2I(11, 7)) };
			this.spriteLight = new Sprite[] { new Sprite(GameData.SHEET_ITEMS_LARGE_LIGHT, new Point2I(11, 7)) };
		}

	}
}
