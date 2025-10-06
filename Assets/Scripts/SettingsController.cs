using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.Localization.Settings;
using System.Collections;
using Firebase;
using Firebase.Storage;
using System.Threading.Tasks;
#if UNITY_ANDROID
using UnityEngine.Android;
#endif

public class SettingsController : MonoBehaviour
{
    [Header("UI References")]
    public TMP_Dropdown languageDropdown;
    public Toggle downloadToggle;
    public TextMeshProUGUI storageSizeText;
    public Button deleteButton;

    [Header("Firebase")]
    public string firebaseStoragePath = "ar_content_bundle";

    private FirebaseStorage storage;
    private string localContentPath;
    private long downloadedBytes = 0;

    private void Start()
    {
        // Setup language dropdown
        languageDropdown.onValueChanged.AddListener(OnLanguageChanged);
        // Setup download toggle
        downloadToggle.onValueChanged.AddListener(OnDownloadToggleChanged);
        // Setup delete button
        deleteButton.onClick.AddListener(DeleteDownloadedContent);

        // Set local path for AR content
        localContentPath = Application.persistentDataPath + "/ar_content.bundle";

        // Initialize Firebase
        InitializeFirebase();

        // Update UI
        UpdateStorageSizeUI();
        CheckPermissions();
    }

    private void InitializeFirebase()
    {
        storage = FirebaseStorage.DefaultInstance;
    }

    // Language selection
    private void OnLanguageChanged(int index)
    {
        // 0: English, 1: French, 2: Kinyarwanda
        StartCoroutine(SetLocale(index));
    }

    private IEnumerator SetLocale(int index)
    {
        yield return LocalizationSettings.InitializationOperation;
        LocalizationSettings.SelectedLocale = LocalizationSettings.AvailableLocales.Locales[index];
    }

    // Download AR content
    private void OnDownloadToggleChanged(bool isOn)
    {
        if (isOn)
        {
            DownloadARContent();
        }
        else
        {
            // Optionally, prompt user to delete or keep content
        }
    }

    private void DownloadARContent()
    {
        var storageRef = storage.GetReference(firebaseStoragePath);
        storageRef.GetFileAsync(localContentPath).ContinueWith(OnDownloadCompleted);
    }

    private void OnDownloadCompleted(Task task)
    {
        if (task.IsCompleted && !task.IsFaulted && !task.IsCanceled)
        {
            downloadedBytes = new System.IO.FileInfo(localContentPath).Length;
            UpdateStorageSizeUI();
        }
        else
        {
            Debug.LogError("Failed to download AR content: " + task.Exception);
        }
    }

    // Show storage size
    private void UpdateStorageSizeUI()
    {
        if (System.IO.File.Exists(localContentPath))
        {
            long size = new System.IO.FileInfo(localContentPath).Length;
            storageSizeText.text = $"Storage Used: {FormatBytes(size)}";
        }
        else
        {
            storageSizeText.text = "Storage Used: 0 B";
        }
    }

    private string FormatBytes(long bytes)
    {
        if (bytes >= 1073741824)
            return $"{bytes / 1073741824f:F2} GB";
        if (bytes >= 1048576)
            return $"{bytes / 1048576f:F2} MB";
        if (bytes >= 1024)
            return $"{bytes / 1024f:F2} KB";
        return $"{bytes} B";
    }

    // Delete downloaded content
    private void DeleteDownloadedContent()
    {
        if (System.IO.File.Exists(localContentPath))
        {
            System.IO.File.Delete(localContentPath);
            UpdateStorageSizeUI();
            downloadToggle.isOn = false;
        }
    }

    // Permissions
    private void CheckPermissions()
    {
#if UNITY_ANDROID
        if (!Permission.HasUserAuthorizedPermission(Permission.Camera))
            Permission.RequestUserPermission(Permission.Camera);
        if (!Permission.HasUserAuthorizedPermission(Permission.ExternalStorageWrite))
            Permission.RequestUserPermission(Permission.ExternalStorageWrite);
#elif UNITY_IOS
        // iOS permissions are handled via Info.plist and user prompts
#endif
    }
}
