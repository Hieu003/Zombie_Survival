using System;
using UnityEngine;

namespace HQFPSWeapons.UserInterface
{
	public enum CrosshairType 
	{
		Static, Dynamic
	}

	[Serializable]
	public class CrosshairData
	{
		[DatabaseItem]
		public string ItemName;

		public Color NormalColor = Color.white;

		public Color OnEntityColor = Color.red;

		public bool ShowWhenAiming;

		[Space]

		public CrosshairType Type;

		[ShowIf("Type", (int)CrosshairType.Static)]
		public StaticCrosshairSettings StaticCrosshair;

		[ShowIf("Type", (int)CrosshairType.Dynamic)]
		public DynamicCrosshairSettings DynamicCrosshair;
	}

	[Serializable]
	public class StaticCrosshairSettings
	{
		public Sprite Sprite;

		public Vector2 Size = new Vector2(64f, 64f);
	}

	[Serializable]
	public class DynamicCrosshairSettings
	{
		[Range(0f, 3f)]
		public float IdleScale = 1f;

		[Range(0f, 3f)]
		public float CrouchScale = 0.65f;

		[Range(0f, 3f)]
		public float WalkScale = 1.2f;

		[Range(0f, 3f)]
		public float RunScale = 1.5f;

		[Range(0f, 3f)]
		public float JumpScale = 1.8f;

		[Space]

		[Range(0f, 1f)]
		public float AimMultiplier = 0.5f;

		[Space]

		[Range(0f, 10f)]
		public float m_PunchSize = 2f;

		[Range(0f, 20f)]
		public float m_MoveSpeed = 3f;
	}
}