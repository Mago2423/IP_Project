using UnityEngine;

public class ClickMoveIndicator : MonoBehaviour
{
    [SerializeField] private float yOffset = 0.05f;
    [SerializeField] private float visibleTime = 0.5f;
    [SerializeField] private bool alignToSurface = true;
    [SerializeField] private float settleScaleSpeed = 14f;
    [SerializeField] private float pulseScale = 1.2f;

    private float hideAtTime;
    private Vector3 baseScale;

    void Awake()
    {
        baseScale = transform.localScale;
        gameObject.SetActive(false);
    }

    void Update()
    {
        if (!gameObject.activeSelf)
        {
            return;
        }

        float t = 1f - Mathf.Exp(-settleScaleSpeed * Time.deltaTime);
        transform.localScale = Vector3.Lerp(transform.localScale, baseScale, t);

        if (Time.time >= hideAtTime)
        {
            gameObject.SetActive(false);
        }
    }

    public void Show(Vector3 worldPoint, Vector3 surfaceNormal)
    {
        transform.position = worldPoint + surfaceNormal.normalized * yOffset;

        if (alignToSurface)
        {
            transform.up = surfaceNormal;
        }

        transform.localScale = baseScale * pulseScale;

        if (!gameObject.activeSelf)
        {
            gameObject.SetActive(true);
        }

        hideAtTime = Time.time + visibleTime;
    }
}
