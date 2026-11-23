using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Minimapa: uma segunda câmera, de cima, que segue o jogador e desenha numa RenderTexture
/// (mostrada num círculo no HUD). Elites, guardião e boss ganham marcadores coloridos
/// numa camada que só o minimapa vê.
/// </summary>
public class MinimapController : MonoBehaviour
{
    private const float MarkerHeight = 20f;

    [SerializeField] private Camera minimapCamera;
    [SerializeField] private Transform player;
    [SerializeField] private HordeSpawner spawner;
    [SerializeField] private Sprite markerSprite;
    [SerializeField] private int markerLayer = 8;

    [Tooltip("Desenha o minimapa a cada N frames (economiza desempenho no celular).")]
    [SerializeField] private int renderEveryFrames = 2;

    [SerializeField] private Color eliteColor = new Color(1f, 0.75f, 0.2f);
    [SerializeField] private Color guardianColor = new Color(1f, 0.25f, 0.25f);
    [SerializeField] private Color bossColor = new Color(0.8f, 0.2f, 1f);

    private readonly List<SpriteRenderer> freeMarkers = new List<SpriteRenderer>();
    private readonly Dictionary<Enemy, SpriteRenderer> activeMarkers = new Dictionary<Enemy, SpriteRenderer>();
    private Transform markerRoot;
    private int frameCounter;

    private void Awake()
    {
        markerRoot = new GameObject("MinimapMarkers").transform;
        for (int i = 0; i < 16; i++)
        {
            freeMarkers.Add(CreateMarker());
        }
        // A câmera é desenhada à mão (RenderEveryFrames), não sozinha
        minimapCamera.enabled = false;
    }

    private void OnEnable()
    {
        spawner.EnemySpawned += HandleSpawned;
        spawner.EnemyDespawned += HandleDespawned;
    }

    private void OnDisable()
    {
        spawner.EnemySpawned -= HandleSpawned;
        spawner.EnemyDespawned -= HandleDespawned;
    }

    private void HandleSpawned(Enemy enemy)
    {
        if (enemy.Data.rank == EnemyRank.Common || activeMarkers.ContainsKey(enemy))
        {
            return;
        }

        SpriteRenderer marker = freeMarkers.Count > 0 ? freeMarkers[freeMarkers.Count - 1] : CreateMarker();
        if (freeMarkers.Count > 0)
        {
            freeMarkers.RemoveAt(freeMarkers.Count - 1);
        }

        switch (enemy.Data.rank)
        {
            case EnemyRank.Elite: marker.color = eliteColor; marker.transform.localScale = Vector3.one * 2.5f; break;
            case EnemyRank.Guardian: marker.color = guardianColor; marker.transform.localScale = Vector3.one * 4f; break;
            default: marker.color = bossColor; marker.transform.localScale = Vector3.one * 5f; break;
        }
        marker.gameObject.SetActive(true);
        activeMarkers.Add(enemy, marker);
    }

    private void HandleDespawned(Enemy enemy)
    {
        if (activeMarkers.TryGetValue(enemy, out SpriteRenderer marker))
        {
            activeMarkers.Remove(enemy);
            marker.gameObject.SetActive(false);
            freeMarkers.Add(marker);
        }
    }

    private void LateUpdate()
    {
        // Câmera de cima centralizada no jogador
        Vector3 p = player.position;
        minimapCamera.transform.SetPositionAndRotation(new Vector3(p.x, p.y + 60f, p.z), Quaternion.Euler(90f, 0f, 0f));

        foreach (KeyValuePair<Enemy, SpriteRenderer> pair in activeMarkers)
        {
            Vector3 g = pair.Key.GroundPosition;
            pair.Value.transform.position = new Vector3(g.x, g.y + MarkerHeight, g.z);
        }

        frameCounter++;
        if (frameCounter >= renderEveryFrames)
        {
            frameCounter = 0;
            minimapCamera.Render();
        }
    }

    private SpriteRenderer CreateMarker()
    {
        GameObject go = new GameObject("Marker");
        go.layer = markerLayer;
        go.transform.SetParent(markerRoot, false);
        go.transform.rotation = Quaternion.Euler(90f, 0f, 0f);
        SpriteRenderer sr = go.AddComponent<SpriteRenderer>();
        sr.sprite = markerSprite;
        sr.sortingOrder = 200;
        go.SetActive(false);
        return sr;
    }
}
