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
        bool showBaseStats;
        bool showIcons;
        float zoom = 1f;
        bool needsCenter;

        const float NodeWidth = 160f;
        const float NodeHeight = 60f;
        const float GridSize = 20f;
        const float MinZoom = 0.3f;
        const float MaxZoom = 2f;
        const string ConfigGuidPref = "TypingDefense_UpgradeGraphEditor_ConfigGuid";

        [MenuItem("TypingDefense/Upgrade Graph Editor")]
        static void Open()
        {
            GetWindow<UpgradeGraphEditorWindow>("Upgrade Graph");
        }

        void OnEnable()
        {
            var guid = EditorPrefs.GetString(ConfigGuidPref, "");
            if (string.IsNullOrEmpty(guid)) return;
            var path = AssetDatabase.GUIDToAssetPath(guid);
            config = AssetDatabase.LoadAssetAtPath<UpgradeGraphConfig>(path);
            if (config != null)
                needsCenter = true;
        }

        void OnGUI()
        {
            DrawToolbar();

            if (config == null)
            {
                EditorGUILayout.HelpBox("Assign an UpgradeGraphConfig to edit.", MessageType.Info);
                return;
            }

            if (needsCenter)
            {
                needsCenter = false;
                CenterOnRoot();
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
            DrawBaseStatsPanel();
            DrawIconsPanel();

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
                var guid = config != null ? AssetDatabase.AssetPathToGUID(AssetDatabase.GetAssetPath(config)) : "";
                EditorPrefs.SetString(ConfigGuidPref, guid);
                if (config != null)
                    CenterOnRoot();
            }

            if (GUILayout.Button("Add Node", EditorStyles.toolbarButton, GUILayout.Width(80)))
            {
                Undo.RecordObject(config, "Add Node");
                var isFirst = config.nodes.Length == 0;
                var nodeId = isFirst ? config.rootNodeId : $"NODE_{config.nodes.Length}";
                var list = config.nodes.ToList();
                list.Add(new UpgradeNode
                {
                    nodeId = nodeId,
                    displayName = isFirst ? "Root" : "New Node",
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

            if (GUILayout.Button(showBaseStats ? "Hide Base Stats" : "Base Stats", EditorStyles.toolbarButton, GUILayout.Width(100)))
                showBaseStats = !showBaseStats;

            if (GUILayout.Button(showIcons ? "Hide Icons" : "Icons", EditorStyles.toolbarButton, GUILayout.Width(70)))
                showIcons = !showIcons;

            if (GUILayout.Button("Center", EditorStyles.toolbarButton, GUILayout.Width(60)))
            {
                zoom = 1f;
                CenterOnRoot();
            }

            EditorGUILayout.EndHorizontal();
        }

        void CenterOnRoot()
        {
            var root = config.nodes.FirstOrDefault(n => n.nodeId == config.rootNodeId);
            if (root == null && config.nodes.Length > 0)
                root = config.nodes[0];
            if (root == null)
            {
                graphOffset = Vector2.zero;
                return;
            }

            var windowCenter = new Vector2(position.width / 2f, position.height / 2f);
            graphOffset = windowCenter - root.position * GridSize * zoom;
        }

        Rect GetInspectorRect()
        {
            return new Rect(position.width - 280, 30, 270, 500);
        }

        void HandleInput(Rect graphRect)
        {
            var e = Event.current;
            var mousePos = e.mousePosition - graphRect.position;

            if (showBaseStats && GetBaseStatsRect().Contains(e.mousePosition))
                return;

            if (selectedNodeIndex >= 0 && GetInspectorRect().Contains(e.mousePosition))
                return;

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
                    config.nodes[selectedNodeIndex].position += e.delta / (GridSize * zoom);
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

                case EventType.ScrollWheel:
                    var oldZoom = zoom;
                    zoom = Mathf.Clamp(zoom - e.delta.y * 0.05f, MinZoom, MaxZoom);
                    // Zoom towards mouse position
                    graphOffset += mousePos * (1f - zoom / oldZoom);
                    e.Use();
                    Repaint();
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
            var pos = node.position * GridSize * zoom + graphOffset;
            var w = NodeWidth * zoom;
            var h = NodeHeight * zoom;
            return new Rect(pos.x - w / 2, pos.y - h / 2, w, h);
        }

        void DrawGrid(Rect graphRect)
        {
            Handles.color = new Color(0.3f, 0.3f, 0.3f, 0.3f);
            var scaledGrid = GridSize * zoom;
            var startX = graphOffset.x % scaledGrid;
            var startY = graphOffset.y % scaledGrid;

            for (var x = startX; x < graphRect.width; x += scaledGrid)
                Handles.DrawLine(new Vector3(x, 0), new Vector3(x, graphRect.height));

            for (var y = startY; y < graphRect.height; y += scaledGrid)
                Handles.DrawLine(new Vector3(0, y), new Vector3(graphRect.width, y));
        }

        void DrawConnections()
        {
            Handles.color = new Color(0.7f, 0.7f, 0.7f, 0.6f);

            foreach (var node in config.nodes)
            {
                var fromPos = node.position * GridSize * zoom + graphOffset;

                foreach (var childId in node.connectedTo)
                {
                    var child = config.nodes.FirstOrDefault(n => n.nodeId == childId);
                    if (child == null) continue;

                    var toPos = child.position * GridSize * zoom + graphOffset;
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
                    fontSize = Mathf.Max(8, (int)(10 * zoom)),
                    wordWrap = true
                };

                GUI.Label(rect, $"{node.displayName}\n{node.upgradeId} — Lv{node.maxLevel}", labelStyle);
            }
        }

        void DrawConnectionPreview()
        {
            if (!isCreatingConnection) return;

            var source = config.nodes[connectionSourceIndex];
            var fromPos = source.position * GridSize * zoom + graphOffset;
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
            var inspectorRect = GetInspectorRect();

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
            EditorGUILayout.LabelField("Levels", EditorStyles.boldLabel);

            if (node.costsPerLevel == null || node.costsPerLevel.Length != node.maxLevel)
            {
                var old = node.costsPerLevel ?? new int[0];
                node.costsPerLevel = new int[node.maxLevel];
                for (var i = 0; i < node.costsPerLevel.Length; i++)
                    node.costsPerLevel[i] = i < old.Length ? old[i] : 100;
            }

            if (node.valuesPerLevel == null || node.valuesPerLevel.Length != node.maxLevel)
            {
                var old = node.valuesPerLevel ?? new float[0];
                node.valuesPerLevel = new float[node.maxLevel];
                for (var i = 0; i < node.valuesPerLevel.Length; i++)
                    node.valuesPerLevel[i] = i < old.Length ? old[i] : 1f;
            }

            for (var i = 0; i < node.maxLevel; i++)
            {
                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.LabelField($"  Lv{i + 1}", GUILayout.Width(35));
                node.costsPerLevel[i] = EditorGUILayout.IntField(node.costsPerLevel[i], GUILayout.Width(60));
                EditorGUILayout.LabelField("coins", GUILayout.Width(35));
                node.valuesPerLevel[i] = EditorGUILayout.FloatField(node.valuesPerLevel[i], GUILayout.Width(60));
                EditorGUILayout.LabelField("val", GUILayout.Width(25));
                EditorGUILayout.EndHorizontal();
            }

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

        Rect GetBaseStatsRect()
        {
            return new Rect(10, 30, 220, 540);
        }

        void DrawBaseStatsPanel()
        {
            if (!showBaseStats) return;

            var rect = GetBaseStatsRect();
            GUI.Box(rect, "");
            GUILayout.BeginArea(new Rect(rect.x + 5, rect.y + 5, rect.width - 10, rect.height - 10));

            EditorGUILayout.LabelField("Base Stats", EditorStyles.boldLabel);
            EditorGUILayout.Space(2);

            Undo.RecordObject(config, "Edit Base Stats");
            var b = config.baseStats;

            EditorGUILayout.LabelField("Combat", EditorStyles.miniLabel);
            b.MaxHp = EditorGUILayout.IntField("Max HP", b.MaxHp);
            b.BaseDamage = EditorGUILayout.IntField("Base Damage", b.BaseDamage);
            b.CritChance = EditorGUILayout.FloatField("Crit Chance", b.CritChance);
            b.BossBonusDamage = EditorGUILayout.IntField("Boss Bonus Dmg", b.BossBonusDamage);
            b.EnergyPerBossHit = EditorGUILayout.FloatField("Energy/Boss Hit", b.EnergyPerBossHit);

            EditorGUILayout.Space(4);
            EditorGUILayout.LabelField("Energy", EditorStyles.miniLabel);
            b.MaxEnergy = EditorGUILayout.FloatField("Max Energy", b.MaxEnergy);
            b.DrainMultiplier = EditorGUILayout.FloatField("Drain Multiplier", b.DrainMultiplier);
            b.EnergyPerKill = EditorGUILayout.FloatField("Energy/Kill", b.EnergyPerKill);

            EditorGUILayout.Space(4);
            EditorGUILayout.LabelField("Economy", EditorStyles.miniLabel);
            b.LettersPerKill = EditorGUILayout.IntField("Letters/Kill", b.LettersPerKill);

            EditorGUILayout.Space(4);
            EditorGUILayout.LabelField("Utility", EditorStyles.miniLabel);
            b.AutoTypeInterval = EditorGUILayout.FloatField("AutoType Interval", b.AutoTypeInterval);
            b.AutoTypeCount = EditorGUILayout.IntField("AutoType Count", b.AutoTypeCount);
            b.PowerUpKillInterval = EditorGUILayout.IntField("PowerUp Interval", b.PowerUpKillInterval);
            b.PowerUpDurationBonus = EditorGUILayout.FloatField("PowerUp Bonus", b.PowerUpDurationBonus);

            EditorGUILayout.Space(4);
            EditorGUILayout.LabelField("Collection", EditorStyles.miniLabel);
            b.CollectionDuration = EditorGUILayout.FloatField("Collection Duration", b.CollectionDuration);
            b.CollectionSpeed = EditorGUILayout.FloatField("Collection Speed", b.CollectionSpeed);
            b.LetterAttraction = EditorGUILayout.FloatField("Letter Attraction", b.LetterAttraction);

            GUILayout.EndArea();
        }

        Rect GetIconsRect()
        {
            var x = showBaseStats ? 240 : 10;
            return new Rect(x, 30, 260, 560);
        }

        void DrawIconsPanel()
        {
            if (!showIcons) return;

            var rect = GetIconsRect();
            GUI.Box(rect, "");
            GUILayout.BeginArea(new Rect(rect.x + 5, rect.y + 5, rect.width - 10, rect.height - 10));

            EditorGUILayout.LabelField("Upgrade Icons", EditorStyles.boldLabel);
            EditorGUILayout.Space(4);

            var usedIds = System.Enum.GetValues(typeof(UpgradeId))
                .Cast<UpgradeId>()
                .OrderBy(id => id.ToString())
                .ToArray();

            foreach (var upgradeId in usedIds)
            {
                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.LabelField(upgradeId.ToString(), GUILayout.Width(120));

                var currentSprite = GetIconForId(upgradeId);
                var newSprite = (Sprite)EditorGUILayout.ObjectField(
                    currentSprite, typeof(Sprite), false, GUILayout.Width(100));

                if (newSprite != currentSprite)
                {
                    Undo.RecordObject(config, "Change Upgrade Icon");
                    SetIconForId(upgradeId, newSprite);
                }

                EditorGUILayout.EndHorizontal();
            }

            GUILayout.EndArea();
        }

        Sprite GetIconForId(UpgradeId id)
        {
            if (config.upgradeIcons == null) return null;

            foreach (var entry in config.upgradeIcons)
            {
                if (entry.upgradeId == id) return entry.icon;
            }
            return null;
        }

        void SetIconForId(UpgradeId id, Sprite sprite)
        {
            var list = (config.upgradeIcons ?? System.Array.Empty<UpgradeIconEntry>()).ToList();

            for (var i = 0; i < list.Count; i++)
            {
                if (list[i].upgradeId != id) continue;

                if (sprite == null)
                {
                    list.RemoveAt(i);
                }
                else
                {
                    list[i].icon = sprite;
                }

                config.upgradeIcons = list.ToArray();
                return;
            }

            if (sprite == null) return;

            list.Add(new UpgradeIconEntry { upgradeId = id, icon = sprite });
            config.upgradeIcons = list.ToArray();
        }
    }
}
