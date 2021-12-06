using UnityEngine;

namespace BUT.Utils
{
    public static class ComponentUtilities
    {
        public static bool HasComponent<T>(this GameObject obj)
        {
            return (obj.GetComponent<T>() as Component) != null;
        }

        public static bool HasComponent<T>(this Transform obj)
        {
            return (obj.gameObject.GetComponent<T>() as Component) != null;
        }

    }
}

