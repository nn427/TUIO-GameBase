using DG.Tweening;
using System;
using System.Collections.Generic;
using System.Linq;
using TUIOsharp.Entities;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;


public class MyTest : TUIOGameBase {

    public RectTransform prefab;

    int row = 18;
    int column = 32;

    int startYPos = -30;

    float distanceRandomMinX = 55;
    float distanceRandomMaxX = 65;

    float distanceRandomMinY = 60;
    float distanceRandomMaxY = 60;

    float initMoveDistance = 1200;

    float enlargeSize = 5;

    float radiateSize = 220;

    private List<List<RectTransform>> goList;
    private Dictionary<RectTransform, Vector2> itemPosDict;
    private List<RectTransform> changedItemList;

    protected override void Start() {
        base.Start();
        goList = new List<List<RectTransform>>();
        itemPosDict = new Dictionary<RectTransform, Vector2>();
        changedItemList = new List<RectTransform>();
        createGos();
    }

    private void createGos() {
        for (int i = 0; i < row; i++) {
            List<RectTransform> gos = new List<RectTransform>();
            goList.Add(gos);
            float lastPosX = -1950;
            for (int j = 0; j < column; j++) {
                RectTransform item = (Instantiate(prefab.gameObject) as GameObject).GetComponent<RectTransform>();
                item.name = i + " " + j;
                item.transform.SetParent(transform);
                //Vector2 startPos = new Vector3(Random.Range(distanceRandomMinX, distanceRandomMaxX) + lastPosX, startYPos - i * Random.Range(distanceRandomMinY, distanceRandomMaxY));
                Vector2 startPos = new Vector3(60 + lastPosX, startYPos - i * 60);
                item.anchoredPosition = startPos;
                //Vector2 endPos = new Vector3(startPos.x - initMoveDistance, startZPos - i * Random.Range(distanceRandomMinY, distanceRandomMaxY));
                //Tweener tweener = item.DOAnchorPosX(endPos.x, Random.Range(1.8f, 2f)); 
                //tweener.SetDelay(j * 0.1f + (row - i) * 0.1f);  
                //tweener.SetEase(Ease.InSine); 
                item.gameObject.SetActive(true);
                gos.Add(item);
                itemPosDict.Add(item, startPos);

                lastPosX = item.anchoredPosition.x;
            }
        }
    }

    List<CursorData> list_Cursor = new List<CursorData>();
    Queue<Action> rays = new Queue<Action>();
    Dictionary<int, List<RectTransform>> changedListDic = new Dictionary<int, List<RectTransform>>();

    protected override void onCursorAdd(TuioCursor entity, float x, float y) {
        updateCursor(entity, x, y);
    }

    protected override void onCursorUpdate(TuioCursor entity, float x, float y) {
        updateCursor(entity, x, y);
    }

    protected override void onCursorRemove(TuioCursor entity, float x, float y) {
        onRemove(entity, x, y);
    }

    private void updateCursor(TuioCursor entity, float x, float y) {
        lock (list_Cursor) {
            CursorData data;
            if (list_Cursor.Where(a => a.id == entity.Id).Count() > 0) {
                //update
                data = list_Cursor.Where(a => a.id == entity.Id).First();
                data.pos = new Vector2(x, y);
            } else {
                //add
                data = new CursorData();
                data.id = entity.Id;
                data.pos = new Vector2(x, y);
                list_Cursor.Add(data);
            }

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

                    foreach (RaycastResult result in results) {
                        if (result.gameObject.tag == "target") {
                            if (data.hitName != result.gameObject.name) {
                                try {
                                    lock (changedListDic) {
                                        if (!changedListDic.ContainsKey(data.id)) {
                                            changedListDic.Add(data.id, triggerEnter(result.gameObject.GetComponent<RectTransform>()));
                                        }

                                        data.hitName = result.gameObject.name;
                                        //print($"{data.hitName}  Enter");
                                    }
                                } catch (Exception e) {
                                    Debug.LogError(e);
                                }
                            } else {
                                if (!string.IsNullOrEmpty(data.hitName)) {
                                    //already trigger other enter
                                    try {
                                        lock (changedListDic) {
                                            if (changedListDic.ContainsKey(data.id)) {
                                                List<RectTransform> toRecover = changedListDic[data.id];
                                                triggerExit(toRecover);
                                                changedListDic.Remove(data.id);
                                            }
                                        }
                                    } catch (Exception e) {
                                        Debug.LogError(e);
                                    }
                                    //print($"{data.hitName}  Exit");
                                }
                            }
                        }
                    }
                });
            }
        }
    }

    private void onRemove(TuioCursor entity, float x, float y) {
        lock (list_Cursor) {
            if (list_Cursor.Where(a => a.id == entity.Id).Count() > 0) {
                int index = list_Cursor.FindIndex(a => a.id == entity.Id);
                CursorData data = list_Cursor[index];
                lock (rays) {
                    rays.Enqueue(() => {
                        if (!string.IsNullOrEmpty(data.hitName)) {
                            try {
                                lock (changedListDic) {
                                    if (changedListDic.ContainsKey(data.id)) {
                                        List<RectTransform> toRecover = changedListDic[data.id];
                                        triggerExit(toRecover);
                                        changedListDic.Remove(data.id);
                                    }
                                }
                            } catch (Exception ex) {
                                Debug.LogError(ex);
                            }
                        }
                    });
                }
                list_Cursor.RemoveAt(index);
            }
        }
    }

    float anchorTime = 0.8f;
    float scaleTime = 0.5f;

    private List<RectTransform> triggerEnter(RectTransform item) {
        item.DOScale(enlargeSize, scaleTime);

        Vector2 pos = itemPosDict[item];

        changedItemList = new List<RectTransform>();

        foreach (KeyValuePair<RectTransform, Vector2> i in itemPosDict) {
            if (Vector2.Distance(i.Value, pos) < radiateSize) {
                changedItemList.Add(i.Key);
            }
        }

        for (int i = 0; i < changedItemList.Count; i++) {
            Vector2 targetPos = itemPosDict[item] + (itemPosDict[changedItemList[i]] - itemPosDict[item]).normalized * radiateSize;
            changedItemList[i].DOAnchorPos(targetPos, anchorTime);
        }

        changedItemList.Add(item);

        return changedItemList;
    }

    private void triggerExit(List<RectTransform> toRecover) {
        for (int i = 0; i < toRecover.Count; i++) {
            toRecover[i].DOAnchorPos(itemPosDict[toRecover[i]], anchorTime);
            toRecover[i].DOScale(1, scaleTime);
        }
    }

    private void Update() {
        
    }

    private void FixedUpdate() {
        if (rays.Count != 0) {
            lock (rays) {
                rays.Dequeue().Invoke();
            }
        }
    }

}
