using UnityEngine;

namespace HQFPSWeapons
{
	public class DebrisFader : MonoBehaviour 
	{
		[SerializeField]
		private bool m_StartAutomatically = true;

		[SerializeField]
		[Clamp(0.01f, 30f)]
		private float m_FadeDuration = 10f;

		[SerializeField]
		private Material m_FadeMaterial = null;

		[SerializeField]
		private AnimationCurve m_FadeCurve = AnimationCurve.EaseInOut(0f, 1f, 1f, 0f);

		private float m_DestroyTime;
		private MeshRenderer[] m_Renderers;
		private Material m_SharedMaterial;

		private bool m_StartedFading;


		public void StartFading()
		{
			m_DestroyTime = Time.time + m_FadeDuration;
			m_StartedFading = true;
		}

		private void Awake()
		{
			m_Renderers = GetComponentsInChildren<MeshRenderer>(true);

			if(m_Renderers.Length > 0)
			{
				m_SharedMaterial = new Material(m_FadeMaterial);
				m_SharedMaterial.name = m_FadeMaterial.name + " - Clone";

				for(int i = 0;i < m_Renderers.Length;i ++)
					m_Renderers[i].sharedMaterial = m_SharedMaterial;
			}

			#if UNITY_EDITOR
			UnityEditor.MaterialEditor.ApplyMaterialPropertyDrawers(m_SharedMaterial);
			#endif
		}

		private void Start()
		{
			if(m_StartAutomatically)
				StartFading();
		}

		private void Update()
		{
			if(!m_StartedFading)
				return;

			if(m_Renderers.Length == 0 || Time.time >= m_DestroyTime)
			{
				Destroy(gameObject);
				return;
			}

			float alpha = 1f - (m_DestroyTime - Time.time) / m_FadeDuration;
			m_SharedMaterial.color = new Color(m_SharedMaterial.color.r, m_SharedMaterial.color.g, m_SharedMaterial.color.b, m_FadeCurve.Evaluate(alpha));
		}
	}
}
