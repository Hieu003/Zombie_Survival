namespace HQFPSWeapons
{
    /// <summary>
    /// 
    /// </summary>
    public class FPEquipmentComponent : PlayerComponent
    {
        protected EquipmentHandler m_EHandler;
        

        public virtual void Initialize(EquipmentHandler equipmentHandler)
        {
            m_EHandler = equipmentHandler;
        }
    }
}