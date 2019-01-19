using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum GemType : int
{
    INVALID = 0, Basic, Empty, COUNT
};

public enum GemColor : int
{
    INVALID = 0, Blue, Red, Green, Purple, Yellow, Empty, COUNT
}

public class Gem : MonoBehaviour
{
    public Vector2Int index;
    public GemTypeSO gemTypeSO;
    public float speed = 3.0f;
    private Vector3 defaultTarget = new Vector3(0, 0, -1);
    public Vector3 target;
    private bool reachedTarget = false;
    // Start is called before the first frame update
    void Start()
    {
        target = defaultTarget;
    }

    // Update is called once per frame
    void Update()
    {
        if (target != defaultTarget)
        {
            float step = speed * Time.deltaTime;

            // Move our position a step closer to the target.
            transform.position = Vector3.MoveTowards(transform.position, target, step);

            if((transform.position - target).magnitude < Mathf.Epsilon)
            {
                reachedTarget = true;
                target = defaultTarget;
            }
        }
    }

    public bool CanBeMatchedWith(GemColor gemColor) { return gemColor == gemTypeSO.GemColor; }

    public Vector2Int GetIndex() { return index; }
    public void SetIndex(Vector2Int index) { this.index = index; }
    public void SetType(GemTypeSO gemScriptableObject) {
        gemTypeSO = gemScriptableObject;
        gameObject.GetComponent<SpriteRenderer>().sprite = gemScriptableObject.Sprite;
    }
    public void SetTarget(Vector3 target)
    {
        reachedTarget = false;
        this.target = target; 
    }
    public bool GetReachedTarget() { return reachedTarget; }
}
