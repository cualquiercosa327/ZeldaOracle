﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ZeldaOracle.Common.Graphics;
using ZeldaOracle.Game.Control;
using ZeldaOracle.Game.Main;

namespace ZeldaOracle.Game.GameStates {
	
	public abstract class GameState {

		protected GameManager gameManager;
		private bool isActive;
		private bool isVisible;

		
		//-----------------------------------------------------------------------------
		// Constructor
		//-----------------------------------------------------------------------------

		public GameState() {
			isActive = false;
			isVisible = true;
		}

		//-----------------------------------------------------------------------------
		// Virtual methods
		//-----------------------------------------------------------------------------

		public virtual void OnBegin() {}

		public virtual void OnEnd() {}

		public virtual void Update() {}

		// So far, only used to assign palettes, can be renamed to PreDraw in the future
		public virtual void AssignPalettes() {}

		public virtual void Draw(Graphics2D g) {}


		//-----------------------------------------------------------------------------
		// Accessors
		//-----------------------------------------------------------------------------

		public void Begin(GameManager gameManager) {
			if (!isActive) {
				this.gameManager = gameManager;
				isActive = true;
				OnBegin();
			}
		}

		public void End() {
			if (isActive) {
				isActive = false;
				OnEnd();
			}
		}


		//-----------------------------------------------------------------------------
		// Properties
		//-----------------------------------------------------------------------------

		public bool IsActive {
			get { return isActive; }
			set { isActive = value; }
		}

		public bool IsVisible {
			get { return isVisible; }
			set { isVisible = value; }
		}

		public GameManager GameManager {
			get { return gameManager; }
			set { gameManager = value; }
		}

		public GameControl GameControl {
			get { return gameManager.GameControl; }
		}

		public virtual GameState CurrentGameState {
			get { return this; }
		}
	}
}
