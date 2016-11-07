using UnityEngine;

public sealed class DestroyAfterDelay : MonoBehaviour
{
    [SerializeField]
    private float delay;
	
	private void Start()
    {
        Destroy(gameObject, delay);
	}
}
