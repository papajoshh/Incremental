using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Zenject;

namespace TypingDefense
{
    public class HudView : MonoBehaviour
    {
        [Header("Labels")]
        [SerializeField] TextMeshProUGUI hpLabel;
        [SerializeField] TextMeshProUGUI energyLabel;
        [SerializeField] TextMeshProUGUI levelLabel;
        [SerializeField] TextMeshProUGUI killsLabel;

        [Header("Collection")]
        [SerializeField] GameObject collectionGroup;
        [SerializeField] TextMeshProUGUI collectionTimerLabel;
        [SerializeField] Image collectionFill;

        [Header("Bars")]
        [SerializeField] Image hpFill;
        [SerializeField] Image hpDamageFill;
        [SerializeField] Image energyFill;
        [SerializeField] Image killsFill;
        [SerializeField] GameObject killBarGroup;

        [Header("Flash Overlays")]
        [SerializeField] Image damageFlash;

        RunManager runManager;
        EnergyTracker energyTracker;
        WordManager wordManager;
        PlayerStats playerStats;
        GameFlowController gameFlow;
        CameraShaker cameraShaker;
        LevelProgressionConfig levelConfig;
        CollectionPhaseController collectionPhase;

        int killCount;
        bool isEnergyFlashing;

        [Inject]
        public void Construct(
            RunManager runManager,
            EnergyTracker energyTracker,
            WordManager wordManager,
            PlayerStats playerStats,
            GameFlowController gameFlow,
            CameraShaker cameraShaker,
            LevelProgressionConfig levelConfig,
            CollectionPhaseController collectionPhase)
        {
            this.runManager = runManager;
            this.energyTracker = energyTracker;
            this.wordManager = wordManager;
            this.playerStats = playerStats;
            this.gameFlow = gameFlow;
            this.cameraShaker = cameraShaker;
            this.levelConfig = levelConfig;
            this.collectionPhase = collectionPhase;

            runManager.OnHpChanged += OnHpChanged;
            energyTracker.OnEnergyChanged += OnEnergyChanged;
            runManager.OnLevelChanged += OnLevelChanged;
            wordManager.OnWordCompleted += OnWordCompleted;
            wordManager.OnWordCriticalKill += OnWordCriticalKill;
            wordManager.OnWordReachedCenter += OnWordReachedCenter;
            wordManager.OnBossSpawned += OnBossSpawned;
            wordManager.OnBossDefeated += OnBossDefeated;
            wordManager.OnKillCountChanged += OnKillCountChanged;
            gameFlow.OnStateChanged += OnStateChanged;
            collectionPhase.OnTimerChanged += OnCollectionTimerChanged;
            collectionPhase.OnFreezeReleased += OnChargeReleaseFlash;

            InitFlashOverlays();
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
            wordManager.OnWordCriticalKill -= OnWordCriticalKill;
            wordManager.OnWordReachedCenter -= OnWordReachedCenter;
            wordManager.OnBossSpawned -= OnBossSpawned;
            wordManager.OnBossDefeated -= OnBossDefeated;
            wordManager.OnKillCountChanged -= OnKillCountChanged;
            gameFlow.OnStateChanged -= OnStateChanged;
            collectionPhase.OnTimerChanged -= OnCollectionTimerChanged;
            collectionPhase.OnFreezeReleased -= OnChargeReleaseFlash;
        }

        void InitFlashOverlays()
        {
            damageFlash.color = new Color(1f, 0f, 0f, 0f);
        }

        void OnStateChanged(GameState state)
        {
            var visible = state == GameState.Playing || state == GameState.Collecting;
            gameObject.SetActive(visible);

            if (!visible) return;

            var isCollecting = state == GameState.Collecting;

            energyLabel.gameObject.SetActive(!isCollecting);
            energyFill.transform.parent.gameObject.SetActive(!isCollecting);
            collectionGroup.SetActive(isCollecting);

            // Hide kills bar during collection
            killsLabel.gameObject.SetActive(!isCollecting);
            if (killBarGroup != null) killBarGroup.SetActive(!isCollecting);
            killsFill.transform.parent.gameObject.SetActive(!isCollecting);

            if (isCollecting)
            {
                collectionFill.fillAmount = 1f;
                collectionFill.color = new Color(0.3f, 0.8f, 1f);
                collectionTimerLabel.color = Color.white;

                collectionGroup.transform.DOComplete();
                collectionGroup.transform.localScale = Vector3.one * 1.3f;
                collectionGroup.transform.DOScale(1f, 0.3f).SetEase(Ease.OutBack).SetUpdate(true);
                return;
            }

            killCount = 0;
            killsFill.fillAmount = 0f;
            killsFill.color = new Color(0.3f, 0.7f, 1f);
            UpdateKillsDisplay();
            hpFill.fillAmount = 1f;
            hpDamageFill.fillAmount = 1f;
            energyFill.fillAmount = 1f;
        }

        void OnCollectionTimerChanged(float time)
        {
            var ratio = collectionPhase.TimerRatio;
            collectionTimerLabel.text = $"COLLECT  {time:F1}s";

            collectionFill.DOComplete();
            collectionFill.DOFillAmount(ratio, 0.15f).SetEase(Ease.OutQuad).SetUpdate(true);

            var barColor = ratio > 0.35f
                ? Color.Lerp(new Color(1f, 0.6f, 0f), new Color(0.3f, 0.8f, 1f), (ratio - 0.35f) / 0.65f)
                : Color.Lerp(Color.red, new Color(1f, 0.6f, 0f), ratio / 0.35f);
            collectionFill.color = barColor;

            if (time > 3f) return;

            collectionGroup.transform.DOComplete();
            collectionGroup.transform.DOPunchScale(Vector3.one * 0.08f, 0.3f, 8, 0f).SetUpdate(true);

            collectionTimerLabel.DOComplete();
            collectionTimerLabel.color = Color.red;
            collectionTimerLabel.DOColor(Color.white, 0.3f).SetUpdate(true);
        }

        void OnChargeReleaseFlash()
        {
            damageFlash.DOComplete();
            damageFlash.color = new Color(1f, 1f, 1f, 0.5f);
            damageFlash.DOFade(0f, 0.4f).SetEase(Ease.OutQuad).SetUpdate(true);
        }

        void OnHpChanged(int hp)
        {
            var ratio = (float)hp / playerStats.MaxHp;
            hpLabel.text = $"HP: {hp}/{playerStats.MaxHp}";

            hpFill.fillAmount = ratio;

            hpDamageFill.DOComplete();
            hpDamageFill.DOFillAmount(ratio, 0.3f).SetDelay(0.4f);

            hpLabel.transform.DOComplete();
            hpLabel.transform.DOPunchScale(Vector3.one * 0.3f, 0.3f, 10, 0);

            if (ratio > 0.3f) return;

            hpFill.DOComplete();
            hpFill.DOColor(Color.red, 0.1f).SetLoops(2, LoopType.Yoyo);
        }

        void OnWordReachedCenter(DefenseWord word)
        {
            damageFlash.DOComplete();
            damageFlash.color = new Color(1f, 0f, 0f, 0.25f);
            damageFlash.DOFade(0f, 0.3f).SetEase(Ease.OutQuad).SetUpdate(true);

            cameraShaker.Shake(0.35f, 0.25f);
        }

        void OnEnergyChanged(float energy)
        {
            var max = playerStats.MaxEnergy;
            var ratio = energy / max;
            energyLabel.text = $"Energy: {energy:F1}/{max:F0}";
            energyFill.fillAmount = ratio;

            if (ratio > 0.25f)
            {
                isEnergyFlashing = false;
                return;
            }

            if (isEnergyFlashing) return;

            isEnergyFlashing = true;
            energyFill.DOColor(new Color(1f, 0.3f, 0f), 0.1f).SetLoops(2, LoopType.Yoyo);
        }

        void OnLevelChanged(int level)
        {
            levelLabel.text = $"Level {level}";

            levelLabel.transform.DOComplete();
            levelLabel.transform.localScale = Vector3.one * 1.5f;
            levelLabel.transform.DOScale(1f, 0.4f).SetEase(Ease.OutElastic);

            levelLabel.DOComplete();
            levelLabel.DOColor(Color.yellow, 0.1f)
                .OnComplete(() => levelLabel.DOColor(Color.white, 0.3f));

            cameraShaker.Shake(0.25f, 0.3f);
        }

        void OnKillCountChanged()
        {
            killCount++;
            UpdateKillsDisplay();

            killsLabel.transform.DOComplete();
            killsLabel.transform.DOPunchScale(Vector3.one * 0.25f, 0.2f, 8, 0);

            killsLabel.DOComplete();
            killsLabel.color = Color.green;
            killsLabel.DOColor(Color.white, 0.2f);
        }

        void OnWordCompleted(DefenseWord word)
        {
            cameraShaker.Shake(0.2f, 0.15f, 12);
        }

        void OnWordCriticalKill(DefenseWord word)
        {
            cameraShaker.Shake(0.45f, 0.3f, 18);

            damageFlash.DOComplete();
            damageFlash.color = new Color(1f, 0.84f, 0f, 0.15f);
            damageFlash.DOFade(0f, 0.25f).SetEase(Ease.OutQuad);
        }

        void OnBossSpawned(DefenseWord word)
        {
            killsFill.DOComplete();
            killsFill.color = Color.white;
            killsFill.DOColor(new Color(1f, 0.3f, 0f), 0.4f);

            killsFill.transform.parent.DOComplete();
            killsFill.transform.parent.DOPunchScale(Vector3.one * 0.3f, 0.35f, 10, 0.5f);

            killsLabel.text = "BOSS!";
            killsLabel.transform.DOComplete();
            killsLabel.transform.localScale = Vector3.one * 1.6f;
            killsLabel.transform.DOScale(1f, 0.4f).SetEase(Ease.OutElastic);

            killsLabel.DOComplete();
            killsLabel.color = new Color(1f, 0.3f, 0f);
            killsLabel.DOColor(Color.white, 0.5f);

            cameraShaker.Shake(0.3f, 0.3f);
        }

        void UpdateKillsDisplay()
        {
            var config = levelConfig.GetLevel(runManager.CurrentLevel);
            var target = config.killsForBoss;
            var ratio = (float)killCount / target;

            killsLabel.text = $"{killCount} / {target}";

            killsFill.DOComplete();
            killsFill.DOFillAmount(ratio, 0.15f).SetEase(Ease.OutQuad);

            var barColor = Color.Lerp(
                new Color(0.3f, 0.7f, 1f),
                new Color(1f, 0.84f, 0f),
                ratio);
            killsFill.color = barColor;

            var punchIntensity = Mathf.Lerp(0.05f, 0.2f, ratio);
            killsFill.transform.DOComplete();
            killsFill.transform.DOPunchScale(Vector3.one * punchIntensity, 0.2f, 8, 0f);
        }

        void OnBossDefeated(DefenseWord word)
        {
            damageFlash.DOComplete();
            damageFlash.color = new Color(1f, 0.9f, 0.3f, 0.4f);
            damageFlash.DOFade(0f, 0.5f).SetEase(Ease.OutQuad);

            cameraShaker.Shake(0.5f, 0.4f);
        }
    }
}
