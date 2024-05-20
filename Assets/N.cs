using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class N : MonoBehaviour
{

    public List<GameObject> list;
    [Space]
    public GameObject m_obj;

    public void _Up()
    {
        Debug.Log("UP");
        list = new List<GameObject>();
        m_obj = GameObject.Find("EscapeTriggers");

        foreach (Transform go in m_obj.transform)
        {
            list.Add(go.gameObject);
        }
    }

    public void _ResetMesh()
    {
        foreach (GameObject go in list)
        {
            go.GetComponent<BoxCollider>().size = new Vector3(1f, 1f, 1f);
        }
    }

    public void _EnableMesh()
    {
        foreach (GameObject go in list)
        {
            go.GetComponent<MeshRenderer>().enabled = true;
        }
    }

    public void _DisableMesh()
    {
        foreach (GameObject go in list)
        {
            go.GetComponent<MeshRenderer>().enabled = false;
        }
    }

    public void _Clear()
    {
        list = new List<GameObject>();
        m_obj = null;
    }
}
