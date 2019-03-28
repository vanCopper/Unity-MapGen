// **********************************************************************
// Author: vanCopper
// Date: 2019/3/25 16:00:03
// Desc: 
// **********************************************************************
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
/// <summary>
/// Park Miller(1988) 随机数发生器
/// </summary>
public class ParkMillerRNG
{
    /// <summary>
    /// 1-0x7FFFFFFF 不可使用0 作为Seed
    /// </summary>
    public uint Seed;

    public ParkMillerRNG()
    {
        Seed = 1;
    }

    public uint NextInt()
    {
        return Gen();
    }

    /// <summary>
    /// 0-1.0
    /// </summary>
    /// <returns></returns>
    public double NextDouble()
    {
        double gen = Gen();
        double result = gen / 2147483647;
        return result;
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="min"></param>
    /// <param name="max"></param>
    /// <returns></returns>
    public uint NextIntRange(double min, double max)
    {
        min -= .4999;
        max += .4999;
        return (uint)(min + ((max - min) * NextDouble()));
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="min"></param>
    /// <param name="max"></param>
    /// <returns></returns>
    public double NextDoubleRange(double min, double max)
    {
        double next = NextDouble();
        return min + ((max - min) * next);
    }

    private uint Gen()
    {
        //Debug.Log(Seed);
        return Seed = (Seed * 16807) % 2147483647;
    }
}
