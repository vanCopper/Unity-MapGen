// **********************************************************************
// Author: vanCopper
// Date: 2019/3/25 18:04:13
// Desc: 
// **********************************************************************
using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class IsLandShape
{
    public static IsLandShapeGen MakeRadial(int seed)
    {
        IsLandShapeGen islandShapeGen = inside;
        
        bool inside(Vector2f q)
        {
            //TODO:
            return true;
        }

        return islandShapeGen;
    }

    public static IsLandShapeGen MakePerlin(int seed)
    {
        IsLandShapeGen islandShapeGen = inside;

        bool inside(Vector2f q)
        {
            //TODO:
            return true;
        }

        return islandShapeGen;
    }

    public static IsLandShapeGen MakeSquare(int seed)
    {
        IsLandShapeGen isLandShapeGen = inside;
        
        bool inside(Vector2f q)
        {
            //TODO:
            return true;
        }

        return isLandShapeGen;
    }


}
