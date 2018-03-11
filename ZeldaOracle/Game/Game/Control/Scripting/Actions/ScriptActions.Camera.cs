﻿
using ZeldaOracle.Common.Geometry;
using ZeldaOracle.Game.Entities;

namespace ZeldaOracle.Game.Control.Scripting.Actions {

	public class ScriptActionsCamera :
		ScriptActionsSection, ZeldaAPI.ScriptActionsCamera
	{

		public void SetCameraTargetToEntity(ZeldaAPI.Entity entity) {
			RoomControl.ViewControl.SetTarget((Entity) entity);
		}

		public void SetCameraTargetToPoint(Vector2F point) {
			RoomControl.ViewControl.SetTarget(point);
		}

		public void ResetCamera() {
			RoomControl.ViewControl.SetTarget(RoomControl.Player);
		}
	}
}
