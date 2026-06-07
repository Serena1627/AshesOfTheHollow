using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class PixelatedBattleTransition : MonoBehaviour
{
    public static PixelatedBattleTransition Instance { get; private set; }

    [Header("Transition UI")]
    [SerializeField] private Canvas transitionCanvas;
    [SerializeField] private RawImage pixelatedImage;
    [SerializeField] private Image blackCutoffImage;

    [Header("Capture Camera")]
    [Tooltip("Camera used only to render the pixelated image. Remove its Audio Listener and leave it disabled in the Inspector.")]
    [SerializeField] private Camera transitionCamera;

    [Header("Pixelation Settings")]
    [SerializeField] private int pixelationSteps = 12;
    [SerializeField] private int minimumWidth = 16;
    [SerializeField] private int minimumHeight = 9;
    [SerializeField] private float pixelateDuration = 0.45f;
    [SerializeField] private float unpixelateDuration = 0.45f;
    [SerializeField] private float blackCutoffDuration = 0.06f;

    [Header("Gameplay")]
    [SerializeField] private bool freezeGameplayDuringTransition = true;

    private RenderTexture captureTexture;
    private int originalWidth;
    private int originalHeight;
    private bool transitionRunning;
    private float previousTimeScale = 1f;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);

        if (transitionCanvas != null)
        {
            transitionCanvas.gameObject.SetActive(false);
        }

        if (pixelatedImage != null)
        {
            pixelatedImage.gameObject.SetActive(false);
        }

        if (blackCutoffImage != null)
        {
            blackCutoffImage.gameObject.SetActive(false);
        }

        if (transitionCamera != null)
        {
            transitionCamera.enabled = false;
            transitionCamera.targetTexture = null;
        }
    }

    public void BeginBattleTransition(string sceneName)
    {
        if (transitionRunning)
        {
            return;
        }

        if (string.IsNullOrWhiteSpace(sceneName))
        {
            Debug.LogWarning("PixelatedBattleTransition received an empty scene name.");
            return;
        }

        StartCoroutine(TransitionToScene(sceneName));
    }

    private IEnumerator TransitionToScene(string sceneName)
    {
        transitionRunning = true;

        Camera outgoingCamera = Camera.main;

        if (!CanRunTransition(outgoingCamera))
        {
            transitionRunning = false;
            SceneManager.LoadScene(sceneName, LoadSceneMode.Single);
            yield break;
        }

        if (freezeGameplayDuringTransition)
        {
            previousTimeScale = Time.timeScale;
            Time.timeScale = 0f;
        }

        originalWidth = Mathf.Max(1, Screen.width);
        originalHeight = Mathf.Max(1, Screen.height);

        CreateCaptureTexture(originalWidth, originalHeight);
        ConfigureTransitionCamera(outgoingCamera);

        transitionCanvas.gameObject.SetActive(true);
        pixelatedImage.gameObject.SetActive(true);

        if (blackCutoffImage != null)
        {
            blackCutoffImage.gameObject.SetActive(false);
        }

        yield return StartCoroutine(
            AnimatePixelation(outgoingCamera, pixelating: true)
        );

        if (blackCutoffImage != null)
        {
            blackCutoffImage.gameObject.SetActive(true);
        }

        yield return new WaitForSecondsRealtime(blackCutoffDuration);

        transitionCamera.enabled = false;

        yield return SceneManager.LoadSceneAsync(
            sceneName,
            LoadSceneMode.Single
        );

        // Allow BattleScene cameras and spawned objects to initialize.
        yield return null;

        Camera incomingCamera = Camera.main;

        if (incomingCamera == null || incomingCamera == transitionCamera)
        {
            Debug.LogWarning(
                "PixelatedBattleTransition could not find the new scene Main Camera."
            );

            FinishTransition();
            yield break;
        }

        ConfigureTransitionCamera(incomingCamera);
        ResizeCaptureTexture(minimumWidth, minimumHeight);
        RenderCapture();

        if (blackCutoffImage != null)
        {
            blackCutoffImage.gameObject.SetActive(false);
        }

        yield return StartCoroutine(
            AnimatePixelation(incomingCamera, pixelating: false)
        );

        FinishTransition();
    }

    private bool CanRunTransition(Camera sourceCamera)
    {
        if (sourceCamera == null)
        {
            Debug.LogWarning(
                "PixelatedBattleTransition could not find a Main Camera."
            );

            return false;
        }

        if (transitionCanvas == null ||
            pixelatedImage == null ||
            transitionCamera == null)
        {
            Debug.LogWarning(
                "PixelatedBattleTransition references are missing. " +
                "The scene will load without the transition."
            );

            return false;
        }

        if (sourceCamera == transitionCamera)
        {
            Debug.LogWarning(
                "Transition Camera must not be tagged MainCamera."
            );

            return false;
        }

        return true;
    }

    private IEnumerator AnimatePixelation(
        Camera sourceCamera,
        bool pixelating
    )
    {
        int steps = Mathf.Max(1, pixelationSteps);

        float duration = pixelating
            ? pixelateDuration
            : unpixelateDuration;

        float delay = duration / steps;

        for (int step = 0; step <= steps; step++)
        {
            float progress = step / (float)steps;

            if (!pixelating)
            {
                progress = 1f - progress;
            }

            int width = Mathf.RoundToInt(
                Mathf.Lerp(originalWidth, minimumWidth, progress)
            );

            int height = Mathf.RoundToInt(
                Mathf.Lerp(originalHeight, minimumHeight, progress)
            );

            ResizeCaptureTexture(width, height);

            if (sourceCamera != null)
            {
                ConfigureTransitionCamera(sourceCamera);
                RenderCapture();
            }

            if (step < steps)
            {
                yield return new WaitForSecondsRealtime(delay);
            }
        }
    }

    private void ConfigureTransitionCamera(Camera sourceCamera)
    {
        transitionCamera.CopyFrom(sourceCamera);

        transitionCamera.transform.SetPositionAndRotation(
            sourceCamera.transform.position,
            sourceCamera.transform.rotation
        );

        transitionCamera.targetTexture = captureTexture;
        transitionCamera.enabled = false;
    }

    private void CreateCaptureTexture(int width, int height)
    {
        ReleaseCaptureTexture();

        captureTexture = new RenderTexture(
            width,
            height,
            16,
            RenderTextureFormat.ARGB32
        );

        captureTexture.name = "BattlePixelTransitionTexture";
        captureTexture.filterMode = FilterMode.Point;
        captureTexture.wrapMode = TextureWrapMode.Clamp;
        captureTexture.Create();

        pixelatedImage.texture = captureTexture;
    }

    private void ResizeCaptureTexture(int width, int height)
    {
        if (captureTexture == null)
        {
            CreateCaptureTexture(width, height);
            return;
        }

        width = Mathf.Max(1, width);
        height = Mathf.Max(1, height);

        if (captureTexture.width == width &&
            captureTexture.height == height)
        {
            return;
        }

        captureTexture.Release();
        captureTexture.width = width;
        captureTexture.height = height;
        captureTexture.Create();
    }

    private void RenderCapture()
    {
        if (transitionCamera == null || captureTexture == null)
        {
            return;
        }

        transitionCamera.targetTexture = captureTexture;
        transitionCamera.Render();
    }

    private void FinishTransition()
    {
        if (transitionCamera != null)
        {
            transitionCamera.enabled = false;
            transitionCamera.targetTexture = null;
        }

        if (pixelatedImage != null)
        {
            pixelatedImage.gameObject.SetActive(false);
            pixelatedImage.texture = null;
        }

        if (blackCutoffImage != null)
        {
            blackCutoffImage.gameObject.SetActive(false);
        }

        if (transitionCanvas != null)
        {
            transitionCanvas.gameObject.SetActive(false);
        }

        ReleaseCaptureTexture();

        if (freezeGameplayDuringTransition)
        {
            Time.timeScale = previousTimeScale;
        }

        transitionRunning = false;
    }

    private void ReleaseCaptureTexture()
    {
        if (captureTexture == null)
        {
            return;
        }

        captureTexture.Release();
        Destroy(captureTexture);
        captureTexture = null;
    }

    private void OnDestroy()
    {
        ReleaseCaptureTexture();

        if (Instance == this)
        {
            Instance = null;
        }
    }

    private void OnApplicationQuit()
    {
        ReleaseCaptureTexture();
    }
}