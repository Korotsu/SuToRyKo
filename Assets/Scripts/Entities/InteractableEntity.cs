using UnityEngine;

namespace Entities
{
    public abstract class InteractableEntity : BaseEntity, ISelectable, IDamageable, IRepairable
    {
         #region ISelectable
            public void SetSelected(bool selected)
            {
                IsSelected = selected;
                SelectedSprite?.SetActive(IsSelected);
            }
           
            #endregion
        
            #region IDamageable
            public void AddDamage(int damageAmount)
            {
                if (IsAlive == false)
                    return;
        
                HP -= damageAmount;
        
                OnHpUpdated?.Invoke();
        
                if (HP <= 0)
                {
                    IsAlive = false;
                    OnDeadEvent?.Invoke();
                    Debug.Log("Entity " + gameObject.name + " died");
                }
            }
            public void Destroy()
            {
                AddDamage(HP);
            }
            #endregion
        
            #region IRepairable
            virtual public bool NeedsRepairing()
            {
                return true;
            }
            virtual public void Repair(int amount)
            {
                OnHpUpdated?.Invoke();
            }
            virtual public void FullRepair()
            {
            }
            #endregion
    }
}