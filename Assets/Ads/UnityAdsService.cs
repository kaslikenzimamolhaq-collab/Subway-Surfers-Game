using UnityEngine;
using UnityEngine.Advertisements;

/// <summary>
/// Central Unity Ads integration for Baby Run.
/// Attach this component to any scene object, or let the runtime bootstrap it automatically.
/// </summary>
public sealed class UnityAdsService : MonoBehaviour, IUnityAdsInitializationListener, IUnityAdsLoadListener, IUnityAdsShowListener
{
    private const string AndroidGameId = "800374540";
    private const string RewardedPlacementId = "BP_Rewarded_Android";
    private const string InterstitialPlacementId = "BP_Interstitial_Android";
    private const string BannerPlacementId = "BP_Banner_Android";

    [SerializeField] private bool testMode = false;

    private static UnityAdsService instance;
    private bool rewardedLoaded;
    private bool interstitialLoaded;
    private bool bannerLoaded;
    private System.Action rewardedCompleted;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    private static void Bootstrap()
    {
        if (instance != null)
        {
            return;
        }

        var serviceObject = new GameObject("UnityAdsService");
        instance = serviceObject.AddComponent<UnityAdsService>();
        DontDestroyOnLoad(serviceObject);
    }

    private void Awake()
    {
        if (instance != null && instance != this)
        {
            Destroy(gameObject);
            return;
        }

        instance = this;
        DontDestroyOnLoad(gameObject);
        InitializeAds();
    }

    private void InitializeAds()
    {
#if UNITY_ANDROID || UNITY_EDITOR
        if (!Advertisement.isInitialized && Advertisement.isSupported)
        {
            Advertisement.Initialize(AndroidGameId, testMode, this);
        }
#else
        Debug.Log("Unity Ads is configured for Android (Game ID 800374540).");
#endif
    }

    public void OnInitializationComplete()
    {
        Debug.Log("Unity Ads initialization complete.");
        LoadRewardedAd();
        LoadInterstitialAd();
        LoadBannerAd();
    }

    public void OnInitializationFailed(UnityAdsInitializationError error, string message)
    {
        Debug.LogError($"Unity Ads initialization failed: {error} - {message}");
    }

    public void LoadRewardedAd()
    {
        rewardedLoaded = false;
        Advertisement.Load(RewardedPlacementId, this);
    }

    public void LoadInterstitialAd()
    {
        interstitialLoaded = false;
        Advertisement.Load(InterstitialPlacementId, this);
    }

    public void LoadBannerAd()
    {
        Advertisement.Banner.SetPosition(BannerPosition.BOTTOM_CENTER);
        var options = new BannerLoadOptions
        {
            loadCallback = () =>
            {
                bannerLoaded = true;
                Advertisement.Banner.Show(BannerPlacementId);
            },
            errorCallback = message => Debug.LogWarning($"Unity Ads banner failed to load: {message}")
        };
        Advertisement.Banner.Load(BannerPlacementId, options);
    }

    public bool ShowInterstitial()
    {
        if (!interstitialLoaded)
        {
            LoadInterstitialAd();
            return false;
        }

        interstitialLoaded = false;
        Advertisement.Show(InterstitialPlacementId, this);
        return true;
    }

    public bool ShowRewarded(System.Action onCompleted = null)
    {
        if (!rewardedLoaded)
        {
            LoadRewardedAd();
            return false;
        }

        rewardedCompleted = onCompleted;
        rewardedLoaded = false;
        Advertisement.Show(RewardedPlacementId, this);
        return true;
    }

    public void ShowBanner()
    {
        if (bannerLoaded)
        {
            Advertisement.Banner.Show(BannerPlacementId);
        }
    }

    public void HideBanner()
    {
        Advertisement.Banner.Hide();
    }

    public void OnUnityAdsAdLoaded(string adUnitId)
    {
        if (adUnitId == RewardedPlacementId)
        {
            rewardedLoaded = true;
        }
        else if (adUnitId == InterstitialPlacementId)
        {
            interstitialLoaded = true;
        }
    }

    public void OnUnityAdsFailedToLoad(string adUnitId, UnityAdsLoadError error, string message)
    {
        Debug.LogWarning($"Unity Ads failed to load {adUnitId}: {error} - {message}");
    }

    public void OnUnityAdsShowFailure(string adUnitId, UnityAdsShowError error, string message)
    {
        Debug.LogWarning($"Unity Ads failed to show {adUnitId}: {error} - {message}");
        if (adUnitId == RewardedPlacementId)
        {
            rewardedCompleted = null;
            LoadRewardedAd();
        }
        else if (adUnitId == InterstitialPlacementId)
        {
            LoadInterstitialAd();
        }
    }

    public void OnUnityAdsShowStart(string adUnitId) { }

    public void OnUnityAdsShowClick(string adUnitId) { }

    public void OnUnityAdsShowComplete(string adUnitId, UnityAdsShowCompletionState showCompletionState)
    {
        if (adUnitId == RewardedPlacementId && showCompletionState == UnityAdsShowCompletionState.COMPLETED)
        {
            var completion = rewardedCompleted;
            rewardedCompleted = null;
            completion?.Invoke();
            LoadRewardedAd();
        }
        else if (adUnitId == InterstitialPlacementId)
        {
            LoadInterstitialAd();
        }
    }
}
