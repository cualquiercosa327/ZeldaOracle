﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ZeldaOracle.Game.Worlds;
using ZeldaEditor.Control;
using ZeldaOracle.Common.Geometry;
using ZeldaOracle.Game.Tiles;
using ZeldaOracle.Common.Scripting;
using ZeldaOracle.Game.Control.Scripting;

namespace ZeldaEditor.Undo {
	public class ActionRenameID : EditorAction {

		private IIDObject idObject;
		private string oldID;
		private string newID;

		public ActionRenameID(IIDObject idObject, string newID) {
			ActionName = "Rename " + idObject.GetType().Name;
			ActionIcon = EditorImages.Rename;
			this.idObject = idObject;
			this.oldID = idObject.ID;
			this.newID = newID;
		}

		public override void Undo(EditorControl editorControl) {
			if (idObject is Level) {
				editorControl.World.RenameLevel((Level)idObject, oldID);
				editorControl.EditorWindow.TreeViewWorld.RefreshLevels();
			}
			else if (idObject is Dungeon) {
				editorControl.World.RenameDungeon((Dungeon)idObject, oldID);
				editorControl.EditorWindow.TreeViewWorld.RefreshDungeons();
			}
			else if (idObject is Script) {
				editorControl.World.RenameScript((Script)idObject, oldID);
				editorControl.EditorWindow.TreeViewWorld.RefreshScripts(true, false);
				editorControl.ScriptRenamed(newID, oldID);
			}
			else if (idObject is World) {
				editorControl.World.ID = oldID;
				editorControl.EditorWindow.TreeViewWorld.RefreshWorld();
			}
		}

		public override void Redo(EditorControl editorControl) {
			if (idObject is Level) {
				editorControl.World.RenameLevel((Level)idObject, newID);
				editorControl.EditorWindow.TreeViewWorld.RefreshLevels();
			}
			else if (idObject is Dungeon) {
				editorControl.World.RenameDungeon((Dungeon)idObject, newID);
				editorControl.EditorWindow.TreeViewWorld.RefreshDungeons();
			}
			else if (idObject is Script) {
				editorControl.World.RenameScript((Script)idObject, newID);
				editorControl.EditorWindow.TreeViewWorld.RefreshScripts(true, false);
				editorControl.ScriptRenamed(oldID, newID);
			}
			else if (idObject is World) {
				editorControl.World.ID = newID;
				editorControl.EditorWindow.TreeViewWorld.RefreshWorld();
			}
		}

		public override bool IgnoreAction { get { return newID == oldID; } }
	}
}