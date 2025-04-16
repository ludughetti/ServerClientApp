using System;
using System.Collections.Generic;
using UnityEngine;
using static Utils.Validator;

namespace Navigation
{
    public class NavigationMenu : MonoBehaviour
    {
        [Header("Menus")]
        [SerializeField] private MenuDataSource selfMenu;
        [SerializeField] private List<MenuDataSource> availableMenus;
    
        [Header("Buttons")]
        [SerializeField] private NavigationButton button;
        [SerializeField] private Transform buttonContainer;
    
        public event Action<string> OnMenuChange;
    
        private void Awake()
        {
            ValidateDependencies();
            selfMenu.DataInstance = this;
        }
        
        public void Setup()
        {
            foreach (var id in availableMenus)
            {
                var newButton = Instantiate(button, buttonContainer);
                newButton.Setup(id.GetMenuId(), HandleButtonClick);
            }
        }
        
        private void ValidateDependencies()
        {
            enabled = IsDependencyConfigured(name, "Self Menu", selfMenu) && 
                      IsDependencyConfigured(name, "Available Menus", availableMenus.Count > 0) &&
                      IsDependencyConfigured(name, "Button", button) &&
                      IsDependencyConfigured(name, "Button Container", buttonContainer);
        }
    
        private void HandleButtonClick(string id)
        {
            OnMenuChange?.Invoke(id);
        }
    }
}
