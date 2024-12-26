using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using HQFPSWeapons.UserInterface;
using UnityEngine.UI;

namespace HQFPSWeapons
{
	public class GameManager : Singleton<GameManager>
	{
		public static GameManager instance;

		public int highScore;

		public int currentScore;

		public Text currentScoreText;

		public Material[] PreloadedMaterials { get { return m_PreloadedMaterials; } set { m_PreloadedMaterials = value; } }
		public Player CurrentPlayer { get; private set; }
		public UIManager CurrentInterface { get; private set; }

		[BHeader("General", true)]

		[SerializeField]
		private SceneField[] m_GameScenes = null;

		[Space]

		[SerializeField]
		private Texture2D m_CustomCursorTex = null;

		[Space]

		[SerializeField]
		[Tooltip("This will help with stuttering and lag when loading new objects for the first time, but will increase the memory usage right away.")]
		private bool m_PreloadMaterialsInEditor = false;

		[SerializeField]
		private Material[] m_PreloadedMaterials = null;


		public void Quit()
		{
			Application.Quit();
		}

		/// <summary> Starting the game with an index of -1 is going to reload the current scene</summary>
		public void StartGame(int index = -1)
		{
			Cursor.visible = false;
			Cursor.lockState = CursorLockMode.Locked;

			// Load the game scene
			if (index == -1)
				SceneManager.LoadSceneAsync(SceneManager.GetActiveScene().name, LoadSceneMode.Single);
			else
				SceneManager.LoadSceneAsync(m_GameScenes[index].SceneName, LoadSceneMode.Single);
		}

		public void SetPlayerPosition()
		{
			//Set the position and rotation with the random spawn point transform
			CurrentPlayer.transform.position = GetSpawnPoint();
			CurrentPlayer.transform.rotation = GetSpawnRotation();
		}

		private Vector3 GetSpawnPoint()
		{
			//get a random spawn point
			var spawnPoints = FindObjectOfType<PlayerSpawnPoints>();
			Vector3 spawnPoint = CurrentPlayer.transform.position;

			if (spawnPoints != null)
			{
				var newSpawnPoint = spawnPoints.GetRandomSpawnPoint();

				if (newSpawnPoint != Vector3.zero)
					spawnPoint = newSpawnPoint;
			}

			return spawnPoint;
		}

		private Quaternion GetSpawnRotation()
		{
			var spawnPoints = FindObjectOfType<PlayerSpawnPoints>();
			Quaternion spawnRotation = CurrentPlayer.transform.rotation;

			if (spawnPoints != null)
				spawnRotation = spawnPoints.GetRandomRotation();

			return spawnRotation;
		}

		private void OnEnable()
		{
			SceneManager.sceneLoaded += OnSceneLoaded;
		}

		private void OnDestroy()
		{
			SceneManager.sceneLoaded -= OnSceneLoaded;
		}

		private void Awake()
		{
			if (Instance != null && Instance != this)
			{
				Destroy(gameObject);
				return;
			}

			if(Application.isEditor && m_PreloadMaterialsInEditor)
			{
				List<GameObject> preloadObjects = new List<GameObject>();

				Camera camera = new GameObject("Material Preload Camera", typeof(Camera)).GetComponent<Camera>();
				camera.orthographic = true;
				camera.orthographicSize = 100f;
				camera.farClipPlane = 100f;
				camera.depth = 999;
				camera.renderingPath = RenderingPath.Forward;
				camera.useOcclusionCulling = camera.allowHDR = camera.allowMSAA = camera.allowDynamicResolution = false;

				preloadObjects.Add(camera.gameObject);

				foreach(var mat in m_PreloadedMaterials)
				{
					if(mat == null)
						continue;

					var quad = GameObject.CreatePrimitive(PrimitiveType.Quad);
					quad.transform.position = camera.transform.position + camera.transform.forward * 50f + camera.transform.right * UnityEngine.Random.Range(-100f, 100f) + camera.transform.up * UnityEngine.Random.Range(-100f, 100f);
					quad.transform.localScale = Vector3.one * 0.01f;

					quad.GetComponent<Renderer>().sharedMaterial = mat;

					preloadObjects.Add(quad);
				}

				camera.Render();

				foreach(var obj in preloadObjects)
					Destroy(obj);

				preloadObjects.Clear();
			}

			if(m_CustomCursorTex != null)
				Cursor.SetCursor(m_CustomCursorTex, Vector2.zero, CursorMode.Auto);

			DontDestroyOnLoad(gameObject);
		}

		private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
		{
			CurrentPlayer = FindObjectOfType<Player>();
			CurrentInterface = FindObjectOfType<UIManager>();

			CurrentInterface.AttachToPlayer(CurrentPlayer);
		}

		private void Start()
		{
			instance = this;
			Shader.WarmupAllShaders();
			GC.Collect();
		}

        private void Update()
        {
            if(currentScore > highScore)
			{
				highScore = currentScore;
			}

			currentScoreText.text = currentScore.ToString();
        }
    }
}