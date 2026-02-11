using System.Collections.Generic;
using TMPro;
using UnityEngine;
using Zenject;

namespace Programental
{
    public class CodeLineCloneManager : MonoBehaviour
    {
        [SerializeField] private TMP_Text cloneLinePrefab;
        [SerializeField] private Transform clonesParent;
        [SerializeField] private float positionRange = 50f;
        [SerializeField] private Vector2 scaleRange = new(0.4f, 0.8f);

        [Inject] private CodeTyper _mainTyper;
        [Inject] private LinesTracker _linesTracker;
        [Inject] private BonusMultipliers _bonusMultipliers;
        [Inject] private CodeStructuresTracker _tracker;

        private readonly List<CloneLine> _clones = new();

        private void OnEnable()
        {
            _mainTyper.OnCharTyped += OnMainCharTyped;
            _tracker.OnStructureChanged += OnStructureChanged;
        }

        private void OnDisable()
        {
            _mainTyper.OnCharTyped -= OnMainCharTyped;
            _tracker.OnStructureChanged -= OnStructureChanged;
        }

        private void OnStructureChanged(int _) => SyncCloneCount();

        private void OnMainCharTyped(char c, string visibleText)
        {
            if (c == '\0') return;

            foreach (var clone in _clones)
                clone.Typer.TypeNextChar();
        }

        private void SyncCloneCount()
        {
            var target = _bonusMultipliers.CloneLineCount;

            while (_clones.Count < target)
                _clones.Add(CreateClone());

            while (_clones.Count > target)
            {
                var last = _clones[_clones.Count - 1];
                Destroy(last.Text.gameObject);
                _clones.RemoveAt(_clones.Count - 1);
            }
        }

        private CloneLine CreateClone()
        {
            var typer = new CodeTyper();
            typer.Initialize();

            var text = Instantiate(cloneLinePrefab, clonesParent);
            text.richText = false;

            var rt = text.rectTransform;
            rt.anchoredPosition = new Vector2(
                Random.Range(-positionRange, positionRange),
                Random.Range(-positionRange, positionRange));
            var scale = Random.Range(scaleRange.x, scaleRange.y);
            rt.localScale = Vector3.one * scale;

            typer.OnCharTyped += (c, visibleText) => text.text = visibleText;
            typer.OnLineCompleted += (line, total) => _linesTracker.AddCompletedLine();

            return new CloneLine { Typer = typer, Text = text };
        }

        private class CloneLine
        {
            public CodeTyper Typer;
            public TMP_Text Text;
        }
    }
}
