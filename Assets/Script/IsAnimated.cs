using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class IsAnimated : MonoBehaviour
{
    public Animator myAnimationController;

    public void Dancing()
    {
        myAnimationController.SetTrigger("Dancing");
    }
}
