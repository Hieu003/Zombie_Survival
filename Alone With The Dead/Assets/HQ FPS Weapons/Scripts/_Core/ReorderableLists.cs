using System;
using UnityEngine;

namespace HQFPSWeapons
{
	[Serializable]
	public class BooleanList : ReorderableArray<bool> {  }

	[Serializable]
	public class IntegerList : ReorderableArray<int> {  }

	[Serializable]
	public class FloatList : ReorderableArray<float> {  }

	[Serializable]
	public class StringList : ReorderableArray<string> {  }

	[Serializable]
	public class Vector2List : ReorderableArray<Vector2> {  }

	[Serializable]
	public class Vector3List : ReorderableArray<Vector3> {  }

	[Serializable]
	public class QuaternionList : ReorderableArray<Quaternion> {  }

	[Serializable]
	public class TransformList : ReorderableArray<Transform> {  }

	[Serializable]
	public class RectTransformList : ReorderableArray<RectTransform> {  }

	[Serializable]
	public class TextureList : ReorderableArray<Texture> {  }

	[Serializable]
	public class PooledObjectList : ReorderableArray<PoolableObject> {  }

	[Serializable]
	public class AudioClipList : ReorderableArray<AudioClip> {  }

	[Serializable]
	public class ParticleSystemList : ReorderableArray<ParticleSystem> {  }

	[Serializable]
	public class ItemGeneratorList : ReorderableArray<ItemGenerator> {  }

	[Serializable]
	public class ItemDescriptionList : ReorderableArray<ItemDescription> {  }

	[Serializable]
	public class ItemPropertyList : ReorderableArray<ItemProperty.Value> {  }
}