using System;
using TUIOsharp;
using TUIOsharp.DataProcessors;
using TUIOsharp.Entities;
using UnityEngine;

public class CursorData {
    public int id;
    public Vector2 pos;
    public string hitName = "";
}

public class TUIOHandler : MonoBehaviour {
    public static int port = 3333;

    public static bool degs = false;
    public static bool invertX = false;
    public static bool invertY = false;

    private static TuioServer tuioServer;
    private int screenWidth;
    private int screenHeight;

    [SerializeField] private bool showLog = false;

    private void connect() {
        if (!Application.isPlaying) return;
        if (tuioServer != null) disconnect();

        tuioServer = new TuioServer(port);
        Debug.Log("TUIO Port" + port);
        tuioServer.Connect();
    }

    private void OnEnable() {
        screenWidth = Screen.width;
        screenHeight = Screen.height;

        CursorProcessor cursorProcessor = new CursorProcessor();
        cursorProcessor.CursorAdded += onCursorAdded;
        cursorProcessor.CursorUpdated += onCursorUpdated;
        cursorProcessor.CursorRemoved += onCursorRemoved;

        BlobProcessor blobProcessor = new BlobProcessor();
        blobProcessor.BlobAdded += OnBlobAdded;
        blobProcessor.BlobUpdated += OnBlobUpdated;
        blobProcessor.BlobRemoved += OnBlobRemoved;

        ObjectProcessor objectProcessor = new ObjectProcessor();
        objectProcessor.ObjectAdded += OnObjectAdded;
        objectProcessor.ObjectUpdated += OnObjectUpdated;
        objectProcessor.ObjectRemoved += OnObjectRemoved;

        connect();
        tuioServer.AddDataProcessor(cursorProcessor);
        tuioServer.AddDataProcessor(blobProcessor);
        tuioServer.AddDataProcessor(objectProcessor);
    }

    protected void OnDisable() {
        disconnect();
    }

    public Action<TuioCursor, float, float> OnCursorAdded;
    public Action<TuioCursor, float, float> OnCursorUpdated;
    public Action<TuioCursor, float, float> OnCursorRemoved;

    private void onCursorAdded(object sender, TuioCursorEventArgs e) {
        TuioCursor entity = e.Cursor;
        lock (tuioServer) {
            //var x = invertX ? (1 - entity.X) : entity.X;
            //var y = invertY ? (1 - entity.Y) : entity.Y;
            var x = entity.X * screenWidth;
            var y = (1 - entity.Y) * screenHeight;
            if (showLog) {
                Debug.Log(string.Format("Cursor Added {0}:{1},{2}", entity.Id, x, y));
            }
            OnCursorAdded?.Invoke(entity, x, y);
        }
        //Debug.Log("OnCursorAdded");
    }

    private void onCursorUpdated(object sender, TuioCursorEventArgs e) {
        var entity = e.Cursor;
        lock (tuioServer) {
            //var x = invertX ? (1 - entity.X) : entity.X;
            //var y = invertY ? (1 - entity.Y) : entity.Y;
            var x = Mathf.Round(entity.X * screenWidth);
            var y = (1 - entity.Y) * screenHeight;
            //Debug.Log($"{entity.X}  {entity.Y}  {entity.VelocityX}  {entity.VelocityY}  {entity.Id}");
            //Debug.Log(string.Format("{0} Cursor Moved {1}:{2},{3}", ((CursorProcessor)sender).FrameNumber, entity.Id, x, y));
            OnCursorUpdated?.Invoke(entity, x, y);
        }
        //Debug.Log("OnCursorUpdated");
    }

    private void onCursorRemoved(object sender, TuioCursorEventArgs e) {
        var entity = e.Cursor;
        lock (tuioServer) {
            if (showLog) {
                Debug.Log(string.Format("{0} Cursor Removed {1}", ((CursorProcessor)sender).FrameNumber, entity.Id));
            }
            OnCursorRemoved?.Invoke(entity, 0, 0);
        }
    }

    private void OnBlobAdded(object sender, TuioBlobEventArgs e) {
        var entity = e.Blob;
        lock (tuioServer) {
            var x = invertX ? (1 - entity.X) : entity.X;
            var y = invertY ? (1 - entity.Y) : entity.Y;
            var angle = degs ? (entity.Angle * (180f / Math.PI)) : entity.Angle;
            Debug.Log(string.Format("{0} Blob Added {1}:{2},{3} {4:F3}", ((BlobProcessor)sender).FrameNumber, entity.Id, x, y, angle));
        }
        Debug.Log("OnBlobAdded");
    }

    private void OnBlobUpdated(object sender, TuioBlobEventArgs e) {
        var entity = e.Blob;
        lock (tuioServer) {
            var x = invertX ? (1 - entity.X) : entity.X;
            var y = invertY ? (1 - entity.Y) : entity.Y;
            var angle = degs ? (entity.Angle * (180f / Math.PI)) : entity.Angle;
            Debug.Log(string.Format("{0} Blob Moved {1}:{2},{3} {4:F3}", ((BlobProcessor)sender).FrameNumber, entity.Id, x, y, angle));
        }
        Debug.Log("OnBlobUpdated");
    }

    private void OnBlobRemoved(object sender, TuioBlobEventArgs e) {
        var entity = e.Blob;
        lock (tuioServer) {
            Debug.Log(string.Format("{0} Blob Removed {1}", ((BlobProcessor)sender).FrameNumber, entity.Id));
        }
        Debug.Log("OnBlobRemoved");
    }

    private void OnObjectAdded(object sender, TuioObjectEventArgs e) {
        var entity = e.Object;
        lock (tuioServer) {
            var x = invertX ? (1 - entity.X) : entity.X;
            var y = invertY ? (1 - entity.Y) : entity.Y;
            var angle = degs ? (entity.Angle * (180f / Math.PI)) : entity.Angle;
            Debug.Log(string.Format("{0} Object Added {1}/{2}:{3},{4} {5:F3}", ((ObjectProcessor)sender).FrameNumber, entity.ClassId, entity.Id, x, y, angle));
        }
        Debug.Log("OnObjectAdded");
    }

    private void OnObjectUpdated(object sender, TuioObjectEventArgs e) {
        var entity = e.Object;
        lock (tuioServer) {
            var x = invertX ? (1 - entity.X) : entity.X;
            var y = invertY ? (1 - entity.Y) : entity.Y;
            var angle = degs ? (entity.Angle * (180f / Math.PI)) : entity.Angle;
            Debug.Log(string.Format("{0} Object Moved {1}/{2}:{3},{4} {5:F3}", ((ObjectProcessor)sender).FrameNumber, entity.ClassId, entity.Id, x, y, angle));
        }
    }

    private void OnObjectRemoved(object sender, TuioObjectEventArgs e) {
        var entity = e.Object;
        lock (tuioServer) {
            Debug.Log(string.Format("{0} Object Removed {1}/{2}", ((ObjectProcessor)sender).FrameNumber, entity.ClassId, entity.Id));
        }
    }

    void disconnect() {
        if (tuioServer != null) {
            tuioServer.RemoveAllDataProcessors();
            tuioServer.Disconnect();
            tuioServer = null;
        }
    }


}
