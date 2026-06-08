using UnityEngine;

// Put on the shared Main Camera prefab. Applies one skybox material to every
// scene from a single place (RenderSettings.skybox is otherwise per-scene), and
// makes sure the camera clears to the skybox rather than a flat colour.
[RequireComponent(typeof(Camera))]
public class SkyboxSetup : MonoBehaviour
{
    [SerializeField] Material skyboxMaterial;   // assign CloudyCrown_Midday here

    void Awake()
    {
        if (skyboxMaterial != null) RenderSettings.skybox = skyboxMaterial;
        GetComponent<Camera>().clearFlags = CameraClearFlags.Skybox;
    }
}