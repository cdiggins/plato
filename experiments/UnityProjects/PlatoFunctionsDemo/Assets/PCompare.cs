using System;
using UnityEngine;

public class PCompare : MonoBehaviour
{
    public enum ComparisonType
    {
        LessThan,
        LessThanOrEqual,
        EqualTo,
        GreaterThanOrEqualTo,
        GreaterThan,
        NotEqualTo,
    }

    // TODO: this needs to be a property of a value. 
    // I should be able to write ".x" to get the ".x" component of a vector3.
    // FOr any object I should be able to write ".position.x" 
    // This should be a pull down. 
    public float SourceValue;

    // TODO: this would be fun to also be a property 
    public float CompareTo;

    public bool Result;
    public float Difference;
    public ComparisonType Comparison;

    public void Update()
    {
        Difference = CompareTo - SourceValue;
        switch (Comparison)
        {
            case ComparisonType.LessThan:
                Result = Difference < 0;
                break;
            case ComparisonType.LessThanOrEqual:
                Result = Difference <= 0;
                break;
            case ComparisonType.EqualTo:
                Result = Math.Abs(Difference) <= float.Epsilon;
                break;
            case ComparisonType.GreaterThanOrEqualTo:
                Result = Difference >= 0;
                break;
            case ComparisonType.GreaterThan:
                Result = Difference > 0;
                break;
            case ComparisonType.NotEqualTo:
                Result = Math.Abs(Difference) > float.Epsilon;
                break;
        }
    }
}