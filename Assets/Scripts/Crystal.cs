using UnityEngine;

public class Crystal : MonoBehaviour
{
    private bool collected;

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (collected || !other.CompareTag("Player"))
            return;

        collected = true;

        GameManager.Instance.CollectCrystal();

        Destroy(gameObject);
    }
}