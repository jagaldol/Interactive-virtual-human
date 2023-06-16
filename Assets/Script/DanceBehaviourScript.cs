using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DanceBehaviourScript : MonoBehaviour
{
//Attach this script to a GameObject with an Animator component attached.
//For this example, create parameters in the Animator and name them ¡°Crouch¡± and ¡°Jump¡±
//Apply these parameters to your transitions between states

//This script allows you to trigger an Animator parameter and reset the other that could possibly still be active. Press the up and down arrow keys to do this.
    Animator m_Animator;

    void Start()
    {
        //Get the Animator attached to the GameObject you are intending to animate.
        m_Animator = gameObject.GetComponent<Animator>();
    }

    void Update()
    {
        if (Input.GetKey(KeyCode.DownArrow))
        {
            dancing();
        }
    }

    public void dancing()
    {
        m_Animator.SetTrigger("Dancing");
    }
}
