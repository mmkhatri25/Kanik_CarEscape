using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(N))]
public class NEditor : Editor
{
    public override void OnInspectorGUI()
    {

        DrawDefaultInspector();
        N m_nnn = (N)target;

        if (GUILayout.Button("Clear"))
        {
            m_nnn._Clear();
        }
        if (GUILayout.Button("Add"))
        {
            m_nnn._Up();
        }
        if (GUILayout.Button("ResetBoxColider"))
        {
            m_nnn._ResetMesh();
        }
        if (GUILayout.Button("EnableMesh"))
        {
            m_nnn._EnableMesh();
        }
        if (GUILayout.Button("DisableMesh"))
        {
            m_nnn._DisableMesh();
        }



    }
}
