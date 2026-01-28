using UnityEngine;

[DefaultExecutionOrder(10)] // Ensures this runs AFTER CameraFollow (Default 0) in LateUpdate
public class ParallaxBackground : MonoBehaviour
{
    [Header("Settings")]
    public Vector2 parallaxEffectMultiplier = new Vector2(0.5f, 0.5f); // (0,0) = static, (1,1) = moves with cam
    public bool infiniteHorizontal = true;
    public bool infiniteVertical = false;

    private Transform cameraTransform;
    private CameraFollow cameraFollow;
    private Vector3 lastCameraPosition;
    private float textureUnitSizeX;
    private float textureUnitSizeY;

    void Start()
    {
        if (Camera.main != null)
        {
            cameraTransform = Camera.main.transform;
            cameraFollow = cameraTransform.GetComponent<CameraFollow>();
            
            // Use logical position if available to avoid shake artifacts on start
            if (cameraFollow != null)
                lastCameraPosition = cameraFollow.GetLogicalPosition();
            else
                lastCameraPosition = cameraTransform.position;
        }

        SpriteRenderer sprite = GetComponent<SpriteRenderer>();
        if (sprite != null)
        {
            Texture2D texture = sprite.sprite.texture;
            textureUnitSizeX = texture.width / sprite.sprite.pixelsPerUnit;
            textureUnitSizeY = texture.height / sprite.sprite.pixelsPerUnit;
        }
    }

    void LateUpdate()
    {
        if (cameraTransform == null) return;

        Vector3 currentCameraPosition = (cameraFollow != null) ? cameraFollow.GetLogicalPosition() : cameraTransform.position;
        Vector3 deltaMovement = currentCameraPosition - lastCameraPosition;
        
        // Move the layer by a fraction of the camera movement
        transform.position += new Vector3(
            deltaMovement.x * parallaxEffectMultiplier.x, 
            deltaMovement.y * parallaxEffectMultiplier.y, 
            0);
            
        lastCameraPosition = currentCameraPosition;

        if (infiniteHorizontal && textureUnitSizeX > 0)
        {
            float distCheck = Mathf.Abs(currentCameraPosition.x - transform.position.x);
            
            if (distCheck >= textureUnitSizeX)
            {
                float offsetPositionX = (currentCameraPosition.x - transform.position.x) % textureUnitSizeX;
                transform.position = new Vector3(currentCameraPosition.x + offsetPositionX, transform.position.y);
            }
        }
        
        if (infiniteVertical && textureUnitSizeY > 0)
        {
            float distCheck = Mathf.Abs(currentCameraPosition.y - transform.position.y);

            if (distCheck >= textureUnitSizeY)
            {
                float offsetPositionY = (currentCameraPosition.y - transform.position.y) % textureUnitSizeY;
                transform.position = new Vector3(transform.position.x, currentCameraPosition.y + offsetPositionY);
            }
        }
    }
}
