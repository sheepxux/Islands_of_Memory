using UnityEngine;
using UnityEngine.UI;

public class IntroFlow : MonoBehaviour
{
    [Header("Slides")]
    public Image slideImage;
    public Sprite[] slides;

    private int currentIndex = 0;

    private void Start()
    {
        if (slides.Length > 0)
            slideImage.sprite = slides[0];

        Time.timeScale = 0f;
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            NextSlide();
        }
    }

    void NextSlide()
    {
        currentIndex++;

        if (currentIndex >= slides.Length)
        {
            StartGame();
            return;
        }

        slideImage.sprite = slides[currentIndex];
    }

    void StartGame()
    {
        Time.timeScale = 1f;
        gameObject.SetActive(false);
    }
}