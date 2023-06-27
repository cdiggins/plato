using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

public class Cloner : MonoBehaviour
{
    public int Count;
}

public class GridCloner : Cloner
{
    public int Rows = 1;
    public int Columns = 1;
    public int Levels = 1;

    public float RowSpacing = 1;
    public float ColSpacing = 1;
    public float LevelSpacing = 1;
}

public class CloneRenderer : MonoBehaviour
{
}

public class Effector : MonoBehaviour
{
}

public class Deformer : MonoBehaviour
{
}
