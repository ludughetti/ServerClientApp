using System;
using UnityEngine;

namespace Navigation
{
    [CreateAssetMenu(fileName = "MenuDataSource", menuName = "Scriptable Objects/MenuDataSource")]
    public class MenuDataSource : ScriptableObject
    {
        [SerializeField] private string menuId;
    
        public NavigationMenu DataInstance { get; set; }

        public String GetMenuId()
        {
            return menuId;
        }
    }
}
