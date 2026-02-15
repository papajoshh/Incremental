using System.Linq;
using UnityEditor;
using UnityEngine;

namespace TypingDefense.Editor
{
    public class UpgradeGraphEditorWindow : EditorWindow
    {
        UpgradeGraphConfig config;
        Vector2 graphOffset;
        int selectedNodeIndex = -1;
        bool isDraggingNode;
        bool isDraggingCanvas;
        bool isCreatingConnection;
        int connectionSourceIndex;
        Vector2 lastMousePos;

        const float NodeWidth = 160f;
        const float NodeHeight = 60f;
        const float GridSize = 20f;

        [MenuItem("TypingDefense/Upgrade Graph Editor")]
        static void Open()
        {
            GetWindow<UpgradeGraphEditorWindow>("Upgrade Graph");
        }

        void OnGUI()
        {
            DrawToolbar();

            if (config == null)
            {
                EditorGUILayout.HelpBox("Assign an UpgradeGraphConfig to edit.", MessageType.Info);
                return;
            }

            var graphRect = new Rect(0, EditorGUIUtility.singleLineHeight + 6,
                position.width, position.height - EditorGUIUtility.singleLineHeight - 6);
            HandleInput(graphRect);

            GUI.BeginClip(graphRect);
            DrawGrid(graphRect);
            DrawConnections();
            DrawNodes();
            DrawConnectionPreview();
            GUI.EndClip();

            DrawInspector();

            if (GUI.changed)
            {
                EditorUtility.SetDirty(config);
                config.InvalidateCache();
            }
        }

        void DrawToolbar()
        {
            EditorGUILayout.BeginHorizontal(EditorStyles.toolbar);

            var newConfig = (UpgradeGraphConfig)EditorGUILayout.ObjectField(
                config, typeof(UpgradeGraphConfig), false, GUILayout.Width(250));

            if (newConfig != config)
            {
                config = newConfig;
                selectedNodeIndex = -1;
            }

            if (GUILayout.Button("Add Node", EditorStyles.toolbarButton, GUILayout.Width(80)))
            {
                Undo.RecordObject(config, "Add Node");
                var list = config.nodes.ToList();
                list.Add(new UpgradeNode
                {
                    nodeId = $"NODE_{list.Count}",
                    displayName = "New Node",
                    position = -graphOffset / GridSize,
                    maxLevel = 1,
                    costsPerLevel = new[] { 100 }
                });
                config.nodes = list.ToArray();
                selectedNodeIndex = config.nodes.Length - 1;
            }

            if (selectedNodeIndex >= 0 && GUILayout.Button("Delete Node", EditorStyles.toolbarButton, GUILayout.Width(80)))
            {
                Undo.RecordObject(config, "Delete Node");
                var deletedId = config.nodes[selectedNodeIndex].nodeId;
                var list = config.nodes.ToList();
                list.RemoveAt(selectedNodeIndex);

                foreach (var node in list)
                {
                    node.connectedTo = node.connectedTo
                        .Where(id => id != deletedId)
                        .ToArray();
                }

                config.nodes = list.ToArray();
                selectedNodeIndex = -1;
            }

            GUILayout.FlexibleSpace();

            if (GUILayout.Button("Center", EditorStyles.toolbarButton, GUILayout.Width(60)))
                graphOffset = Vector2.zero;

            EditorGUILayout.EndHorizontal();
        }

        void HandleInput(Rect graphRect)
        {
            var e = Event.current;
            var mousePos = e.mousePosition - graphRect.position;

            switch (e.type)
            {
                case EventType.MouseDown when e.button == 0:
                {
                    var hitIndex = GetNodeAtPosition(mousePos);

                    if (e.control && hitIndex >= 0)
                    {
                        isCreatingConnection = true;
                        connectionSourceIndex = hitIndex;
                        e.Use();
                        break;
                    }

                    if (hitIndex >= 0)
                    {
                        selectedNodeIndex = hitIndex;
                        isDraggingNode = true;
                        e.Use();
                    }
                    else
                    {
                        selectedNodeIndex = -1;
                    }
                    break;
                }

                case EventType.MouseDown when e.button == 2:
                    isDraggingCanvas = true;
                    lastMousePos = e.mousePosition;
                    e.Use();
                    break;

                case EventType.MouseDrag when isDraggingNode && selectedNodeIndex >= 0:
                    Undo.RecordObject(config, "Move Node");
                    config.nodes[selectedNodeIndex].position += e.delta / GridSize;
                    e.Use();
                    Repaint();
                    break;

                case EventType.MouseDrag when isDraggingCanvas:
                    graphOffset += e.mousePosition - lastMousePos;
                    lastMousePos = e.mousePosition;
                    e.Use();
                    Repaint();
                    break;

                case EventType.MouseUp when e.button == 0:
                {
                    if (isCreatingConnection)
                    {
                        var hitIndex = GetNodeAtPosition(mousePos);
                        if (hitIndex >= 0 && hitIndex != connectionSourceIndex)
                        {
                            Undo.RecordObject(config, "Create Connection");
                            var source = config.nodes[connectionSourceIndex];
                            var targetId = config.nodes[hitIndex].nodeId;

                            if (!source.connectedTo.Contains(targetId))
                            {
                                var connections = source.connectedTo.ToList();
                                connections.Add(targetId);
                                source.connectedTo = connections.ToArray();
                            }
                        }
                        isCreatingConnection = false;
                        e.Use();
                        Repaint();
                    }
                    isDraggingNode = false;
                    break;
                }

                case EventType.MouseUp when e.button == 2:
                    isDraggingCanvas = false;
                    break;
            }
        }

        int GetNodeAtPosition(Vector2 mousePos)
        {
            for (var i = config.nodes.Length - 1; i >= 0; i--)
            {
                var nodeRect = GetNodeRect(config.nodes[i]);
                if (nodeRect.Contains(mousePos))
                    return i;
            }
            return -1;
        }

        Rect GetNodeRect(UpgradeNode node)
        {
            var pos = node.position * GridSize + graphOffset;
            return new Rect(pos.x - NodeWidth / 2, pos.y - NodeHeight / 2, NodeWidth, NodeHeight);
        }

        void DrawGrid(Rect graphRect)
        {
            Handles.color = new Color(0.3f, 0.3f, 0.3f, 0.3f);
            var startX = graphOffset.x % GridSize;
            var startY = graphOffset.y % GridSize;

            for (var x = startX; x < graphRect.width; x += GridSize)
                Handles.DrawLine(new Vector3(x, 0), new Vector3(x, graphRect.height));

            for (var y = startY; y < graphRect.height; y += GridSize)
                Handles.DrawLine(new Vector3(0, y), new Vector3(graphRect.width, y));
        }

        void DrawConnections()
        {
            Handles.color = new Color(0.7f, 0.7f, 0.7f, 0.6f);

            foreach (var node in config.nodes)
            {
                var fromPos = node.position * GridSize + graphOffset;

                foreach (var childId in node.connectedTo)
                {
                    var child = config.nodes.FirstOrDefault(n => n.nodeId == childId);
                    if (child == null) continue;

                    var toPos = child.position * GridSize + graphOffset;
                    Handles.DrawLine(
                        new Vector3(fromPos.x, fromPos.y),
                        new Vector3(toPos.x, toPos.y));
                }
            }
        }

        void DrawNodes()
        {
            for (var i = 0; i < config.nodes.Length; i++)
            {
                var node = config.nodes[i];
                var rect = GetNodeRect(node);
                var isSelected = i == selectedNodeIndex;
                var isRoot = node.nodeId == config.rootNodeId;

                var bgColor = isRoot
                    ? new Color(0.2f, 0.6f, 0.2f)
                    : isSelected
                        ? new Color(0.3f, 0.5f, 0.8f)
                        : new Color(0.25f, 0.25f, 0.25f);

                EditorGUI.DrawRect(rect, bgColor);

                var borderColor = isSelected ? Color.yellow : new Color(0.5f, 0.5f, 0.5f);
                Handles.color = borderColor;
                Handles.DrawSolidRectangleWithOutline(rect, Color.clear, borderColor);

                var labelStyle = new GUIStyle(EditorStyles.label)
                {
                    alignment = TextAnchor.MiddleCenter,
                    normal = { textColor = Color.white },
                    fontSize = 10,
                    wordWrap = true
                };

                GUI.Label(rect, $"{node.displayName}\n{node.nodeId} (Lv{node.maxLevel})", labelStyle);
            }
        }

        void DrawConnectionPreview()
        {
            if (!isCreatingConnection) return;

            var source = config.nodes[connectionSourceIndex];
            var fromPos = source.position * GridSize + graphOffset;
            var toPos = Event.current.mousePosition;

            Handles.color = Color.yellow;
            Handles.DrawDottedLine(
                new Vector3(fromPos.x, fromPos.y),
                new Vector3(toPos.x, toPos.y), 4f);

            Repaint();
        }

        void DrawInspector()
        {
            if (selectedNodeIndex < 0 || selectedNodeIndex >= config.nodes.Length) return;

            var node = config.nodes[selectedNodeIndex];
            var inspectorRect = new Rect(position.width - 280, 30, 270, 400);

            GUI.Box(inspectorRect, "");
            GUILayout.BeginArea(new Rect(inspectorRect.x + 5, inspectorRect.y + 5,
                inspectorRect.width - 10, inspectorRect.height - 10));

            EditorGUILayout.LabelField("Node Inspector", EditorStyles.boldLabel);
            EditorGUILayout.Space(4);

            Undo.RecordObject(config, "Edit Node");

            node.nodeId = EditorGUILayout.TextField("Node ID", node.nodeId);
            node.upgradeId = (UpgradeId)EditorGUILayout.EnumPopup("Upgrade ID", node.upgradeId);
            node.displayName = EditorGUILayout.TextField("Display Name", node.displayName);
            node.description = EditorGUILayout.TextField("Description", node.description);
            node.position = EditorGUILayout.Vector2Field("Position", node.position);
            node.maxLevel = EditorGUILayout.IntField("Max Level", node.maxLevel);

            EditorGUILayout.Space(4);
            EditorGUILayout.LabelField("Costs Per Level", EditorStyles.boldLabel);

            if (node.costsPerLevel == null || node.costsPerLevel.Length != node.maxLevel)
            {
                var old = node.costsPerLevel ?? new int[0];
                node.costsPerLevel = new int[node.maxLevel];
                for (var i = 0; i < node.costsPerLevel.Length; i++)
                    node.costsPerLevel[i] = i < old.Length ? old[i] : 100;
            }

            for (var i = 0; i < node.costsPerLevel.Length; i++)
                node.costsPerLevel[i] = EditorGUILayout.IntField($"  Level {i + 1}", node.costsPerLevel[i]);

            EditorGUILayout.Space(4);
            EditorGUILayout.LabelField($"Connections: {node.connectedTo.Length}", EditorStyles.boldLabel);
            EditorGUILayout.HelpBox("Ctrl+Click node, drag to another to connect.", MessageType.Info);

            for (var i = node.connectedTo.Length - 1; i >= 0; i--)
            {
                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.LabelField(node.connectedTo[i]);
                if (GUILayout.Button("X", GUILayout.Width(20)))
                {
                    var list = node.connectedTo.ToList();
                    list.RemoveAt(i);
                    node.connectedTo = list.ToArray();
                }
                EditorGUILayout.EndHorizontal();
            }

            GUILayout.EndArea();
        }
    }
}
