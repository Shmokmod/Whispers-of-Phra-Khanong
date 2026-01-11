using UnityEngine;

public class NPCWalkLoop : MonoBehaviour
{
    public float speed = 3f; // Speed of movement
    public float removeDelay = 180f; // Time after which the object is removed
    public Vector3 direction = Vector3.right; // Direction of movement

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        InvokeRepeating("DeleteObj", 180f, removeDelay);
    }

    // Update is called once per frame
    void Update()
    {
        transform.position += direction * speed * Time.deltaTime;
    }

    public void DeleteObj()
    {
        Destroy(gameObject);
    }


}
