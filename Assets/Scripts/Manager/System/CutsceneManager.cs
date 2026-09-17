using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Playables;
using TMPro;

public class CutsceneManager : MonoBehaviour
{
    [Header("Opening Cutscene")]
    [SerializeField] private PlayableDirector openingDirector;

    [Header("Death Cutscene")]
    [SerializeField] private PlayableDirector deathDirector;
    [SerializeField] private TMP_Text shardSubTitleText;

    [Header("Goal Cutscene")]
    [SerializeField] private PlayableDirector goalDirector;

    public PlayableDirector GoalDirector => goalDirector;

    private readonly HashSet<PlayableDirector> subscribedDirectors = new HashSet<PlayableDirector>();
    private readonly HashSet<PlayableDirector> activeDirectors = new HashSet<PlayableDirector>();
    private readonly List<CanvasSortingState> deathCanvasSortingStates = new List<CanvasSortingState>();
    private readonly List<RendererSortingState> playerRendererSortingStates = new List<RendererSortingState>();
    private bool deathRenderOrderApplied;
    private bool deathSequenceControlsRenderOrder;

    private struct CanvasSortingState
    {
        public Canvas canvas;
        public bool overrideSorting;
        public int sortingLayerID;
        public int sortingOrder;
    }

    private struct RendererSortingState
    {
        public Renderer renderer;
        public int sortingLayerID;
        public int sortingOrder;
    }

    private void OnEnable()
    {
        SubscribeDirector(openingDirector);
        SubscribeDirector(deathDirector);
        SubscribeDirector(goalDirector);
    }

    private void OnDisable()
    {
        foreach (PlayableDirector director in subscribedDirectors)
        {
            if (director == null) continue;
            director.played -= HandleDirectorPlayed;
            director.stopped -= HandleDirectorStopped;
        }

        subscribedDirectors.Clear();
        activeDirectors.Clear();
        deathSequenceControlsRenderOrder = false;
        RestoreDeathRenderOrder();
    }

    private void Start()
    {
        if (openingDirector != null)
        {
            openingDirector.timeUpdateMode = DirectorUpdateMode.UnscaledGameTime;
            if (openingDirector.state == PlayState.Playing)
                HandleDirectorPlayed(openingDirector);
            if (openingDirector.state != PlayState.Playing && !openingDirector.playOnAwake)
                openingDirector.gameObject.SetActive(false);
        }

        if (deathDirector != null)
        {
            deathDirector.timeUpdateMode = DirectorUpdateMode.UnscaledGameTime;
            if (deathDirector.state == PlayState.Playing)
                HandleDirectorPlayed(deathDirector);
            if (deathDirector.state != PlayState.Playing && !deathDirector.playOnAwake)
                deathDirector.gameObject.SetActive(false);
        }

        if (goalDirector != null)
        {
            goalDirector.timeUpdateMode = DirectorUpdateMode.UnscaledGameTime;
            if (goalDirector.state == PlayState.Playing)
                HandleDirectorPlayed(goalDirector);
            if (goalDirector.state != PlayState.Playing && !goalDirector.playOnAwake)
                goalDirector.gameObject.SetActive(false);
        }
    }

    private void SubscribeDirector(PlayableDirector director)
    {
        if (director == null || !subscribedDirectors.Add(director)) return;

        director.played += HandleDirectorPlayed;
        director.stopped += HandleDirectorStopped;
    }

    private void HandleDirectorPlayed(PlayableDirector director)
    {
        if (director == null || !activeDirectors.Add(director)) return;
        if (director == deathDirector)
            ApplyDeathRenderOrder();
    }

    private void HandleDirectorStopped(PlayableDirector director)
    {
        if (director != null)
            activeDirectors.Remove(director);

        if (director == deathDirector && !deathSequenceControlsRenderOrder)
            RestoreDeathRenderOrder();
    }

    private void ApplyDeathRenderOrder()
    {
        if (deathRenderOrderApplied || deathDirector == null) return;

        CachePlayerComponents();
        GameObject player = _playerMovement != null
            ? _playerMovement.gameObject
            : _mindPlayerMovement != null
                ? _mindPlayerMovement.gameObject
                : null;

        SortingLayer[] sortingLayers = SortingLayer.layers;
        int highestSortingLayerID = sortingLayers != null && sortingLayers.Length > 0
            ? sortingLayers[sortingLayers.Length - 1].id
            : 0;

        Canvas[] deathCanvases = deathDirector.GetComponentsInChildren<Canvas>(true);
        for (int i = 0; i < deathCanvases.Length; i++)
        {
            Canvas canvas = deathCanvases[i];
            if (canvas == null) continue;

            deathCanvasSortingStates.Add(new CanvasSortingState
            {
                canvas = canvas,
                overrideSorting = canvas.overrideSorting,
                sortingLayerID = canvas.sortingLayerID,
                sortingOrder = canvas.sortingOrder
            });

            int relativeOrder = Mathf.Clamp(canvas.sortingOrder, -100, 100);
            canvas.overrideSorting = true;
            canvas.sortingLayerID = highestSortingLayerID;
            canvas.sortingOrder = 32000 + relativeOrder;
        }

        if (player != null)
        {
            Renderer[] playerRenderers = player.GetComponentsInChildren<Renderer>(true);
            for (int i = 0; i < playerRenderers.Length; i++)
            {
                Renderer playerRenderer = playerRenderers[i];
                if (playerRenderer == null) continue;

                playerRendererSortingStates.Add(new RendererSortingState
                {
                    renderer = playerRenderer,
                    sortingLayerID = playerRenderer.sortingLayerID,
                    sortingOrder = playerRenderer.sortingOrder
                });

                int relativeOrder = Mathf.Clamp(playerRenderer.sortingOrder, -50, 50);
                playerRenderer.sortingLayerID = highestSortingLayerID;
                playerRenderer.sortingOrder = 32700 + relativeOrder;
            }
        }

        deathRenderOrderApplied = true;
    }

    private void RestoreDeathRenderOrder()
    {
        if (!deathRenderOrderApplied) return;

        for (int i = 0; i < deathCanvasSortingStates.Count; i++)
        {
            CanvasSortingState state = deathCanvasSortingStates[i];
            if (state.canvas == null) continue;

            state.canvas.overrideSorting = state.overrideSorting;
            state.canvas.sortingLayerID = state.sortingLayerID;
            state.canvas.sortingOrder = state.sortingOrder;
        }

        for (int i = 0; i < playerRendererSortingStates.Count; i++)
        {
            RendererSortingState state = playerRendererSortingStates[i];
            if (state.renderer == null) continue;

            state.renderer.sortingLayerID = state.sortingLayerID;
            state.renderer.sortingOrder = state.sortingOrder;
        }

        deathCanvasSortingStates.Clear();
        playerRendererSortingStates.Clear();
        deathRenderOrderApplied = false;
    }

    private PlayerMovement _playerMovement;
    private PlayerAttack _playerAttack;
    private Rigidbody2D _playerRb;
    private Collider2D _playerCollider;
    private MindPlayerMovement _mindPlayerMovement;

    private void CachePlayerComponents()
    {
        _playerMovement = FindAnyObjectByType<PlayerMovement>(FindObjectsInactive.Exclude);
        _mindPlayerMovement = FindAnyObjectByType<MindPlayerMovement>(FindObjectsInactive.Exclude);

        GameObject player = _playerMovement != null
            ? _playerMovement.gameObject
            : _mindPlayerMovement != null
                ? _mindPlayerMovement.gameObject
                : null;

        if (player == null)
        {
            return;
        }

        _playerAttack = player.GetComponent<PlayerAttack>();
        _playerRb = player.GetComponent<Rigidbody2D>();
        _playerCollider = player.GetComponent<Collider2D>();
    }

    private void LockPlayer(bool locked)
    {
        if (_playerMovement == null && _mindPlayerMovement == null) CachePlayerComponents();

        if (_playerMovement != null)
            _playerMovement.enabled = !locked;

        if (_playerAttack != null)
            _playerAttack.enabled = !locked;
            
        if (_mindPlayerMovement != null)
            _mindPlayerMovement.enabled = !locked;

        if (GameInput.Instance != null)
            GameInput.Instance.SetInputsEnabled(!locked);
    }

    public IEnumerator PlayOpeningSequence()
    {
        if (openingDirector == null) yield break;

        LockPlayer(true);

        openingDirector.gameObject.SetActive(true);
        openingDirector.Play();
        yield return StartCoroutine(WaitForDirectorOrSkip(openingDirector));
        openingDirector.gameObject.SetActive(false);
        HandleDirectorStopped(openingDirector);

        LockPlayer(false);
    }

    public IEnumerator PlayDeathSequence(int lostShards)
    {
        if (deathDirector == null)
        {
            Debug.LogError("[CutsceneManager] Cannot play the death sequence because no death director is assigned.", this);
            yield break;
        }

        if (shardSubTitleText != null)
        {
            shardSubTitleText.text = $"You lost {lostShards} Shards";
        }

        LockPlayer(true);

        if (_playerRb != null)
        {
            _playerRb.linearVelocity = new Vector2(0, _playerRb.linearVelocity.y);
        }

        try
        {
            deathSequenceControlsRenderOrder = true;
            ApplyDeathRenderOrder();
            deathDirector.gameObject.SetActive(true);
            deathDirector.Stop();
            deathDirector.time = 0d;
            deathDirector.Evaluate();
            deathDirector.Play();
            yield return StartCoroutine(WaitForDirectorOrSkip(deathDirector));

            yield return new WaitUntil(() => Input.anyKeyDown);
        }
        finally
        {
            deathDirector.Stop();
            deathDirector.time = 0d;
            deathDirector.gameObject.SetActive(false);
            HandleDirectorStopped(deathDirector);
            deathSequenceControlsRenderOrder = false;
            RestoreDeathRenderOrder();

            if (TimeManager.Instance != null) TimeManager.Instance.ClearAllPauses();
            else Time.timeScale = 1f;
            LockPlayer(false);
        }
    }

    public IEnumerator PlayGoalSequence(Vector3 goalPosition)
    {
        LockPlayer(true);

        if (_playerRb != null)
        {
            float originalGravity = _playerRb.gravityScale;
            _playerRb.gravityScale = 0f;
            _playerRb.linearVelocity = Vector2.zero;

            float time = 0f;
            float duration = 0.5f;
            Vector3 startPos = _playerRb.position;
            
            while (time < duration)
            {
                time += Time.unscaledDeltaTime;
                float t = time / duration;
                _playerRb.position = new Vector2(Mathf.Lerp(startPos.x, goalPosition.x, t), startPos.y);
                yield return null;
            }

            _playerRb.position = new Vector2(goalPosition.x, startPos.y);
            _playerRb.gravityScale = originalGravity;
            _playerRb.linearVelocity = new Vector2(0, -15f);
        }

        if (goalDirector != null)
        {
            goalDirector.gameObject.SetActive(true);
            goalDirector.Play();
            yield return StartCoroutine(WaitForDirectorOrSkip(goalDirector));
            goalDirector.gameObject.SetActive(false);
            HandleDirectorStopped(goalDirector);
        }
        else
        {
            yield return new WaitForSeconds(1.5f);
        }

        LockPlayer(false);
    }

    private IEnumerator WaitForDirectorOrSkip(PlayableDirector director)
    {
        if (director == null) yield break;

        float lastInputTime = -10f;
        float doubleTapThreshold = 0.5f;

        // Wait a frame in case Play() hasn't updated the state yet
        yield return null;

        // Wait strictly until the director finishes playing
        while (director.state == PlayState.Playing)
        {
            if (Input.anyKeyDown)
            {
                if (Time.unscaledTime - lastInputTime < doubleTapThreshold)
                {
                    // Double tap detected! Fast-forward cutscene.
                    director.time = director.duration;
                    director.Evaluate();
                    director.Stop(); // Force state to end
                    break;
                }
                lastInputTime = Time.unscaledTime;
            }

            yield return null;
        }
    }
}
