using UnityEngine;

public class Brush : MonoBehaviour
{
    [SerializeField] private float brushSize;
    [SerializeField] private float brushStrength;
    [SerializeField] private float brushDistance;
    [SerializeField] private ChunkManager chunkManager;
    [SerializeField] private Camera mainCamera;

    private void Update()
    {
        if (Input.GetKey(KeyCode.Mouse0) || Input.GetKey(KeyCode.Mouse1))
        {
            if (Physics.Raycast(mainCamera.ScreenPointToRay(Input.mousePosition), out var hit, brushDistance))
            {
                var sign = Input.GetKey(KeyCode.Mouse0) ? 1 : -1;
                chunkManager.AddValue(hit.point, brushSize, brushStrength * sign * Time.deltaTime);
            }
        }
    }
}
