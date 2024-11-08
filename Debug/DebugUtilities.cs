using System.Collections;
using UnityEngine;

public static class DebugUtilities
{
    /// <summary>
    /// Draw primitive forms like capsule, cubes ecc... for a N time in a location.
    /// (similar to Debug.DrawRay)
    /// </summary>
    public static void DrawPrimitive(Vector3 position, float scale, PrimitiveType primitiveType, Color color, float time = 1f)
    {
#if UNITY_EDITOR
        GameObject sphere = GameObject.CreatePrimitive(primitiveType);
        sphere.transform.localScale = new Vector3(scale, scale, scale);
        sphere.transform.position = position;
        sphere.GetComponent<Renderer>().material.color = color;
        sphere.AddComponent<DestroySelf>();
#endif
    }
}

public class DestroySelf : MonoBehaviour
{
    public void Start()
    {
        StartCoroutine(SelfDestruct());
    }

    private IEnumerator SelfDestruct()
    {
        yield return new WaitForSeconds(5f);
        Destroy(gameObject);
    }
}