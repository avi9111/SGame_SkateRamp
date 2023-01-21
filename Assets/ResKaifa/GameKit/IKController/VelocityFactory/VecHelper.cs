using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class VecHelper : SpecSingleton<VecHelper>
{
    public Vector3? GetGroundPosition(Vector3 source)
    {
        RaycastHit hit;
        if(Physics.Raycast(source,Vector3.down,out hit,10))
        {
            return hit.point;
        }

        return null;
    }
    
    
}
