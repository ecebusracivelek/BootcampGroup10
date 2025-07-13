using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.Collections;

public class MainMenuLogic : MonoBehaviour
{
    [Header("Audio Settings")]
    [SerializeField] private AudioSource menuMusicSource;
    [SerializeField] private AudioClip menuMusic;
    [SerializeField] private AudioClip playButtonSound;
    
    [Header("Fade Settings")]
    [SerializeField] private float fadeOutDuration = 2f;
    [SerializeField] private float playButtonSoundVolume = 0.7f;
    
    [Header("Black Screen Transition")]
    [SerializeField] private float blackScreenDuration = 2f; // Siyah ekran süresi
    [SerializeField] private float fadeToBlackDuration = 1f; // Siyaha geçiş süresi
    [SerializeField] private Image blackScreenOverlay; // Siyah ekran için overlay
    
    [Header("Image Slideshow Settings")]
    [SerializeField] private Image displayImage;
    [SerializeField] private Sprite[] slideshowImages;
    
    [Header("Slideshow Timing")]
    [SerializeField] private float imageDuration = 3f;
    [SerializeField] private float imageFadeDuration = 1f;
    [SerializeField] private bool autoStartSlideshow = true;
    [SerializeField] private bool loopSlideshow = true;
    
    [Header("Debug")]
    [SerializeField] private bool showDebugLogs = true;
    
    [Header("UI Layer Settings")]
    [SerializeField] private Canvas backgroundCanvas; // Slideshow için
    [SerializeField] private Canvas uiCanvas; // Butonlar ve title için
    [SerializeField] private int backgroundSortingOrder = 0;
    [SerializeField] private int uiSortingOrder = 10;
    
    // Slideshow private variables
    private int currentImageIndex = 0;
    private bool isSlideshowPlaying = false;
    private Coroutine slideshowCoroutine;

    void Start()
    {
        if (showDebugLogs)
            Debug.Log("MainMenuLogic başlatıldı!");
            
        SetupCanvasLayers();
        SetupBlackScreenOverlay();
        SetupMenuMusic();
        SetupSlideshow();
        
        if (autoStartSlideshow)
        {
            StartSlideshow();
        }
    }

    #region Canvas Layer Management
    
    private void SetupCanvasLayers()
    {
        // Background Canvas ayarları (slideshow için)
        if (backgroundCanvas == null)
        {
            // Eğer atanmamışsa, displayImage'ın canvas'ını kullan
            if (displayImage != null)
            {
                backgroundCanvas = displayImage.GetComponentInParent<Canvas>();
            }
        }
        
        if (backgroundCanvas != null)
        {
            backgroundCanvas.sortingOrder = backgroundSortingOrder;
            backgroundCanvas.overrideSorting = true;
            
            if (showDebugLogs)
                Debug.Log($"Background Canvas sorting order: {backgroundSortingOrder}");
        }
        
        // UI Canvas ayarları (butonlar ve title için)
        if (uiCanvas == null)
        {
            // Tüm Canvas'ları bul ve background canvas olmayan birini seç
            Canvas[] allCanvases = FindObjectsByType<Canvas>(FindObjectsSortMode.None);
            foreach (Canvas canvas in allCanvases)
            {
                if (canvas != backgroundCanvas)
                {
                    uiCanvas = canvas;
                    break;
                }
            }
        }
        
        if (uiCanvas != null)
        {
            uiCanvas.sortingOrder = uiSortingOrder;
            uiCanvas.overrideSorting = true;
            
            if (showDebugLogs)
                Debug.Log($"UI Canvas sorting order: {uiSortingOrder}");
        }
        else
        {
            if (showDebugLogs)
                Debug.LogWarning("UI Canvas bulunamadı! Butonlar ve UI elementleri için ayrı Canvas oluşturun.");
        }
    }
    
    // Slideshow için güvenli parent bulma
    private Transform GetSlideshowParent()
    {
        // DisplayImage'ın direct parent'ını kullan (aynı canvas içinde)
        if (displayImage != null)
        {
            return displayImage.transform.parent;
        }
        
        // Yoksa background canvas içinde bir parent ara
        if (backgroundCanvas != null)
        {
            return backgroundCanvas.transform;
        }
        
        return null;
    }
    
    #endregion

    #region Black Screen Transition
    
    private void SetupBlackScreenOverlay()
    {
        // Eğer blackScreenOverlay atanmamışsa otomatik oluştur
        if (blackScreenOverlay == null)
        {
            // En üstte bir Canvas oluştur overlay için
            GameObject overlayCanvas = new GameObject("BlackScreenCanvas");
            Canvas canvas = overlayCanvas.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 999; // En üstte olsun
            
            // CanvasScaler ekle
            CanvasScaler scaler = overlayCanvas.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920, 1080);
            
            // Siyah image oluştur
            GameObject blackImageObj = new GameObject("BlackScreenOverlay");
            blackImageObj.transform.SetParent(overlayCanvas.transform, false);
            
            blackScreenOverlay = blackImageObj.AddComponent<Image>();
            blackScreenOverlay.color = new Color(0, 0, 0, 0); // Başlangıçta şeffaf
            
            // Tam ekran yap
            RectTransform rectTransform = blackScreenOverlay.rectTransform;
            rectTransform.anchorMin = Vector2.zero;
            rectTransform.anchorMax = Vector2.one;
            rectTransform.offsetMin = Vector2.zero;
            rectTransform.offsetMax = Vector2.zero;
            
            if (showDebugLogs)
                Debug.Log("Black screen overlay otomatik oluşturuldu!");
        }
        else
        {
            // Başlangıçta şeffaf yap
            blackScreenOverlay.color = new Color(0, 0, 0, 0);
        }
    }
    
    #endregion

    #region Audio Methods
    
    private void SetupMenuMusic()
    {
        // AudioSource yoksa otomatik olarak ekle
        if (menuMusicSource == null)
        {
            menuMusicSource = GetComponent<AudioSource>();
            if (menuMusicSource == null)
            {
                menuMusicSource = gameObject.AddComponent<AudioSource>();
            }
        }

        // Menü müziği ayarları
        if (menuMusic != null && menuMusicSource != null)
        {
            menuMusicSource.clip = menuMusic;
            menuMusicSource.loop = true;
            menuMusicSource.volume = 0.5f;
            menuMusicSource.Play();
            
            if (showDebugLogs)
                Debug.Log("Menü müziği başlatıldı!");
        }
        else
        {
            if (showDebugLogs)
                Debug.LogWarning("Menü müziği veya AudioSource atanmamış!");
        }
    }

    public void LaunchGame()
    {
        if (showDebugLogs)
            Debug.Log("LaunchGame metoduna tıklandı!");
        
        // Slideshow'u durdur
        StopSlideshow();
        
        // Play buton sesi çal
        PlayButtonSound();
        
        // Siyah ekran geçişi ile sahneyi yükle
        StartCoroutine(BlackScreenTransitionAndLoadScene());
    }

    private void PlayButtonSound()
    {
        if (playButtonSound != null)
        {
            // Geçici AudioSource oluştur play button sesi için
            GameObject tempAudio = new GameObject("TempButtonAudio");
            AudioSource tempSource = tempAudio.AddComponent<AudioSource>();
            tempSource.clip = playButtonSound;
            tempSource.volume = playButtonSoundVolume;
            tempSource.Play();
            
            // Ses bittikten sonra objeyi yok et
            Destroy(tempAudio, playButtonSound.length);
            
            if (showDebugLogs)
                Debug.Log("Play button sesi çalındı!");
        }
    }

    private IEnumerator BlackScreenTransitionAndLoadScene()
    {
        if (showDebugLogs)
            Debug.Log("Siyah ekran geçişi başlıyor...");

        // 1. Müziği fade out yap
        Coroutine musicFadeCoroutine = null;
        if (menuMusicSource != null && menuMusicSource.isPlaying)
        {
            musicFadeCoroutine = StartCoroutine(FadeOutMusic());
        }

        // 2. Siyah ekrana fade yap
        yield return StartCoroutine(FadeToBlack());

        // 3. Belirtilen süre boyunca siyah ekranda bekle
        if (showDebugLogs)
            Debug.Log($"Siyah ekranda {blackScreenDuration} saniye bekleniyor...");
        
        yield return new WaitForSeconds(blackScreenDuration);

        // 4. Müzik fade'inin bitmesini bekle
        if (musicFadeCoroutine != null)
        {
            yield return musicFadeCoroutine;
        }

        // 5. Sahneyi yükle
        LoadGameScene();
    }

    private IEnumerator FadeToBlack()
    {
        if (blackScreenOverlay == null)
        {
            if (showDebugLogs)
                Debug.LogWarning("Black screen overlay bulunamadı!");
            yield break;
        }

        float elapsedTime = 0;
        Color startColor = blackScreenOverlay.color;
        Color targetColor = new Color(0, 0, 0, 1); // Tam siyah

        while (elapsedTime < fadeToBlackDuration)
        {
            elapsedTime += Time.deltaTime;
            float progress = elapsedTime / fadeToBlackDuration;
            
            blackScreenOverlay.color = Color.Lerp(startColor, targetColor, progress);
            
            yield return null;
        }

        blackScreenOverlay.color = targetColor;
        
        if (showDebugLogs)
            Debug.Log("Siyah ekrana geçiş tamamlandı!");
    }

    private IEnumerator FadeOutMusic()
    {
        if (menuMusicSource == null || !menuMusicSource.isPlaying)
            yield break;

        float startVolume = menuMusicSource.volume;
        float currentTime = 0;

        while (currentTime < fadeOutDuration)
        {
            currentTime += Time.deltaTime;
            menuMusicSource.volume = Mathf.Lerp(startVolume, 0, currentTime / fadeOutDuration);
            yield return null;
        }

        menuMusicSource.Stop();
        if (showDebugLogs)
            Debug.Log("Menü müziği fade out ile kapatıldı!");
    }

private void LoadGameScene()
{
    // Sahne adını "City" olarak güncelleyin
    if (Application.CanStreamedLevelBeLoaded("City"))
    {
        if (showDebugLogs)
            Debug.Log("City sahnesine geçiliyor...");
        SceneManager.LoadScene("City");
    }
    else
    {
        Debug.LogError("City sahnesi bulunamadı! Build Settings'e eklenmiş mi kontrol edin.");
        SceneManager.LoadScene("SampleScene");
    }
}

    private IEnumerator FadeOutAndLoadScene()
    {
        if (menuMusicSource != null && menuMusicSource.isPlaying)
        {
            float startVolume = menuMusicSource.volume;
            float currentTime = 0;

            // Fade out
            while (currentTime < fadeOutDuration)
            {
                currentTime += Time.deltaTime;
                menuMusicSource.volume = Mathf.Lerp(startVolume, 0, currentTime / fadeOutDuration);
                yield return null;
            }

            menuMusicSource.Stop();
            if (showDebugLogs)
                Debug.Log("Menü müziği fade out ile kapatıldı!");
        }

        // Sahne yükleme
        if (Application.CanStreamedLevelBeLoaded("GameScene"))
        {
            if (showDebugLogs)
                Debug.Log("GameScene sahnesine geçiliyor...");
            SceneManager.LoadScene("GameScene");
        }
        else
        {
            Debug.LogError("GameScene bulunamadı! Build Settings'e eklenmiş mi kontrol edin.");
            SceneManager.LoadScene("SampleScene");
        }
    }

    public void ExitGame()
    {
        if (showDebugLogs)
            Debug.Log("ExitGame metoduna tıklandı!");
        
        // Slideshow'u durdur
        StopSlideshow();
        
        #if UNITY_EDITOR
            if (showDebugLogs)
                Debug.Log("Editor'da çalışıyor - oyun kapatılamaz.");
            UnityEditor.EditorApplication.isPlaying = false;
        #else
            if (showDebugLogs)
                Debug.Log("Oyun kapatılıyor...");
            Application.Quit();
        #endif
    }
    
    // Müzik kontrol metodları
    public void SetMusicVolume(float volume)
    {
        if (menuMusicSource != null)
        {
            menuMusicSource.volume = Mathf.Clamp01(volume);
        }
    }

    public void ToggleMusic()
    {
        if (menuMusicSource != null)
        {
            if (menuMusicSource.isPlaying)
            {
                menuMusicSource.Pause();
                if (showDebugLogs)
                    Debug.Log("Menü müziği duraklatıldı!");
            }
            else
            {
                menuMusicSource.UnPause();
                if (showDebugLogs)
                    Debug.Log("Menü müziği devam ediyor!");
            }
        }
    }

    public void StopMusic()
    {
        if (menuMusicSource != null)
        {
            menuMusicSource.Stop();
            if (showDebugLogs)
                Debug.Log("Menü müziği durduruldu!");
        }
    }

    #endregion

    #region Slideshow Methods

    private void SetupSlideshow()
    {
        // DisplayImage yoksa sahne içinde Image arayabilirsiniz
        if (displayImage == null)
        {
            displayImage = FindFirstObjectByType<Image>();
        }

        if (displayImage == null)
        {
            if (showDebugLogs)
                Debug.LogWarning("Slideshow için Image component bulunamadı!");
            return;
        }

        // Canvas Scaler ayarlarını kontrol et
        Canvas parentCanvas = displayImage.GetComponentInParent<Canvas>();
        if (parentCanvas != null)
        {
            CanvasScaler scaler = parentCanvas.GetComponent<CanvasScaler>();
            if (scaler == null)
            {
                scaler = parentCanvas.gameObject.AddComponent<CanvasScaler>();
            }
            
            // Tam ekran için Canvas Scaler ayarları
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920, 1080);
            scaler.screenMatchMode = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
            scaler.matchWidthOrHeight = 0.5f;
            
            // Canvas'ı render mode kontrol et
            parentCanvas.renderMode = RenderMode.ScreenSpaceOverlay;
            
            if (showDebugLogs)
                Debug.Log("Canvas Scaler ve RenderMode ayarlandı!");
        }

        // RectTransform'u parent'ın tam boyutuna ayarla
        RectTransform rectTransform = displayImage.GetComponent<RectTransform>();
        if (rectTransform != null)
        {
            // Anchor'ları tam ekran yap
            rectTransform.anchorMin = Vector2.zero;
            rectTransform.anchorMax = Vector2.one;
            
            // Offset'leri sıfırla
            rectTransform.offsetMin = Vector2.zero;
            rectTransform.offsetMax = Vector2.zero;
            
            // Position ve size'ı sıfırla
            rectTransform.anchoredPosition = Vector2.zero;
            rectTransform.sizeDelta = Vector2.zero;
            
            // Transform'u sıfırla
            rectTransform.localPosition = Vector3.zero;
            rectTransform.localScale = Vector3.one;
            
            if (showDebugLogs)
                Debug.Log($"DisplayImage RectTransform ayarlandı: {rectTransform.rect}");
        }

        // Image ayarları - ÖNEMLİ!
        displayImage.type = Image.Type.Simple;
        displayImage.preserveAspect = false; // Tam ekranı kaplasın
        displayImage.raycastTarget = false;

        // İlk resmi göster
        if (slideshowImages != null && slideshowImages.Length > 0)
        {
            displayImage.sprite = slideshowImages[0];
            displayImage.color = Color.white;
            
            if (showDebugLogs)
                Debug.Log($"İlk slideshow resmi yüklendi: {slideshowImages[0].name} - Image rect: {displayImage.rectTransform.rect}");
        }
        else
        {
            if (showDebugLogs)
                Debug.LogWarning("Slideshow resimleri atanmamış!");
        }
    }

    public void StartSlideshow()
    {
        if (slideshowImages == null || slideshowImages.Length <= 1)
        {
            if (showDebugLogs)
                Debug.LogWarning("Slideshow için en az 2 resim gerekli!");
            return;
        }

        if (displayImage == null)
        {
            if (showDebugLogs)
                Debug.LogWarning("Display Image atanmamış!");
            return;
        }

        if (!isSlideshowPlaying)
        {
            isSlideshowPlaying = true;
            slideshowCoroutine = StartCoroutine(SlideshowLoop());
            
            if (showDebugLogs)
                Debug.Log("Slideshow başlatıldı!");
        }
    }

    public void StopSlideshow()
    {
        if (isSlideshowPlaying)
        {
            isSlideshowPlaying = false;
            
            if (slideshowCoroutine != null)
            {
                StopCoroutine(slideshowCoroutine);
                slideshowCoroutine = null;
            }
            
            if (showDebugLogs)
                Debug.Log("Slideshow durduruldu!");
        }
    }

    private IEnumerator SlideshowLoop()
    {
        while (isSlideshowPlaying)
        {
            // Mevcut resmi göster
            yield return new WaitForSeconds(imageDuration);
            
            if (!isSlideshowPlaying) break;
            
            // Sonraki resme geç
            yield return StartCoroutine(FadeToNextImage());
            
            // Loop kontrolü
            if (!loopSlideshow && currentImageIndex >= slideshowImages.Length - 1)
            {
                isSlideshowPlaying = false;
                if (showDebugLogs)
                    Debug.Log("Slideshow tamamlandı (loop kapalı)!");
                break;
            }
        }
    }

    // FadeToNextImage metodunu tamamen değiştirin:

private IEnumerator FadeToNextImage()
{
    // Sonraki resim index'ini hesapla
    int nextIndex = (currentImageIndex + 1) % slideshowImages.Length;
    
    if (showDebugLogs)
        Debug.Log($"Overlay-fade: {slideshowImages[currentImageIndex].name} -> {slideshowImages[nextIndex].name}");

    // DisplayImage'ın aynı parent'ında overlay oluştur
    Transform parent = displayImage.transform.parent;
    if (parent == null)
    {
        if (showDebugLogs)
            Debug.LogWarning("DisplayImage parent bulunamadı!");
        yield break;
    }

    // İkinci bir Image oluştur geçiş için (aynı parent içinde)
    GameObject tempImageObj = new GameObject("OverlayFadeImage");
    tempImageObj.transform.SetParent(parent, false);
    
    Image overlayImage = tempImageObj.AddComponent<Image>();
    RectTransform overlayRect = overlayImage.rectTransform;
    RectTransform displayRect = displayImage.rectTransform;
    
    // Overlay'ı tam ekran yap
    overlayRect.anchorMin = Vector2.zero;
    overlayRect.anchorMax = Vector2.one;
    overlayRect.offsetMin = Vector2.zero;
    overlayRect.offsetMax = Vector2.zero;
    overlayRect.anchoredPosition = Vector2.zero;
    overlayRect.sizeDelta = Vector2.zero;
    overlayRect.localPosition = Vector3.zero;
    overlayRect.localScale = Vector3.one;
    
    // Overlay image ayarları - tam ekran için
    overlayImage.sprite = slideshowImages[nextIndex];
    overlayImage.type = Image.Type.Simple;
    overlayImage.preserveAspect = false; // Tam ekranı kaplasın
    overlayImage.raycastTarget = false;
    overlayImage.color = new Color(1f, 1f, 1f, 0f); // Başlangıçta şeffaf
    
    // Overlay'ı displayImage'ın hemen ÜSTÜNe koy
    overlayImage.transform.SetSiblingIndex(displayImage.transform.GetSiblingIndex() + 1);

    if (showDebugLogs)
        Debug.Log($"Overlay oluşturuldu. DisplayImage rect: {displayRect.rect}, Overlay rect: {overlayRect.rect}");

    // SADECE overlay image fade in olsun
    float elapsedTime = 0;
    
    while (elapsedTime < imageFadeDuration)
    {
        elapsedTime += Time.deltaTime;
        float progress = elapsedTime / imageFadeDuration;
        
        // Sadece yeni resim fade in olsun
        overlayImage.color = new Color(1f, 1f, 1f, Mathf.Lerp(0f, 1f, progress));
        
        yield return null;
    }

    // Fade tamamlandı - overlay tamamen opak
    overlayImage.color = Color.white;
    
    // Şimdi displayImage'ı yeni resimle güncelle ve overlay'i kaldır
    displayImage.sprite = slideshowImages[nextIndex];
    currentImageIndex = nextIndex;
    
    // Overlay'i yok et
    DestroyImmediate(tempImageObj);
    
    if (showDebugLogs)
        Debug.Log($"Fade tamamlandı. Yeni resim: {slideshowImages[nextIndex].name}");
}

private IEnumerator FadeToImage(int targetIndex)
{
    if (showDebugLogs)
        Debug.Log($"Manual overlay-fade to: {slideshowImages[targetIndex].name}");

    // DisplayImage'ın aynı parent'ında overlay oluştur
    Transform parent = displayImage.transform.parent;
    if (parent == null)
    {
        if (showDebugLogs)
            Debug.LogWarning("DisplayImage parent bulunamadı!");
        yield break;
    }

    // İkinci bir Image oluştur geçiş için (aynı parent içinde)
    GameObject tempImageObj = new GameObject("OverlayFadeImage");
    tempImageObj.transform.SetParent(parent, false);
    
    Image overlayImage = tempImageObj.AddComponent<Image>();
    RectTransform overlayRect = overlayImage.rectTransform;
    
    // Overlay'ı tam ekran yap
    overlayRect.anchorMin = Vector2.zero;
    overlayRect.anchorMax = Vector2.one;
    overlayRect.offsetMin = Vector2.zero;
    overlayRect.offsetMax = Vector2.zero;
    overlayRect.anchoredPosition = Vector2.zero;
    overlayRect.sizeDelta = Vector2.zero;
    overlayRect.localPosition = Vector3.zero;
    overlayRect.localScale = Vector3.one;
    
    // Overlay image ayarları - tam ekran için
    overlayImage.sprite = slideshowImages[targetIndex];
    overlayImage.type = Image.Type.Simple;
    overlayImage.preserveAspect = false; // Tam ekranı kaplasın
    overlayImage.raycastTarget = false;
    overlayImage.color = new Color(1f, 1f, 1f, 0f); // Başlangıçta şeffaf
    
    // Overlay'ı displayImage'ın hemen ÜSTÜNe koy
    overlayImage.transform.SetSiblingIndex(displayImage.transform.GetSiblingIndex() + 1);

    // SADECE overlay image fade in olsun
    float elapsedTime = 0;
    
    while (elapsedTime < imageFadeDuration)
    {
        elapsedTime += Time.deltaTime;
        float progress = elapsedTime / imageFadeDuration;
        
        // Sadece yeni resim fade in olsun
        overlayImage.color = new Color(1f, 1f, 1f, Mathf.Lerp(0f, 1f, progress));
        
        yield return null;
    }

    // Fade tamamlandı
    overlayImage.color = Color.white;
    
    // DisplayImage'ı güncelle ve overlay'i kaldır
    displayImage.sprite = slideshowImages[targetIndex];
    currentImageIndex = targetIndex;
    
    // Overlay objeyi yok et
    DestroyImmediate(tempImageObj);
}
    // Manuel slideshow kontrolleri
    public void NextImage()
    {
        if (slideshowImages != null && slideshowImages.Length > 1 && displayImage != null)
        {
            StopSlideshow();
            StartCoroutine(FadeToNextImage());
        }
    }

    public void PreviousImage()
    {
        if (slideshowImages != null && slideshowImages.Length > 1 && displayImage != null)
        {
            StopSlideshow();
            currentImageIndex = (currentImageIndex - 1 + slideshowImages.Length) % slideshowImages.Length;
            StartCoroutine(FadeToImage(currentImageIndex));
        }
    }


    #endregion

    // Component yok edildiğinde tüm işlemleri durdur
    private void OnDestroy()
    {
        StopSlideshow();
        
        if (menuMusicSource != null)
        {
            menuMusicSource.Stop();
        }
    }

    #region Debug Controls (Editor Only)
    
    [Header("Manual Controls (Runtime Only)")]
    [SerializeField] private bool testNextImage = false;
    [SerializeField] private bool testPreviousImage = false;
    [SerializeField] private bool testStartSlideshow = false;
    [SerializeField] private bool testStopSlideshow = false;
    [SerializeField] private bool testToggleMusic = false;

    private void OnValidate()
    {
        if (Application.isPlaying)
        {
            if (testNextImage) { testNextImage = false; NextImage(); }
            if (testPreviousImage) { testPreviousImage = false; PreviousImage(); }
            if (testStartSlideshow) { testStartSlideshow = false; StartSlideshow(); }
            if (testStopSlideshow) { testStopSlideshow = false; StopSlideshow(); }
            if (testToggleMusic) { testToggleMusic = false; ToggleMusic(); }
        }
    }
    
    #endregion
}