using System;
using System.Collections;
using System.Collections.Generic;
using TUIOsharp.Entities;
using UnityEngine;
using UnityEngine.EventSystems;
using Random = UnityEngine.Random;

public class BreakBubble : TUIOGameBase {

    [SerializeField] private GameObject prefab_Bubble;
    [SerializeField] private int bubbleCountMax = 20;
    [SerializeField] private Transform bubbleContainer;

    private List<GameObject> bubbles = new List<GameObject>();

    protected override void Start() {
        base.Start();

        prefab_Bubble.SetActive(false);

        StartCoroutine(generator());
    }

    int bubbleSize = 60;
    private void generateBubbles() {
        lock (bubbles) {
            if (bubbles.Count < bubbleCountMax) {
                GameObject t = Instantiate(prefab_Bubble, bubbleContainer);
                bubbles.Add(t);
                int x = Random.Range(-(Screen.width - bubbleSize) / 2, (Screen.width - bubbleSize) / 2);
                int y = Random.Range(-(Screen.height - bubbleSize) / 2, (Screen.height - bubbleSize) / 2);
                t.transform.localPosition = new Vector3(x, y);

                t.SetActive(true);
            }
        }
    }

    private IEnumerator generator() {
        while (true) { 
            yield return new WaitForSeconds(1f);
            generateBubbles();
        }
    }

    private Queue<Action> rays = new Queue<Action>();

    protected override void onCursorAdd(TuioCursor entity, float x, float y) {
        breakBubble(x, y);
    }

    protected override void onCursorUpdate(TuioCursor entity, float x, float y) {
        breakBubble(x, y);
    }

    private void breakBubble(float x, float y) {
        lock (rays) {
            rays.Enqueue(() => {
                PointerEventData pointer = new PointerEventData(eventSystem);
                pointer.position = new Vector2(x, y);
                List<RaycastResult> results = new List<RaycastResult>();
                try {
                    graphicRaycaster.Raycast(pointer, results);
                } catch (Exception e) {
                    Debug.LogError(e);
                }

                lock (bubbles) {
                    foreach (RaycastResult result in results) {
                        if (result.gameObject.tag == "bubble") {
                            bubbles.Remove(result.gameObject);
                            Destroy(result.gameObject);
                        }
                    }
                }
            });
        }
    }

    private void FixedUpdate() {
        if (rays.Count != 0) {
            lock (rays) {
                for (int i = 0; i < rays.Count; i++) {
                    rays.Dequeue().Invoke();
                }
            }
        }
    }

}
