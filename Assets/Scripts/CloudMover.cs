using UnityEngine;
using UnityEngine.Rendering.Universal;

public class CloudMover : MonoBehaviour
{
    public float speed = 1f; // Speed at which the cloud cookie moves
    public Vector2 direction = Vector2.left; // Direction of movement (left by default)
    public Vector2 initialPosition; // Initial position of the cookie
    private UniversalAdditionalLightData urpLightData;

    void Start()
    {
        urpLightData = GetComponent<UniversalAdditionalLightData>();
        urpLightData.lightCookieOffset = initialPosition;
    }

    // Update is called once per frame
    void Update()
    {
        // Move the cookie in the specified direction
        initialPosition += direction * (speed * Time.deltaTime);
        urpLightData.lightCookieOffset = initialPosition;
    }
}
