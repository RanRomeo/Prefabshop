﻿using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace Packages.PrefabshopEditor
{
    [BrushKeyCode(KeyCode.F)]
    public class FillTool : Tool
    {
        public RaycastHit raycastHit;

        GameObject go;

        public FillTool(BrushInfo into) : base(into)
        {
            AddParameter(new Count());
            AddParameter(new Scale());
            AddParameter(new Gap());
            AddParameter(new Tag());
            AddParameter(new Layer());
            AddParameter(new Parent());
            AddParameter(new IgnoringLayer());
            AddParameter(new ListOfObjects());
        }

        public override void SelectTool()
        {
            base.SelectTool();
            go = HandleUtility.PickGameObject(Event.current.mousePosition, false);
            Selection.activeGameObject = go;
        }

        public override void DeselectTool()
        {
            base.DeselectTool();
            Selection.activeGameObject = null;
        }

        public override void DrawHandle(Ray ray)
        {
            base.DrawHandle(ray);
            //var casts = Physics.RaycastAll(ray, Mathf.Infinity, ~(GetParameter<IgnoringLayer>().value));
            //var raycastHit = casts[casts.Length - 1];
            //Handles.color = new Color(1, 0, 0, 0.25f);
            //Handles.SphereHandleCap(0, raycastHit.point, Quaternion.identity, GetParameter<Radius>().value * 2, EventType.Repaint);
            //Handles.color = Color.white;
            //if (go == null)
            //{
            if (Event.current.type == EventType.MouseMove)
            {
                go = HandleUtility.PickGameObject(Event.current.mousePosition, false);

            }
            if (go != null)
            {
                var shape = go.GetComponentInChildren<MeshFilter>().sharedMesh;
                var position = go.transform.position;
                var rotation = go.transform.rotation;
                var scale = go.transform.lossyScale;


                var matrix = new Matrix4x4();
                matrix.SetTRS(position, rotation, scale);
                var mat = new Material(Shader.Find("Raptorij/BrushShape"));
                mat.SetColor("_Color", new Color(0, 1, 0, 0.25f));
                mat.SetPass(0);
                Graphics.DrawMeshNow(shape, matrix, 0);
            }
            //}
            //Selection.activeGameObject = go;
        }

        

        public override void Paint(RaycastHit drawPointHit)
        {
            base.Paint(drawPointHit);

            var castRay = HandleUtility.GUIPointToWorldRay(Event.current.mousePosition);
            RaycastHit raycast;
            Physics.Raycast(castRay, out raycast, Mathf.Infinity, ~(GetParameter<IgnoringLayer>().value));
            var objectWithMesh = go.GetComponentInChildren<MeshFilter>().transform;
            for (int i = 0; i < GetParameter<Count>().value; i++)
            {
                FindPointOnMesh(objectWithMesh);
            }
        }

        void FindPointOnMesh(Transform currentObject)
        {
            Vector3[] meshPoints = currentObject.GetComponent<MeshFilter>().sharedMesh.vertices;
            int[] tris = currentObject.GetComponent<MeshFilter>().sharedMesh.triangles;
            int triStart = Random.Range(0, meshPoints.Length / 3) * 3; // get first index of each triangle

            float a = Random.value;
            float b = Random.value;

            if (a + b >= 1)
            { // reflect back if > 1
                a = 1 - a;
                b = 1 - b;
            }

            var newPointOnMesh = meshPoints[triStart] + (a * (meshPoints[triStart + 1] - meshPoints[triStart])) + (b * (meshPoints[triStart + 2] - meshPoints[triStart])); // apply formula to get new random point inside triangle

            newPointOnMesh = currentObject.TransformPoint(newPointOnMesh); // convert back to worldspace
            var bounds = GeometryUtility.CalculateBounds(meshPoints, currentObject.localToWorldMatrix);
            
            float r = 0;

            if (bounds.max.x > r)
            {
                r = bounds.max.x;
            }
            if (bounds.max.y > r)
            {
                r = bounds.max.y;
            }
            if (bounds.max.z > r)
            {
                r = bounds.max.z;
            }

            var rayOrigin = ((Random.onUnitSphere * r) + currentObject.position); // put the ray randomly around the transform

            RaycastHit hitPoint = new RaycastHit();
            var casts = Physics.RaycastAll(rayOrigin, newPointOnMesh - rayOrigin, Mathf.Infinity);
            for (int i = 0; i < casts.Length; i++)
            {
                if (casts[i].transform == currentObject)
                {
                    hitPoint = casts[i];
                }
            }
            CreateObject(hitPoint.point, Quaternion.FromToRotation(Vector3.up, hitPoint.normal));
        }

        void ReplacePrefabs()
        {
            var gameObjects = Selection.objects;
            for (int i = 0; i < gameObjects.Length; i++)
            {
                GameObject obj = gameObjects[i] as GameObject;
                var position = obj.transform.position;
                var rotation = obj.transform.rotation;
                CreateObject(position, rotation);
            }
            for (int i = 0; i < gameObjects.Length; i++)
            {
                Undo.DestroyObjectImmediate(gameObjects[i]);
            }
        }

        void CreateObject(Vector3 position, Quaternion rotation)
        {
            GameObject osd = PrefabUtility.InstantiatePrefab(brushInfo.brushObjects[Random.Range(0, brushInfo.brushObjects.Count)]) as GameObject;
            osd.transform.position = position;
            osd.transform.rotation = rotation;
            if (GetParameter<Scale>().randomScale)
            {
                osd.transform.localScale *= Random.Range(GetParameter<Scale>().minValue, GetParameter<Scale>().maxValue);
            }
            Undo.RegisterCreatedObjectUndo(osd, "Create Prefab Instance");
        }
    }
}