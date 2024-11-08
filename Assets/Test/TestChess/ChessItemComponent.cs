using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ChessItemComponent : MonoBehaviour
{
    public HoneycombVector2 Point;

    public static Color DefaultColor = Color.gray;
    public static Color StartColor = Color.green;
    public static Color EndColor = Color.red;

    public static Color PathColor = Color.yellow;
    public static Color ObstacleColor = Color.blue;
    // Start is called before the first frame update
    void Start()
    {
        SetColor(DefaultColor);
    }

    public void SetColor(Color color)
    {
        GetComponent<MeshRenderer>().material.color = color;
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
