using System;
using UnityEngine;
using GoogleMobileAds.Api;
using GoogleMobileAds.Ump.Api;

public class Ads : MonoBehaviour
{
    // Test ad unit IDs from Google AdMob
    private string _adsUnitbnr = "ca-app-pub-3940256099942544/6300978111";  // Test banner
    private string _adsUnitGcs = "ca-app-pub-3940256099942544/1033173712";  // Test interstitial
    private string _adsUnitOdl = "ca-app-pub-3940256099942544/5224354917";  // Test rewarded

    BannerView _bannerView;
    InterstitialAd _InterstitialAd;
    RewardedAd _rewardedAd;

    private void Start()
    {
        // Initialize the Google Mobile Ads SDK
        MobileAds.Initialize(initStatus =>
        {
            LoadAd();             // Load banner ad
            LoadInterstitialAd(); // Load interstitial ad
            LoadRewardedAd();     // Load rewarded ad
        });
    }

    // Load a banner ad
    public void LoadAd()
    {
        if (_bannerView == null)
        {
            CreateBannerView(); // Create banner view if not already created
        }

        var adRequest = new AdRequest();
        _bannerView.LoadAd(adRequest);
    }

    // Create a new banner view at the bottom of the screen
    public void CreateBannerView()
    {
        if (_bannerView != null)
        {
            DestroyAd(); // Destroy existing banner if any
        }

        _bannerView = new BannerView(_adsUnitbnr, AdSize.Banner, AdPosition.Bottom);
    }

    // Destroy the banner ad
    public void DestroyAd()
    {
        if (_bannerView != null)
        {
            _bannerView.Destroy();
            _bannerView = null;
        }
    }

    // Load a new interstitial ad
    public void LoadInterstitialAd()
    {
        if (_InterstitialAd != null)
        {
            _InterstitialAd.Destroy();
            _InterstitialAd = null;
        }

        var adRequest = new AdRequest();

        InterstitialAd.Load(_adsUnitGcs, adRequest, (InterstitialAd ad, LoadAdError error) =>
        {
            if (error != null || ad == null)
            {
                Debug.Log("Failed to load interstitial ad: " + error);
                return;
            }
            _InterstitialAd = ad;
        });
    }

    // Show the interstitial ad, and call onAdClosed when it's done or skipped
    public void ShowInterstitialAd(Action onAdClosed)
    {
        if (_InterstitialAd != null && _InterstitialAd.CanShowAd())
        {
            _InterstitialAd.OnAdFullScreenContentClosed += () =>
            {
                onAdClosed?.Invoke();
                LoadInterstitialAd(); // Reload after showing
            };

            _InterstitialAd.OnAdFullScreenContentFailed += (AdError error) =>
            {
                Debug.Log("Interstitial ad show failed: " + error);
                onAdClosed?.Invoke();
            };

            _InterstitialAd.Show();
        }
        else
        {
            Debug.Log("Interstitial ad is not ready.");
            onAdClosed?.Invoke(); // Continue even if ad isn't ready
        }
    }

    // Load a new rewarded ad
    public void LoadRewardedAd()
    {
        if (_rewardedAd != null)
        {
            _rewardedAd.Destroy();
            _rewardedAd = null;
        }

        var adRequest = new AdRequest();

        RewardedAd.Load(_adsUnitOdl, adRequest, (RewardedAd ad, LoadAdError error) =>
        {
            if (error != null || ad == null)
            {
                Debug.Log("Failed to load rewarded ad: " + error);
                return;
            }
            _rewardedAd = ad;
        });
    }

    // Show the rewarded ad, run onRewardEarned when the user earns the reward, and onAdClosed when the ad is closed
    public void ShowRewardedAd(Action onRewardEarned, Action onAdClosed = null)
    {
        const string rewardMsg = "Rewarded ad completed. Type: {0}, Amount: {1}";

        if (_rewardedAd != null && _rewardedAd.CanShowAd())
        {
            _rewardedAd.OnAdFullScreenContentClosed += () =>
            {
                onAdClosed?.Invoke();
                LoadRewardedAd(); // Reload after showing
            };

            _rewardedAd.Show((Reward reward) =>
            {
                Debug.Log(string.Format(rewardMsg, reward.Type, reward.Amount));
                onRewardEarned?.Invoke();
            });
        }
        else
        {
            Debug.Log("Rewarded ad is not ready.");
            onAdClosed?.Invoke(); // Continue even if ad isn't ready
        }
    }
}