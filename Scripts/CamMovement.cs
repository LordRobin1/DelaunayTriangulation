using System.Text;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CamMovement : MonoBehaviour
{
    int speed = 5;

    //speed is dynamically changed with a slider in unity
    public void changeSpeed (float value) {
        speed = (int)value;
    }

    void Update() {
        transform.Translate(0, 0, Input.GetAxis("Vertical") * speed * Time.deltaTime);
    }
}
