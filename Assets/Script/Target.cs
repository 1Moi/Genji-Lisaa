using UnityEngine;

public class Target : MonoBehaviour
{
    public delegate void TargetDestroyed();
    public static event TargetDestroyed OnTargetDestroyed;

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("Projectile"))
        {
            if (OnTargetDestroyed != null)
            {
                OnTargetDestroyed();
            }

            Destroy(gameObject);
        }
    }
}
