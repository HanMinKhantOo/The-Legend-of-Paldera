
using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif

public class IntroStoryController : MonoBehaviour
{
    [Header("Story UI")]
    [SerializeField] private TMP_Text storyText;
    [SerializeField] private TMP_Text palderaTitle;
    [SerializeField] private TMP_Text continueText;
    [SerializeField] private Button skipButton;

    [Header("Story Settings")]
    [SerializeField] private float secondsPerCharacter = 0.04f;

    [Header("Scene Settings")]
    [SerializeField] private string gameplaySceneName;

    private bool storyComplete = false;
    private bool isLoading = false;

    private Coroutine storyCoroutine;

    private void Start()
    {
        storyText.ForceMeshUpdate();
        storyText.maxVisibleCharacters = 0;

        palderaTitle.gameObject.SetActive(false);
        continueText.gameObject.SetActive(false);

        skipButton.onClick.AddListener(SkipIntro);

        storyCoroutine = StartCoroutine(ShowStory());
    }

    private IEnumerator ShowStory()
    {
        storyText.ForceMeshUpdate();

        int totalCharacters =
            storyText.textInfo.characterCount;

        for (int i = 0; i <= totalCharacters; i++)
        {
            storyText.maxVisibleCharacters = i;

            yield return new WaitForSecondsRealtime(
                secondsPerCharacter
            );
        }

        CompleteStory();
    }

    private void Update()
    {
        if (isLoading)
            return;

        if (SpacePressed())
        {
            if (!storyComplete)
            {
                if (storyCoroutine != null)
                    StopCoroutine(storyCoroutine);

                CompleteStory();
            }
            else
            {
                LoadGameplay();
            }
        }
    }

    private bool SpacePressed()
    {
        bool pressed = false;

#if ENABLE_INPUT_SYSTEM
        if (Keyboard.current != null)
        {
            pressed |=
                Keyboard.current.spaceKey.wasPressedThisFrame;
        }
#endif

#if ENABLE_LEGACY_INPUT_MANAGER
        pressed |= Input.GetKeyDown(KeyCode.Space);
#endif

        return pressed;
    }

    private void CompleteStory()
    {
        if (storyComplete)
            return;

        storyComplete = true;

        storyText.maxVisibleCharacters = int.MaxValue;

        palderaTitle.gameObject.SetActive(true);
        continueText.gameObject.SetActive(true);
    }

    public void SkipIntro()
    {
        LoadGameplay();
    }

    private void LoadGameplay()
    {
        if (isLoading)
            return;

        if (string.IsNullOrWhiteSpace(gameplaySceneName))
        {
            Debug.LogError(
                "Gameplay Scene Name is not assigned!"
            );

            return;
        }

        isLoading = true;

        SceneManager.LoadScene(gameplaySceneName);
    }
}