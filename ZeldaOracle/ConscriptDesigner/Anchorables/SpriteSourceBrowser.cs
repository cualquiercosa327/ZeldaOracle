﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Xml;
using ConscriptDesigner.Control;

namespace ConscriptDesigner.Anchorables {
	public class SpriteSourceBrowser : RequestCloseAnchorable {

		private SpriteSourceBrowserControl browser;


		//-----------------------------------------------------------------------------
		// Constructor
		//-----------------------------------------------------------------------------

		/// <summary>Constructs the sprite browser.</summary>
		public SpriteSourceBrowser() {
			Border border = CreateBorder();
			this.browser = new SpriteSourceBrowserControl();
			border.Child = this.browser;

			Closed += OnAnchorableClosed;

			Title = "Source Browser";
			Content = border;
		}


		//-----------------------------------------------------------------------------
		// Control
		//-----------------------------------------------------------------------------

		public void RefreshList() {
			browser.RefreshList();
		}

		public void ClearList() {
			browser.ClearList();
		}


		//-----------------------------------------------------------------------------
		// XML Serialization
		//-----------------------------------------------------------------------------

		public override void ReadXml(XmlReader reader) {
			base.ReadXml(reader);
			DesignerControl.MainWindow.SpriteSourceBrowser = this;
		}


		//-----------------------------------------------------------------------------
		// Event Handlers
		//-----------------------------------------------------------------------------

		/// <summary>Called to force cleanup during close.</summary>
		private void OnAnchorableClosed(object sender, EventArgs e) {
			browser.Dispose();
		}
	}
}
