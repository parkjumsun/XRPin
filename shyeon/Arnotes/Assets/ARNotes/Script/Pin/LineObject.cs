using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class LineObject
{
    [SerializeField]  public List<Vector3> Points;
    [SerializeField]  public Color Color;
    [SerializeField]  public float Width;
    

    public LineObject(List<Vector3> Points, Color Color, float Width)
    {
        this.Points = Points;
        this.Color = Color;
        this.Width = Width;
    }

    public void AddPoint(float x, float y, float z)
    {
        this.Points.Add(new Vector3(x, y, z));
    }
    public void AddPoint(Vector3 point)
    {
        this.Points.Add(point);
    }
    public List<Vector3> GetPoints()
    {
        return this.Points;
    }
}
