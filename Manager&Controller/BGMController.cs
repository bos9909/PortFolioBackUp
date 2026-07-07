using UnityEngine;

public class BGMController : MonoBehaviour
{
    static BGMController BGMCont;


    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Awake()
    {
        if (BGMCont == null)
        {
            BGMCont = this;
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
