using UnityEngine;

namespace Utils
{
    public static class Validator
    {
        public static bool IsDependencyConfigured(string componentName, string objectName, bool isDependencyConfigured)
        {
            if (isDependencyConfigured) 
                return true;
            
            Debug.LogError($"{componentName}: {objectName} is not configured properly.\nDisabling component.");
            return false;
        }
    }
}
