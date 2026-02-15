using TMPro;
using UnityEngine;
using Zenject;

namespace TypingDefense
{
    public class ConverterLetterView : MonoBehaviour
    {
        [SerializeField] TextMeshPro label;
        [SerializeField] SpriteRenderer background;

        static readonly Color[] LetterColors =
        {
            new(0.6f, 0.6f, 0.6f),
            new(0.3f, 0.7f, 1f),
            new(0.3f, 1f, 0.3f),
            new(1f, 0.6f, 0f),
            new(1f, 0.3f, 1f),
        };

        public void Setup(LetterType type, Vector3 position)
        {
            transform.position = position;
            label.text = type.ToString().ToLower();

            if (background != null)
                background.color = LetterColors[(int)type];

            transform.rotation = Quaternion.Euler(0, 0, Random.Range(-15f, 15f));
        }

        public class Factory : PlaceholderFactory<ConverterLetterView> { }
    }
}
