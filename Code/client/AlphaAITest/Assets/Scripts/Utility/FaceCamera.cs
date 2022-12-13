using UnityEngine;
using System.Collections;

public class FaceCamera : MonoBehaviour
{
    // Use this for initialization
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
        // Rotate the camera every frame so it keeps looking at the target
        //transform.LookAt(Camera.main.transform);

        // Same as above, but setting the worldUp parameter to Vector3.left in this example turns the camera on its side
        transform.LookAt(Camera.main.transform, Vector3.up);

        // then lock rotation to Y axis only...
        transform.localEulerAngles = new Vector3(0, transform.localEulerAngles.y, 0);
    }
}
