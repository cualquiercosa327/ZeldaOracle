﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using ZeldaOracle.Common.Geometry;
using ZeldaOracle.Game;
using ZeldaOracle.Game.Worlds;
using ZeldaOracle.Common.Content;

namespace ZeldaEditor {
	public partial class LevelAddForm : Form {

		public LevelAddForm() {
			InitializeComponent();
			
			buttonAdd.DialogResult		= DialogResult.OK;
			buttonCancel.DialogResult	= DialogResult.Cancel;

			comboBoxRoomSize.SelectedIndex = 0;
			comboBoxZone.SelectedIndex = 0;
		}

		public string LevelName {
			get { return textBoxLevelName.Text; }
		}
		
		public int LevelWidth {
			get { return (int) numericLevelWidth.Value; }
		}
		
		public int LevelHeight {
			get { return (int) numericLevelHeight.Value; }
		}
		
		public int LevelLayerCount {
			get { return (int) numericLayerCount.Value; }
		}
		
		public Point2I LevelRoomSize {
			get {
				if (comboBoxRoomSize.SelectedIndex == 1)
					return GameSettings.ROOM_SIZE_LARGE;
				return GameSettings.ROOM_SIZE_SMALL;
			}
		}
		
		public Zone LevelZone {
			get { return null; }
			//get { return Resources.GetResource<Zone>(comboBoxZone.Text); }
		}
	}
}
