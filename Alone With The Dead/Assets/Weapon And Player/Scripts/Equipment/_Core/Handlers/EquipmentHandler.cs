using UnityEngine;
using System;
using System.Collections.Generic;

namespace HQFPSWeapons
{
    public class EquipmentHandler : PlayerComponent
    {
        #region Internal
        #pragma warning disable 0649

        [Serializable]
        protected struct BaseSettings
        {
            public Animator AnimController;

            public Transform AccesoryParent;

            [Space]

            public SkinnedMeshRenderer Item;
            public SkinnedMeshRenderer RightArm;
            public SkinnedMeshRenderer LeftArm;
        }

        [Serializable]
        public struct EquipmentSettings
        {
            public Vector3 OriginalLightPosition { get; set; }

            public Transform Armature;

            public Transform Muzzle;
            public Transform CasingEjection;
            public Transform MagazineEjection;

            public Transform[] BulletBones;

            [Space]

            public LightEffect LightEffect;
        }

        #pragma warning restore 0649
        #endregion

        //Events
        public Message<bool> OnSelected = new Message<bool>();
        public Message<bool, bool> ItemUsed = new Message<bool, bool>();
        public Message<bool> Reloading = new Message<bool>();
        public Activity UsingItem = new Activity();

        //Properties
        public EquipmentPhysicsHandler PhysicsHandler { get { return m_ItemPhysicsHandler; } private set { } }
        public EquipmentManager EquipmentManager { get { return m_EquipmentManager; } private set { } }
        public SaveableItem CurrentlyAttachedItem { get { return m_CurrentlyAttachedItem; } private set { } }
        public EquipmentItem CurrentItem { get { return m_CurrentItem; } private set { } }
        public GameObject ItemModelTransform { get => m_BaseSettings.Item.gameObject; private set { } }
        public Transform ItemTransform { get { return m_CurrentItem != null ? m_CurrentItem.transform : transform; } private set { } }
        public Animator Animator { get { return m_BaseSettings.AnimController; } private set { } }
        public float LastChangeItemTime { get; private set; }
        public EquipmentSettings EquipSettings => m_EquipmentSettings;


        [SerializeField]
        [Group]
        protected BaseSettings m_BaseSettings = new BaseSettings();

        [SerializeField]
        [Group]
        private EquipmentSettings m_EquipmentSettings = new EquipmentSettings();

        protected GameObject m_CurrentAccesory;

        protected EquipmentItem m_CurrentItem;
        private SaveableItem m_CurrentlyAttachedItem;

        protected AudioSource m_AudioSource;

        private List<QueuedSound> m_QueuedSounds = new List<QueuedSound>();
        private List<QueuedCameraForce> m_QueuedCamForces = new List<QueuedCameraForce>();

        private EquipmentPhysicsHandler m_ItemPhysicsHandler;
        private EquipmentManager m_EquipmentManager;


        public void ClearDelayedSounds() { m_QueuedSounds.Clear(); }
        public void ClearDelayedCamForces() { m_QueuedCamForces.Clear(); }

        public void UpdateFirstPersonArms(int armIndex)
        {
            FirstPersonArmsData fpArms = FirstPersonArms.Default.GetFirstPersonArms(armIndex);

            m_BaseSettings.LeftArm.materials = fpArms.LeftArm.sharedMaterials;
            m_BaseSettings.RightArm.materials = fpArms.RightArm.sharedMaterials;
            m_BaseSettings.LeftArm.sharedMesh = fpArms.LeftArm.sharedMesh;
            m_BaseSettings.RightArm.sharedMesh = fpArms.RightArm.sharedMesh;

            if (m_BaseSettings.AccesoryParent != null)
            {
                if (fpArms.AccesoryModel != null)
                    m_CurrentAccesory = Instantiate(fpArms.AccesoryModel, m_BaseSettings.AccesoryParent);
                else
                    Destroy(m_CurrentAccesory);
            }
        }

        public void PlayAudio(SoundPlayer soundPlayer, float volume, ItemSelection.Method selectionMethod = ItemSelection.Method.RandomExcludeLast)
        {
            soundPlayer.Play(selectionMethod, m_AudioSource, volume);
        }

        public bool TryUseOnce(Camera camera)
        {
            bool usedSuccessfully = m_CurrentItem.TryUseOnce(camera);

            if (usedSuccessfully)
            {
                UsingItem.ForceStart();
                CurrentItem.OnUseStart();
            }

            ItemUsed.Send(usedSuccessfully, false);

            return usedSuccessfully;
        }

        public bool TryUseContinuously(Camera camera)
        {
            bool usedSuccessfully = m_CurrentItem.TryUseContinuously(camera);

            if (usedSuccessfully)
            {         
                UsingItem.ForceStart();
                CurrentItem.OnUseStart();
            }

            ItemUsed.Send(usedSuccessfully, true);

            return usedSuccessfully;
        }

        public virtual void WieldItem(SaveableItem item)
        {
            //Destroy the previous item
            if (m_CurrentItem != null)
                Destroy(m_CurrentItem.gameObject);

            if (item != null)
                InstantiateItem(item.Name);
            else
                InstantiateItem("");

            m_QueuedSounds.Clear();
            m_QueuedCamForces.Clear();

            LastChangeItemTime = Time.time;

            m_CurrentlyAttachedItem = item;

            if (m_CurrentItem != null)
                m_CurrentItem.Wield(item);

            if(!ItemModelTransform.activeSelf)
                ItemModelTransform.SetActive(true);

            OnSelected.Send(true);
        }

        public void UnwieldItem()
        {
            m_CurrentItem.Unwield();
            m_CurrentlyAttachedItem = null;

            OnSelected.Send(false);
        }

        public void PlayDelayedSound(DelayedSound delayedSound)
        {
            m_QueuedSounds.Add(new QueuedSound(delayedSound, Time.time + delayedSound.GetDelay()));
        }

        public void PlaySounds(DelayedSound[] clipsData)
        {
            for (int i = 0; i < clipsData.Length; i++)
                PlayDelayedSound(clipsData[i]);
        }

        public void PlayCameraForce(DelayedCameraForce delayedCamForce)
        {
            m_QueuedCamForces.Add(new QueuedCameraForce(delayedCamForce, Time.time + delayedCamForce.Delay));
        }

        public void PlayCameraForces(DelayedCameraForce[] delayedForces)
        {
            for (int i = 0; i < delayedForces.Length; i++)
                PlayCameraForce(delayedForces[i]);
        }

        protected virtual void InstantiateItem(string itemName)
        {
            //Instantiate the new item
            FPItem newItem;
            newItem = FPItemDatabase.Default.GetFPItemData(itemName);
            FPItem.EquipmentSkin equipmentSkin = newItem.GetEquipmentSkin(0);

            if (newItem.GetEquipmentSkin(0) != null)
            {
                m_BaseSettings.Item.sharedMesh = equipmentSkin.FPMesh.sharedMesh;
                m_BaseSettings.Item.sharedMaterials = equipmentSkin.FPMaterials;
            }

            var newEquipmentItem = Instantiate(newItem.FPItemData, transform);

            if (newEquipmentItem.TryGetComponent(out m_CurrentItem))
            {
                m_CurrentItem.GetComponent<EquipmentAnimation>().AssignAnimations(m_BaseSettings.AnimController);

                FPEquipmentComponent[] fpComponents = m_CurrentItem.GetComponents<FPEquipmentComponent>();
                foreach (var comp in fpComponents)
                {
                    comp.Initialize(this);
                }
            }
        }

        protected virtual void Awake()
        {
            m_EquipmentManager = GetComponentInParent<EquipmentManager>();

            //Equipment Items AudioSource (For Overall first person items audio)
            m_AudioSource = AudioUtils.CreateAudioSource("Audio Source", transform, Vector3.zero, false, 1f, 2.5f);
            m_AudioSource.bypassEffects = m_AudioSource.bypassListenerEffects = m_AudioSource.bypassReverbZones = false;
            m_AudioSource.minDistance = 1f;
            m_AudioSource.maxDistance = 500f;

            if (m_BaseSettings.LeftArm == null || m_BaseSettings.RightArm == null)
            {
                Debug.LogWarning(gameObject.name + " is missing the arm model references, assign them in the inspector");
            }

            m_ItemPhysicsHandler = GetComponent<EquipmentPhysicsHandler>();

            Player.Inventory.ContainerChanged.AddListener(OnInventoryChanged);
            m_EquipmentSettings.OriginalLightPosition = m_EquipmentSettings.LightEffect.transform.localPosition;
        }

        private void Update()
        {
            for (int i = 0; i < m_QueuedSounds.Count; i++)
            {
                if (Time.time >= m_QueuedSounds[i].PlayTime)
                {
                    m_QueuedSounds[i].DelayedSound.Sound.Play(ItemSelection.Method.RandomExcludeLast, m_AudioSource);
                    m_QueuedSounds.RemoveAt(i);
                }
            }

            for (int i = 0; i < m_QueuedCamForces.Count; i++)
            {
                if (Time.time >= m_QueuedCamForces[i].PlayTime)
                {
                    var force = m_QueuedCamForces[i].DelayedForce.Force;
                    Player.Camera.AddRotationForce(force.Force, force.Distribution);
                    m_QueuedCamForces.RemoveAt(i);
                }
            }

            //Stop the UsingItem activity after a few miliseconds from being used (e.g. this will not stop the activity if an item being used continuously)
            if (Player.UseOnce.LastExecutionTime + Mathf.Clamp(m_CurrentItem.GetTimeBetweenUses() * 2f, 0f, 0.3f) < Time.time &&
                Player.UseContinuously.LastExecutionTime + Mathf.Clamp(m_CurrentItem.GetTimeBetweenUses() * 2f, 0f, 0.3f) < Time.time &&
                UsingItem.Active)
            {
                UsingItem.ForceStop();
                m_CurrentItem.OnUseEnd();
            }
        }

        private void OnInventoryChanged()
        {
            if (m_CurrentItem != null && m_CurrentItem.NeedsAmmoToUse)
            {
                // Recalculate ammo and update the UI
                m_CurrentItem.UpdateAmmoInfo();
            }
        }
    }
}
