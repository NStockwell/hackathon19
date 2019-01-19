using UnityEngine;

public class RemoveGemFXController : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        Destroy(gameObject, 2.0f);
    }
}
