using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DeactivateArrows : MonoBehaviour
{
    [SerializeField]
    GameObject grab;
    
    public void activateObject()
    {
        grab.SetActive(true);
    }

    public void deactivateObject()
    {
        grab.SetActive(false);
    }

}
