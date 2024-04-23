using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TUIOsharp.Entities;
using UnityEngine;
using UnityEngine.EventSystems;

public class FollowEffect : TUIOGameBase {

    [SerializeField] private GameObject[] prefab_Efs;
    [SerializeField] private int efCountMax = 20;
    [SerializeField] private Transform efContainer;
    [SerializeField] private Material[] shaderMaterials;

    private Dictionary<int, GameObject> efs = new Dictionary<int, GameObject>();

    int countEvent = 0;
    int countTriggered = 0;

    protected override void Start() {
        base.Start();

        for (int i = 0; i < prefab_Efs.Length; i++) {
            prefab_Efs[i].SetActive(false);
        }

        shaderMaterials[0].SetFloatArray("_GradientPosition", new float[] { 0f, 0.2f, 0.5f, 0.8f, 1f });
        shaderMaterials[0].SetColorArray("_GradientColors", new Color[] { Color.red, Color.green, Color.blue, Color.white, Color.yellow });

        shaderMaterials[1].SetFloatArray("_GradientPosition", new float[] { 0f, 0.2f, 0.5f, 0.8f, 1f });
        shaderMaterials[1].SetColorArray("_GradientColors", new Color[] { Color.blue, Color.green, Color.yellow, Color.white, Color.red });
    }

    private Queue<Action> rays = new Queue<Action>();
    protected override void onCursorAdd(TuioCursor entity, float x, float y) {
        lock (rays) {
            rays.Enqueue(() => {
                lock (efs) {
                    if (!efs.ContainsKey(entity.Id)) {
                        GameObject ef = generate();
                        efs.Add(entity.Id, ef);
                        ef.transform.localPosition = new Vector3(x, y);
                        ef.SetActive(true);
                        //print($"added : {entity.Id}");
                    }
                }
            });
            //countEvent += 1;
            //print($"added : {countEvent}");
        }
    }

    private GameObject generate() {
        try {
            return Instantiate(prefab_Efs[UnityEngine.Random.Range(0, prefab_Efs.Length)], efContainer);
        } catch (Exception e) {
            Debug.LogError(e);
            return null;
        }
    }

    protected override void onCursorUpdate(TuioCursor entity, float x, float y) {
        lock (rays) {
            rays.Enqueue(() => {
                lock (efs) {
                    if (efs.ContainsKey(entity.Id)) {
                        GameObject ef = efs[entity.Id];
                        ef.transform.localPosition = new Vector3(x, y);
                    }
                }
            });
            //countEvent += 1;
            //print($"added : {countEvent}");
        }
    }


    protected override void onCursorRemove(TuioCursor entity, float x, float y) {
        lock (rays) {
            rays.Enqueue(() => {
                lock (efs) {
                    if (efs.ContainsKey(entity.Id)) {
                        GameObject ef = efs[entity.Id];
                        efs.Remove(entity.Id);
                        Destroy(ef);
                        //print($"removed : {entity.Id}");
                    }
                }
            });
            //countEvent += 1;
            //print($"added : {countEvent}");
        }
    }

    private void Update() {
        if (Input.GetKeyDown(KeyCode.Escape)) {
            Application.Quit();
        }
    }

    private void FixedUpdate() {
        if (rays.Count != 0) {
            lock (rays) {
                for (int i = 0; i < rays.Count; i++) {
                    rays.Dequeue().Invoke();
                    //countTriggered += 1;
                    //print($"triggered : {countTriggered}");
                }
            }
        }
    }
}
