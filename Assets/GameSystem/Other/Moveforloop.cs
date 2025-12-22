using UnityEngine;

public class Moveforloop : MonoBehaviour
{
    public float speed = 3f; // Speed of movement
    public float removeDelay = 180f; // Time after which the object is removed

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        InvokeRepeating("DeleteObj", 180f, removeDelay);
    }

    // Update is called once per frame
    void Update()
    {
        transform.position += Vector3.right * speed * Time.deltaTime;
        transform.rotation = Quaternion.Euler(-90, 0, 0);
    }

    public void DeleteObj()
    {
        Destroy(gameObject);
    }


}
