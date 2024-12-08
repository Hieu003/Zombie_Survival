using System.Text;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEditorInternal;

namespace HQFPSWeapons
{
	public class ItemManagementWindow : EditorWindow
	{
		/// <summary>
		/// This is a hack for avoiding an issue with the ReorderableList's DrawHeader method. 
		/// </summary>
		public static bool DrawingItemWindow { get; private set; }

		public enum Tab { ItemEditor, PropertyEditor }

		private const float DESCRIPTION_HEIGHT = 54f;
		private const float PROPERTY_HEIGHT = 40f;

		private Tab m_SelectedTab;
		private SerializedObject m_ItemDatabase;
		private ReorderableList m_CategoryList;
		private ReorderableList m_PropertyList;

		private Vector2 m_CategoriesScrollPos;
		private Vector2 m_TypesScrollPos;
		private Vector2 m_PropsScrollPos;
		private Vector2 m_ItemsScrollPos;
		private Vector2 m_ItemInspectorScrollPos;

		private ReorderableList m_ItemList;
		//private ReorderableList m_CurItemDescriptions;
		private ReorderableList m_CurItemProperties;
		private ReorderableList m_CurItemRequiredItems;

		private string[] m_ItemNamesFull;
		private string[] m_ItemNames;


		[MenuItem("Tools/HQ FPS Weapons Pack/Item Management...", false, 6)]
		public static void Init()
		{
			EditorWindow.GetWindow<ItemManagementWindow>(true, "Item Management");
		}

		public void OnGUI()
		{
			DrawingItemWindow = true;

			if(m_ItemDatabase == null)
			{
				EditorGUILayout.HelpBox("No ItemDatabase was found in the Resources folder!", MessageType.Error);

				if(GUILayout.Button("Refresh"))
					InitializeWindow();

				if(m_ItemDatabase == null)
					return;
			}

			GUIStyle richTextStyle = new GUIStyle() { richText = true, alignment = TextAnchor.UpperRight };

			// Display the database path
			EditorGUILayout.LabelField(string.Format("Database path: '{0}'", AssetDatabase.GetAssetPath(m_ItemDatabase.targetObject)));

			// Display the shortcuts
			EditorGUI.LabelField(new Rect(position.width - 262f, 0f, 256f, 16f), "<b>Shift + D</b> to duplicate", richTextStyle);
			EditorGUI.LabelField(new Rect(position.width - 262f, 16f, 256f, 16f), "<b>Delete</b> to delete", richTextStyle);

			Vector2 buttonSize = new Vector2(192f, 32f);
			float topPadding = 32f;

			// Draw the "Item Editor" button.
			Rect itemEditorButtonRect = new Rect(position.width * 0.38f - buttonSize.x / 2f, topPadding, buttonSize.x, buttonSize.y);

			if(m_SelectedTab == Tab.ItemEditor)
				UnityEngine.GUI.backgroundColor = Color.grey;
			else
				UnityEngine.GUI.backgroundColor = Color.white;

			if(UnityEngine.GUI.Button(itemEditorButtonRect, "Item Editor"))
				m_SelectedTab = Tab.ItemEditor;

			// Draw the "Property Editor" button.
			Rect propertyEditorButtonRect = new Rect(position.width * 0.62f - buttonSize.x / 2f, topPadding, buttonSize.x, buttonSize.y);

			if(m_SelectedTab == Tab.PropertyEditor)
				UnityEngine.GUI.backgroundColor = Color.grey;
			else
				UnityEngine.GUI.backgroundColor = Color.white;

			if(UnityEngine.GUI.Button(propertyEditorButtonRect, "Property Editor"))
				m_SelectedTab = Tab.PropertyEditor;

			// Reset the bg color.
			UnityEngine.GUI.backgroundColor = Color.white;

			// Horizontal line.
			UnityEngine.GUI.Box(new Rect(0f, topPadding + buttonSize.y * 1.25f, position.width, 1f), "");

			// Draw the item / recipe editors.
			m_ItemDatabase.Update();

			float innerWindowPadding = 8f;
			Rect innerWindowRect = new Rect(innerWindowPadding, topPadding + buttonSize.y * 1.25f + innerWindowPadding, position.width - innerWindowPadding * 2f, position.height - (topPadding + buttonSize.y * 1.25f + innerWindowPadding * 4.5f));

			// Inner window box.
			UnityEngine.GUI.backgroundColor = Color.grey;
			UnityEngine.GUI.Box(innerWindowRect, "");
			UnityEngine.GUI.backgroundColor = Color.white;

			if(m_SelectedTab == Tab.ItemEditor)
				DrawItemEditor(innerWindowRect);
			else if(m_SelectedTab == Tab.PropertyEditor)
				DrawPropertyEditor(innerWindowRect);

			m_ItemDatabase.ApplyModifiedProperties();

			DrawingItemWindow = false;
		}

		private void OnEnable()
		{
			InitializeWindow();

			Undo.undoRedoPerformed += Repaint;
		}

		private void InitializeWindow()
		{
			var database = Resources.LoadAll<ItemDatabase>("")[0];

			if(database)
			{
				m_ItemDatabase = new SerializedObject(database);

				m_CategoryList = new ReorderableList(m_ItemDatabase, m_ItemDatabase.FindProperty("m_Categories"), true, true ,true ,true);
				m_CategoryList.drawElementCallback += DrawCategory;
				m_CategoryList.drawHeaderCallback = (Rect rect)=> { EditorGUI.LabelField(rect, ""); };
				m_CategoryList.onSelectCallback += On_SelectedCategory;
				m_CategoryList.onRemoveCallback = (ReorderableList list)=> { m_CategoryList.serializedProperty.DeleteArrayElementAtIndex(m_CategoryList.index); };

				m_PropertyList = new ReorderableList(m_ItemDatabase, m_ItemDatabase.FindProperty("m_ItemProperties"), true, true, true, true);
				m_PropertyList.drawElementCallback += DrawItemPropertyDefinition;
				m_PropertyList.drawHeaderCallback = (Rect rect)=> { EditorGUI.LabelField(rect, ""); };
			}
		}

		private void On_SelectedCategory(ReorderableList list)
		{
			m_ItemList = new ReorderableList(m_ItemDatabase, m_CategoryList.serializedProperty.GetArrayElementAtIndex(m_CategoryList.index).FindPropertyRelative("m_Items"), true, true, true, true);
			m_ItemList.drawElementCallback += DrawItem;
			m_ItemList.drawHeaderCallback = (Rect rect)=> { EditorGUI.LabelField(rect, ""); };
			m_ItemList.onSelectCallback += On_SelectedItem;
			m_ItemList.onRemoveCallback = (ReorderableList l)=> { m_ItemList.serializedProperty.DeleteArrayElementAtIndex(m_ItemList.index); };
			m_ItemList.onChangedCallback += On_SelectedItem;
		}

		private void On_SelectedItem(ReorderableList list)
		{
			if(m_ItemList == null || m_ItemList.count == 0 || m_ItemList.index == -1 || m_ItemList.index >= m_ItemList.count)
				return;

			m_ItemNames = ItemManagementUtility.GetItemNames(m_CategoryList.serializedProperty);
			m_ItemNamesFull = ItemManagementUtility.GetItemNamesFull(m_CategoryList.serializedProperty);

			m_CurItemProperties = new ReorderableList(m_ItemDatabase, m_ItemList.serializedProperty.GetArrayElementAtIndex(m_ItemList.index).FindPropertyRelative("m_PropertyValues"), true, true, true, true);
			m_CurItemProperties.drawHeaderCallback = (Rect rect)=> { EditorGUI.LabelField(rect, ""); };
			m_CurItemProperties.drawElementCallback += DrawItemPropertyValue;
			m_CurItemProperties.elementHeight = PROPERTY_HEIGHT;
		}

		private void DrawItemPropertyValue(Rect rect, int index, bool isActive, bool isFocused)
		{
			var list = m_CurItemProperties;

			if(list.serializedProperty.arraySize == index)
				return;

			var element = list.serializedProperty.GetArrayElementAtIndex(index);
			rect.y += 2f;
			rect.height -= 2f;
			ItemManagementUtility.DrawItemProperty(rect, element, m_PropertyList);

			ItemManagementUtility.DoListElementBehaviours(list, index, isFocused, this);
		}

		private void DrawItemEditor(Rect totalRect)
		{
			// Inner window cross (partitioning in 4 smaller boxes)
			UnityEngine.GUI.Box(new Rect(totalRect.x, totalRect.y + totalRect.height * 0.5f, totalRect.width / 2f, 1f), "");
			UnityEngine.GUI.Box(new Rect(totalRect.x + totalRect.width * 0.5f, totalRect.y, 1f, totalRect.height), "");

			Vector2 labelSize = new Vector2(192f, 20f);

			// Draw the item list.
			string itemListName = string.Format("Item List ({0})", (m_CategoryList.count == 0 || m_CategoryList.index == -1) ? "None" : m_CategoryList.serializedProperty.GetArrayElementAtIndex(m_CategoryList.index).FindPropertyRelative("m_Name").stringValue);

			UnityEngine.GUI.Box(new Rect(totalRect.x + totalRect.width * 0.25f - labelSize.x * 0.5f, totalRect.y, labelSize.x, labelSize.y), itemListName);
			Rect itemListRect = new Rect(totalRect.x, totalRect.y + labelSize.y, totalRect.width * 0.5f - 2f, totalRect.height * 0.5f - labelSize.y - 1f);

			if(m_CategoryList.count != 0 && m_CategoryList.index != -1 && m_CategoryList.index < m_CategoryList.count)
				DrawList(m_ItemList, itemListRect, ref m_ItemsScrollPos);
			else
			{
				itemListRect.x -= 6f;
				//UnityEngine.GUI.Label(itemListRect, "Select a category...", new GUIStyle() { fontStyle = FontStyle.BoldAndItalic });
				UnityEngine.GUI.Box(new Rect(itemListRect.x + itemListRect.width * 0.25f - labelSize.x * 0.5f, totalRect.y, labelSize.x / 1.2f, labelSize.y), "Select a category...");
			}

			// Draw the categories.
			UnityEngine.GUI.Box(new Rect(totalRect.x + totalRect.width * 0.25f - labelSize.x * 0.5f, totalRect.y + totalRect.height * 0.5f + 2f, labelSize.x, labelSize.y), "Category List");

			DrawList(m_CategoryList, new Rect(totalRect.x, totalRect.y + totalRect.height * 0.5f + labelSize.y + 2f, totalRect.width * 0.5f - 2f, totalRect.height * 0.5f - labelSize.y - 3f), ref m_CategoriesScrollPos);

			// Inspector label.
			UnityEngine.GUI.Box(new Rect(totalRect.x + totalRect.width * 0.75f - labelSize.x * 0.5f, totalRect.y, labelSize.x, labelSize.y), "Item Inspector");

			// Draw the inspector.
			bool itemIsSelected = m_CategoryList.count != 0 && m_ItemList != null && m_ItemList.count != 0 && m_ItemList.index != -1 && m_ItemList.index < m_ItemList.count;
			Rect inspectorRect = new Rect(totalRect.x + totalRect.width * 0.5f + 4f, totalRect.y + labelSize.y, totalRect.width * 0.5f - 5f, totalRect.height - labelSize.y - 1f);

			if(itemIsSelected)
				DrawItemInspector(inspectorRect);
			else
			{
				inspectorRect.x += 4f;
				inspectorRect.y += 4f;
				
				UnityEngine.GUI.Box(inspectorRect, "Select an item to inspect...");
			}
		}

		private void DrawList(ReorderableList list, Rect totalRect, ref Vector2 scrollPosition)
		{
			float scrollbarWidth = 16f;

			Rect onlySeenRect = new Rect(totalRect.x, totalRect.y, totalRect.width, totalRect.height);
			Rect allContentRect = new Rect(totalRect.x, totalRect.y, totalRect.width - scrollbarWidth, (list.count + 4) * list.elementHeight);

			scrollPosition = UnityEngine.GUI.BeginScrollView(onlySeenRect, scrollPosition, allContentRect, false, true);

			// Draw the clear button.
			Vector2 buttonSize = new Vector2(56f, 16f);

			if(list.count > 0 && UnityEngine.GUI.Button(new Rect(allContentRect.x + 2f, allContentRect.yMax - 60f, buttonSize.x, buttonSize.y), "Clear"))
			if(EditorUtility.DisplayDialog("Warning!", "Are you sure you want the list to be cleared? (All elements will be deleted)", "Yes", "Cancel"))
				list.serializedProperty.ClearArray();

			list.DoList(allContentRect);

			UnityEngine.GUI.EndScrollView();
		}

		private void DrawListElement(ReorderableList list, Rect rect, int index, bool isActive, bool isFocused)
		{
			if(list.serializedProperty.arraySize == index)
				return;

			var element = list.serializedProperty.GetArrayElementAtIndex(index);
			rect.y += 2;
			EditorGUI.PropertyField(new Rect(rect.x, rect.y, 256f, EditorGUIUtility.singleLineHeight), element, GUIContent.none);

			ItemManagementUtility.DoListElementBehaviours(list, index, isFocused, this);
		}

		private void DrawCategory(Rect rect, int index, bool isActive, bool isFocused) 
		{
			ItemManagementUtility.DrawListElementByName(m_CategoryList, index, rect, "m_Name", isFocused, this); 
		}

		private void DrawItem(Rect rect, int index, bool isActive, bool isFocused)
		{
			if(m_ItemList.serializedProperty.arraySize > index)
			{
				SerializedProperty item = m_ItemList.serializedProperty.GetArrayElementAtIndex(index);
				// SerializedProperty displayProp = item.FindPropertyRelative("m_DisplayName");

				// string toUse = (displayProp.stringValue == string.Empty) ? "m_Name" : "m_DisplayName";

				ItemManagementUtility.DrawListElementByName(m_ItemList, index, rect, "m_Name", isFocused, this);
			}
		}

		private void DrawItemPropertyDefinition(Rect rect, int index, bool isActive, bool isFocused)
		{
			DrawListElement(m_PropertyList, rect, index, isActive, isFocused);
		}

		private void DrawItemInspector(Rect viewportRect)
		{
			var item = m_ItemList.serializedProperty.GetArrayElementAtIndex(m_ItemList.index);

			GUI.Box(viewportRect, "");

			float indentation = 4f;
			Rect rect = new Rect(viewportRect.x + indentation, viewportRect.y + indentation, viewportRect.width - indentation * 2, viewportRect.height - indentation * 2);

			m_ItemInspectorScrollPos = GUI.BeginScrollView(viewportRect, m_ItemInspectorScrollPos, new Rect(rect.x, rect.y, rect.width - 16f, 24f + EditorGUI.GetPropertyHeight(item, true)));

			// Draw item name
			rect.xMin += indentation;
			rect.xMax -= 16f;
			rect.yMin += indentation;

			GUI.Label(rect, item.FindPropertyRelative("m_Name").stringValue, new GUIStyle() { fontStyle = FontStyle.Bold, fontSize = 20});

			// Draw all item fields
			rect.yMax -= 16f;
			rect.y += 24f;

			var properties = item.Copy().GetChildren();

			rect.height = EditorGUIUtility.singleLineHeight;
			rect.y += EditorGUIUtility.standardVerticalSpacing;
			foreach(var prop in properties)
			{
				EditorGUI.PropertyField(rect, prop, true);
				rect.y += EditorGUI.GetPropertyHeight(prop, true) + EditorGUIUtility.standardVerticalSpacing;
			}

			GUI.EndScrollView();
		}

		private void DrawPropertyEditor(Rect totalRect)
		{
			Vector2 labelSize = new Vector2(128f, 20f);

			// Properties label.
			UnityEngine.GUI.Box(new Rect(totalRect.x + totalRect.width * 0.5f - labelSize.x * 0.5f, totalRect.y, labelSize.x, labelSize.y), "Property List");

			// Draw the properties.
			totalRect.y += 24f;
			totalRect.height -= 25f;
			DrawList(m_PropertyList, totalRect, ref m_PropsScrollPos);
		}
	}

	public static class ItemManagementUtility
	{
		public static void DoListElementBehaviours(ReorderableList list, int index, bool isFocused, EditorWindow window = null)
		{
			var current = Event.current;

			if(current.type == EventType.KeyDown)
			{
				if(list.index == index && isFocused)
				{
					if(current.keyCode == KeyCode.Delete)
					{
						int newIndex = 0;
						if(list.count == 1)
							newIndex = -1;
						else if(index == list.count - 1)
							newIndex = index - 1;
						else if(index > 0)
							newIndex = index - 1;

						list.serializedProperty.DeleteArrayElementAtIndex(index);

						if(newIndex != -1)
						{
							list.index = newIndex;
							if(list.onSelectCallback != null)
								list.onSelectCallback(list);
						}

						Event.current.Use();
						if(window)
							window.Repaint();
					}
					else if(current.shift && current.keyCode == KeyCode.D)
					{
						list.serializedProperty.InsertArrayElementAtIndex(list.index);
						list.index ++;
						if(list.onSelectCallback != null)
							list.onSelectCallback(list);

						Event.current.Use();
						if(window)
							window.Repaint();
					}
				}
			}
		}

		public static string[] GetItemNamesFull(SerializedProperty categoryList)
		{
			List<string> names = new List<string>();

			for(int i = 0;i < categoryList.arraySize;i ++)
			{
				var category = categoryList.GetArrayElementAtIndex(i);
				var itemList = category.FindPropertyRelative("m_Items");
				for(int j = 0;j < itemList.arraySize;j ++)
					names.Add(category.FindPropertyRelative("m_Name").stringValue + "/" + itemList.GetArrayElementAtIndex(j).FindPropertyRelative("m_Name").stringValue);
			}

			return names.ToArray();
		}

		public static string[] GetItemNames(SerializedProperty categoryList)
		{
			List<string> names = new List<string>();
			for(int i = 0;i < categoryList.arraySize;i ++)
			{
				var category = categoryList.GetArrayElementAtIndex(i);
				var itemList = category.FindPropertyRelative("m_Items");
				for(int j = 0;j < itemList.arraySize;j ++)
					names.Add(itemList.GetArrayElementAtIndex(j).FindPropertyRelative("m_Name").stringValue);
			}

			return names.ToArray();
		}

		public static int GetItemIndex(SerializedProperty categoryList, string itemName)
		{
			int index = 0;
			for(int i = 0;i < categoryList.arraySize;i ++)
			{
				var category = categoryList.GetArrayElementAtIndex(i);
				var itemList = category.FindPropertyRelative("m_Items");
				for(int j = 0;j < itemList.arraySize;j ++)
				{
					var name = itemList.GetArrayElementAtIndex(j).FindPropertyRelative("m_Name").stringValue;
					if(name == itemName)
						return index;

					index ++;
				}
			}

			return -1;
		}

		public static void DrawListElementByName(ReorderableList list, int index, Rect rect, string nameProperty, bool isFocused, EditorWindow window)
		{
			if(list.serializedProperty.arraySize == index)
				return;

			rect.y += 2;
			var element = list.serializedProperty.GetArrayElementAtIndex(index);
			var name = element.FindPropertyRelative(nameProperty);

			name.stringValue = EditorGUI.TextField(new Rect(rect.x, rect.y, 256f, 16f), name.stringValue);

			DoListElementBehaviours(list, index, isFocused, window);
		}

		public static void DrawItemProperty(Rect rect, SerializedProperty itemProperty, ReorderableList propertyList)
		{
			var name = itemProperty.FindPropertyRelative("m_Name");

			float initialX = rect.x;

			// Source label.
			rect.width = 64f;
			rect.height = 16f;
			UnityEngine.GUI.Label(rect, "Property: ");

			// Source popup.
			var allProperties = GetStringNames(propertyList.serializedProperty, "m_Name");

			if(allProperties.Length == 0)
				return;

			rect.x = rect.xMax;
			rect.width = 128f;

			int selectedIndex = GetStringIndex(name.stringValue, allProperties);
			selectedIndex = EditorGUI.Popup(rect, selectedIndex, allProperties);
			name.stringValue = allProperties[selectedIndex];

			// Value label.
			rect.x = initialX;
			rect.width = 64f;
			rect.y = rect.yMax + 4f;

			UnityEngine.GUI.Label(rect, "Value: ");

			// Editing the value based on the type.
			rect.x = rect.xMax;

			DrawFloatProperty(rect, itemProperty.FindPropertyRelative("m_Float"));
		}

		public static string[] GetStringNames(SerializedProperty property, string subProperty = "")
		{
			List<string> strings = new List<string>();
			for(int i = 0;i < property.arraySize;i ++)
			{
				if(subProperty == "")
					strings.Add(property.GetArrayElementAtIndex(i).stringValue);
				else
					strings.Add(property.GetArrayElementAtIndex(i).FindPropertyRelative(subProperty).stringValue);
			}

			return strings.ToArray();
		}

		public static int GetStringIndex(string str, string[] strings)
		{
			for(int i = 0;i < strings.Length;i ++)
				if(strings[i] == str)
					return i;

			return 0;
		}

		private static void DrawFloatProperty(Rect position, SerializedProperty property)
		{
			var current = property.FindPropertyRelative("m_Current");
			var defaultVal = property.FindPropertyRelative("m_Default");

			current.floatValue = EditorGUI.FloatField(position, current.floatValue);
			defaultVal.floatValue = current.floatValue;
		}
	}
}