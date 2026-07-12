using UnityEngine;

namespace UnityUtils {
    public static class ComponentExtensions {
        /// <summary>
        /// Gets the first component of type <typeparamref name="T"/> found in the component's hierarchy, preferring parents before children.
        /// </summary>
        /// <typeparam name="T">The component type to search for.</typeparam>
        /// <param name="component">The source component whose hierarchy is searched.</param>
        /// <param name="includeInactive">Whether inactive objects should be included in the search.</param>
        /// <returns>The first matching component found in a parent or child, excluding the source object itself in child search; otherwise null.</returns>
        /// <example>
        /// <code>
        /// AudioListener listener = someComponent.GetComponentInHierarchy&lt;AudioListener&gt;(includeInactive: true);
        /// </code>
        /// </example>
        public static T GetComponentInHierarchy<T>(this Component component, bool includeInactive = false) where T : Component {
            if (component == null) return null;

            var parentComponent = component.GetComponentInParentOnly<T>(includeInactive);
            if (parentComponent != null) {
                return parentComponent;
            }

            var hierarchyComponents = component.GetComponentsInChildren<T>(includeInactive);
            for (var i = 0; i < hierarchyComponents.Length; i++) {
                var candidate = hierarchyComponents[i];
                if (candidate.transform != component.transform) {
                    return candidate;
                }
            }

            return null;
        }

        /// <summary>
        /// Gets a component of type <typeparamref name="T"/> from the component's parent chain only.
        /// </summary>
        /// <typeparam name="T">The component type to search for.</typeparam>
        /// <param name="component">The source component.</param>
        /// <param name="includeInactive">Whether inactive parents should be included in the search.</param>
        /// <returns>The first matching parent component, or null if no match exists.</returns>
        /// <example>
        /// <code>
        /// Rigidbody parentBody = wheelCollider.GetComponentInParentOnly&lt;Rigidbody&gt;();
        /// </code>
        /// </example>
        public static T GetComponentInParentOnly<T>(this Component component, bool includeInactive = false) where T : Component {
            if (component == null) return null;

            var parent = component.transform.parent;
            return parent != null ? parent.GetComponentInParent<T>(includeInactive) : null;
        }
    }
}
