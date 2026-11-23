using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

/// <summary>
/// Tela de level-up: pausa o jogo e mostra cartas de upgrade para escolher uma.
/// Escolha com clique, toque ou as teclas 1, 2 e 3.
/// </summary>
public class LevelUpScreen : MonoBehaviour
{
    [Serializable]
    public class CardView
    {
        public Button button;
        public Image icon;
        public Text nameText;
        public Text levelText;
        public Text descriptionText;
    }

    [SerializeField] private PlayerExperience experience;
    [SerializeField] private PlayerLoadout loadout;
    [SerializeField] private PlayerHealth playerHealth;
    [SerializeField] private GameObject panel;
    [SerializeField] private CardView[] cards;

    [Tooltip("Distância horizontal entre as cartas.")]
    [SerializeField] private float cardSpacing = 470f;

    [Tooltip("Espera antes de aceitar escolha, para não escolher sem querer.")]
    [SerializeField] private float inputDelay = 0.35f;

    [Header("Carta de cura (quando não há upgrades disponíveis)")]
    [SerializeField] private string healName = "Chá verde";
    [SerializeField] private string healDescription = "Recupera parte da vida";
    [SerializeField] private Sprite healIcon;

    private readonly List<UpgradeOffer> offers = new List<UpgradeOffer>();
    private float shownTime = -1f;

    public bool IsOpen => panel.activeSelf;

    private void Awake()
    {
        panel.SetActive(false);
        for (int i = 0; i < cards.Length; i++)
        {
            int index = i; // cópia para a função abaixo
            cards[i].button.onClick.AddListener(() => Choose(index));
        }
    }

    private void OnEnable()
    {
        experience.LeveledUp += HandleLeveledUp;
    }

    private void OnDisable()
    {
        experience.LeveledUp -= HandleLeveledUp;
    }

    private void HandleLeveledUp(int level)
    {
        if (!IsOpen && !playerHealth.IsDead)
        {
            Show();
        }
    }

    private void Show()
    {
        loadout.BuildOffers(offers);
        for (int i = 0; i < cards.Length; i++)
        {
            bool visible = i < offers.Count;
            cards[i].button.gameObject.SetActive(visible);
            if (visible)
            {
                Fill(cards[i], offers[i]);
                // Centraliza as cartas visíveis (uma só fica no meio)
                RectTransform rt = (RectTransform)cards[i].button.transform;
                rt.anchoredPosition = new Vector2((i - (offers.Count - 1) * 0.5f) * cardSpacing, rt.anchoredPosition.y);
            }
        }
        panel.SetActive(true);
        shownTime = Time.unscaledTime;
        Time.timeScale = 0f;
    }

    private void Fill(CardView card, UpgradeOffer offer)
    {
        if (offer.IsHeal)
        {
            card.icon.sprite = healIcon;
            card.nameText.text = healName;
            card.levelText.text = "";
            card.descriptionText.text = healDescription;
            return;
        }

        UpgradeDefinition def = offer.Definition;
        card.icon.sprite = def.icon;
        card.nameText.text = def.displayName;
        string kind = def.Kind == UpgradeKind.Ability ? "Habilidade" : "Passiva";
        card.levelText.text = offer.IsNew
            ? kind + " - Nova!"
            : kind + " - Nível " + offer.NextLevel + (offer.NextLevel == def.MaxLevel ? " (máx.)" : "");
        card.descriptionText.text = def.GetDescription(offer.NextLevel);
    }

    private void Update()
    {
        if (!IsOpen || Time.unscaledTime - shownTime < inputDelay || Keyboard.current == null)
        {
            return;
        }

        if (Keyboard.current.digit1Key.wasPressedThisFrame || Keyboard.current.numpad1Key.wasPressedThisFrame) Choose(0);
        else if (Keyboard.current.digit2Key.wasPressedThisFrame || Keyboard.current.numpad2Key.wasPressedThisFrame) Choose(1);
        else if (Keyboard.current.digit3Key.wasPressedThisFrame || Keyboard.current.numpad3Key.wasPressedThisFrame) Choose(2);
    }

    public void Choose(int index)
    {
        if (!IsOpen || index >= offers.Count || Time.unscaledTime - shownTime < inputDelay)
        {
            return;
        }

        loadout.Apply(offers[index]);
        experience.ConsumePendingLevelUp();
        panel.SetActive(false);

        if (experience.PendingLevelUps > 0)
        {
            Show(); // ainda tem level-up acumulado
        }
        else if (!playerHealth.IsDead)
        {
            Time.timeScale = 1f;
        }
    }
}
