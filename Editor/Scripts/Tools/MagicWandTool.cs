﻿using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace Packages.PrefabshopEditor
{
    [ToolKeyCodeAttribute(KeyCode.M)]
    public class MagicWandTool : Tool
    {
        System.Action onReplace;
        GameObject underMouse;

        public MagicWandTool() : base()
        {
            var type = this.GetType();
   
            AddParameter(new Tag(type));
            AddParameter(new Layer(type));
            AddParameter(new Parent(type));
            AddParameter(new IgnoringLayer(type));
            AddParameter(new ListOfObjects(type));
            AddParameter(new PrefabsSet(type));
            AddParameter(new ButtonParameter(type));
            GetParameter<ButtonParameter>().buttonName = "Replace Selected";
            GetParameter<PrefabsSet>().Activate();
        }

        public override void SelectTool()
        {
            base.SelectTool();
            underMouse = null;
            GetParameter<ButtonParameter>().onButtonClick += ReplacePrefabs;
        }

        public override void DeselectTool()
        {
            GetParameter<ButtonParameter>().onButtonClick -= ReplacePrefabs;
            underMouse = null;
            base.DeselectTool();
        }

        public override void DrawHandle(Ray drawPointHit)
        {
            base.DrawHandle(drawPointHit);

            if (Event.current.type == EventType.MouseMove)
            {
                underMouse = HandleUtility.PickGameObject(Event.current.mousePosition, false);
            }
            if (underMouse != null)
            {
                if (PrefabUtility.GetOutermostPrefabInstanceRoot(underMouse) != null)
                {
                    underMouse = PrefabUtility.GetOutermostPrefabInstanceRoot(underMouse);
                    var shape = underMouse.GetComponentsInChildren<MeshFilter>();
                    var mat = new Material(Shader.Find("Raptorij/BrushShape"));
                    mat.SetColor("_Color", new Color(0, 1, 0, 0.25f));
                    mat.SetPass(0);
                    for (int i = 0; i < shape.Length; i++)
                    {
                        var position = shape[i].transform.position;
                        var rotation = shape[i].transform.rotation;
                        var scale = shape[i].transform.lossyScale;


                        var matrix = new Matrix4x4();
                        matrix.SetTRS(position, rotation, scale);
                        Graphics.DrawMeshNow(shape[i].sharedMesh, matrix, 0);
                    }
                }
            }
        }

        public override void Paint(RaycastHit drawPointHit)
        {
            base.Paint(drawPointHit);

            var castRay = HandleUtility.GUIPointToWorldRay(Event.current.mousePosition);
            var go = HandleUtility.PickGameObject(Event.current.mousePosition, false);
            SelectPrefabs(go);
        }

        private void SelectPrefabs(GameObject objectUnderMouse)
        {
            GameObject prefabAsset = PrefabUtility.GetCorrespondingObjectFromSource(objectUnderMouse) as GameObject;
            var listOfPrefabs = FindAllPrefabInstances(prefabAsset);
            GetParameter<ListOfObjects>().savedList = listOfPrefabs;
            Selection.objects = listOfPrefabs.ToArray();
        }

        void ReplacePrefabs(string buttonName)
        {
            var prefabs = GetParameter<PrefabsSet>().GetSelectedPrefabs();
            if (prefabs.Count > 0)
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
            else
            {
                Debug.Log($"<color=magenta>[Prefabshop] </color> There is no selected any objects in Options");
            }
        }

        void CreateObject(Vector3 position, Quaternion rotation)
        {
            var prefabs = GetParameter<PrefabsSet>().GetSelectedPrefabs();
            if (prefabs.Count > 0)
            {
                GameObject osd = PrefabUtility.InstantiatePrefab(prefabs[Random.Range(0, prefabs.Count)]) as GameObject;
                osd.transform.position = position;
                osd.transform.rotation = rotation;
                Undo.RegisterCreatedObjectUndo(osd, "Create Prefab Instance");
            }
        }

        List<GameObject> FindAllPrefabInstances(Object myPrefab)
        {
            List<GameObject> result = new List<GameObject>();
            var allObjects = GameObject.FindObjectsOfType<GameObject>();
            foreach (var GO in allObjects)
            {
                if (PrefabUtility.GetOutermostPrefabInstanceRoot(GO) != null)
                {
                    var GO_prefab = PrefabUtility.GetCorrespondingObjectFromSource(GO);
                    if (myPrefab == GO_prefab)
                    {
                        result.Add(PrefabUtility.GetOutermostPrefabInstanceRoot(GO));
                    }
                }
            }
            return result;
        }
    }
}