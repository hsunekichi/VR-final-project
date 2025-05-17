using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using GlobalVars;
using UnityEngine.SceneManagement;


public class Die : MonoBehaviour
{
    public UIController arrowUI;
    void Start()
    {
        arrowUI = GameObject.Find("Canvas").GetComponent<UIController>();
        global.HP = gameObject.GetComponent<Damageable>().HP;
        arrowUI.SetHeartCount();
    }
    public void change()
    {
        if (gameObject.GetComponent<Damageable>().HP <= 0) SceneManager.LoadScene("Game_over", LoadSceneMode.Single);
        else
        {
            global.HP = gameObject.GetComponent<Damageable>().HP;
            arrowUI.SetHeartCount();
        }
    }
}
