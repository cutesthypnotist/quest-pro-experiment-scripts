using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public static class DataUtilities
{
    public static string EncodeVector3CSV(Vector3 v)
    {
        return v.x + "|" + v.y + "|" + v.z;
    }
    public static string EncodeVector2CSV(Vector2 v)
    {
        return v.x + "|" + v.y;
    }
    public static string EncodeQuaternionCSV(Quaternion q)
    {
        return q.x + "|" + q.y + "|" + q.z + "|" + q.w;
    }
}

