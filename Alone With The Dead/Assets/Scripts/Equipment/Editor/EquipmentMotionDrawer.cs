using System;
using UnityEngine;
using UnityEditor;

namespace HQFPSWeapons
{
	[CustomPropertyDrawer(typeof(EquipmentMotionState))]
	public class EquipmentMotionDrawer : PropertyDrawer
	{
		public static event Action<EquipmentMotionDrawer> EnabledVisualization;

		private bool m_Enabled;
		private bool m_Visualize;
		private float m_VisualizationSpeed = 3f;

		private EquipmentPhysics m_MoverComponent;
		private EquipmentPhysics.StateType m_StateType;
		

		public override void OnGUI (Rect position, SerializedProperty property, GUIContent label)
		{
			if(property.isExpanded)
				GUI.Box(new Rect(position.x, position.y + 16f, position.width, position.height - 16f), "");

			if(!m_Enabled)
				OnEnable(property);

			EditorGUI.PropertyField(position, property, true);

			if(property.isExpanded)
			{
				if(Application.isPlaying)
				{
					// Visualize button
					position.x += 8f;
					position.y = position.yMax - 38f;
					position.height = 16f;
					position.width -= 16f;

					USEditorUtility.DoHorizontalLine(new Rect(position.x, position.y - 4f, position.width, 1f));

					GUI.color = m_Visualize ? Color.grey : Color.white;

					if(GUI.Button(position, "Visualize"))
					{
						if(m_MoverComponent != null && m_MoverComponent.enabled && m_MoverComponent.gameObject.activeInHierarchy)
						{
							m_Visualize = !m_Visualize;

							if(m_Visualize)
								EnabledVisualization(this);

							EquipmentPhysics.StateType stateToVisualize = EquipmentPhysics.StateType.None;

							if(m_Visualize)
								stateToVisualize = m_StateType;

							m_MoverComponent.VisualizeState(stateToVisualize, m_VisualizationSpeed);
						}
					}

					GUI.color = Color.white;

					// Visualize speed
					position.y = position.yMax + 2f;
					m_VisualizationSpeed = EditorGUI.Slider(position, "Speed", m_VisualizationSpeed, 0f, 20f);
				}
					
				if(GUI.changed && m_Visualize && m_MoverComponent != null)
					m_MoverComponent.VisualizeState(m_StateType, m_VisualizationSpeed);
			}
		}
		
		public override float GetPropertyHeight (SerializedProperty property, GUIContent label)
		{
			return EditorGUI.GetPropertyHeight(property) + ((Application.isPlaying && property.isExpanded) ? 44f : 0f);
		}

		private void OnEnable(SerializedProperty property)
		{
			EnabledVisualization += On_EnabledVisualization;

			Selection.selectionChanged += On_SelectionChanged;

			m_MoverComponent = property.serializedObject.targetObject as EquipmentPhysics;

			if(property.name == "IdleState")
				m_StateType = EquipmentPhysics.StateType.Idle;
			else if(property.name == "WalkState")
				m_StateType = EquipmentPhysics.StateType.Walk;
			else if(property.name == "RunState")
				m_StateType = EquipmentPhysics.StateType.Run;
			else if(property.name == "AimState")
				m_StateType = EquipmentPhysics.StateType.Aim;
			else if (property.name == "CrouchState")
				m_StateType = EquipmentPhysics.StateType.Crouch;
			else if(property.name == "OnLadderState")
				m_StateType = EquipmentPhysics.StateType.OnLadder;
			else if(property.name == "TooCloseState")
				m_StateType = EquipmentPhysics.StateType.Retraction;

			m_Enabled = true;
		}

		private void On_EnabledVisualization(EquipmentMotionDrawer drawer)
		{
			m_Visualize = (drawer == this);
		}

		private void On_SelectionChanged()
		{
			if(m_MoverComponent == null || Selection.activeGameObject == null || Selection.activeGameObject != m_MoverComponent.gameObject)
				OnDestroy();
		}

		private void OnDestroy()
		{
			EnabledVisualization -= On_EnabledVisualization;
			Selection.selectionChanged -= On_SelectionChanged;

			if(m_MoverComponent != null)
				m_MoverComponent.VisualizeState(EquipmentPhysics.StateType.None);
		}
	}
}