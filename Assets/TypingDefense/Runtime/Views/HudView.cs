using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Zenject;

namespace TypingDefense
{
    public class HudView : MonoBehaviour
    {
        [SerializeField] TextMeshProUGUI hpLabel;
        [SerializeField] TextMeshProUGUI energyLabel;
        [SerializeField] TextMeshProUGUI levelLabel;
        [SerializeField] TextMeshProUGUI killsLabel;
        [SerializeField] Image energyFill;
        [SerializeField] Button retreatButton;

        RunManager runManager;
        EnergyTracker energyTracker;
        WordManager wordManager;
        PlayerStats playerStats;
        GameFlowController gameFlow;

        int killCount;

        [Inject]
        public void Construct(
            RunManager runManager,
            EnergyTracker energyTracker,
            WordManager wordManager,
            PlayerStats playerStats,
            GameFlowController gameFlow)
        {
            this.runManager = runManager;
            this.energyTracker = energyTracker;
            this.wordManager = wordManager;
            this.playerStats = playerStats;
            this.gameFlow = gameFlow;

            runManager.OnHpChanged += OnHpChanged;
            energyTracker.OnEnergyChanged += OnEnergyChanged;
            runManager.OnLevelChanged += OnLevelChanged;
            wordManager.OnWordCompleted += OnWordCompleted;
            gameFlow.OnStateChanged += OnStateChanged;
            retreatButton.onClick.AddListener(OnRetreatClicked);
        }

        void Start()
        {
            OnStateChanged(gameFlow.State);
        }

        void OnDestroy()
        {
            runManager.OnHpChanged -= OnHpChanged;
            energyTracker.OnEnergyChanged -= OnEnergyChanged;
            runManager.OnLevelChanged -= OnLevelChanged;
            wordManager.OnWordCompleted -= OnWordCompleted;
            gameFlow.OnStateChanged -= OnStateChanged;
            retreatButton.onClick.RemoveListener(OnRetreatClicked);
        }

        void OnStateChanged(GameState state)
        {
            gameObject.SetActive(state == GameState.Playing);

            if (state != GameState.Playing) return;

            killCount = 0;
            killsLabel.text = "Kills: 0";
        }

        void OnRetreatClicked()
        {
            runManager.Retreat();
        }

        void OnHpChanged(int hp)
        {
            hpLabel.text = $"HP: {hp}/{playerStats.MaxHp}";
            hpLabel.transform.DOComplete();
            hpLabel.transform.DOPunchScale(Vector3.one * 0.3f, 0.3f, 10, 0);
        }

        void OnEnergyChanged(float energy)
        {
            energyLabel.text = $"Energy: {energy:F1}/{playerStats.MaxEnergy}";
            energyFill.fillAmount = energy / playerStats.MaxEnergy;
        }

        void OnLevelChanged(int level)
        {
            levelLabel.text = $"Level {level}";
            levelLabel.transform.DOComplete();
            levelLabel.transform.DOPunchScale(Vector3.one * 0.4f, 0.4f, 8, 0);
        }

        void OnWordCompleted(DefenseWord word)
        {
            killCount++;
            killsLabel.text = $"Kills: {killCount}";
            killsLabel.transform.DOComplete();
            killsLabel.transform.DOPunchScale(Vector3.one * 0.15f, 0.2f, 5, 0);
        }
    }
}
