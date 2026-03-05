using System;
using Player;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace DefaultNamespace {
    public class ReloadBar : MonoBehaviour {
        [SerializeField] Image fillImage;
        [SerializeField] Image backgroundImage;

        [SerializeField] TextMeshProUGUI ammoCounterText;
        
        [SerializeField] float lerpSpeed = 20f;
        
        float fillAmountTarget;
        WeaponController weaponController;
        
        void Awake() {
            weaponController = GetComponentInParent<WeaponController>();
        }

        void OnEnable() {
            if (weaponController != null)
            {
                weaponController.OnReloadProgressChanged += (fillAmount) => fillAmountTarget = fillAmount;
            }
        }

        void Start() {
            fillAmountTarget = 0f;
            fillImage.fillAmount = 0f;
        }

        void Update() {
            if (Mathf.Abs(fillAmountTarget - fillImage.fillAmount) > 0.001f) {
                fillImage.fillAmount = fillAmountTarget; //Mathf.Lerp(fillImage.fillAmount, fillAmountTarget, Time.deltaTime * lerpSpeed);
            }

            transform.forward = Camera.main.transform.forward;
            
            bool isReloading = fillAmountTarget > 0.01f || fillImage.fillAmount > 0.01f;
            ToggleImages(isReloading);

            ammoCounterText.text = weaponController.GetCurrentWeaponAmmoCounterText();
        }
        
        void ToggleImages(bool show) {
            // Only update the enabled state if it's different to save on performance
            if (fillImage != null && fillImage.enabled != show) 
                fillImage.enabled = show;
                
            if (backgroundImage != null && backgroundImage.enabled != show) 
                backgroundImage.enabled = show;
        }
        
        void OnDisable() {
            if (weaponController != null)
            {
                weaponController.OnReloadProgressChanged -= (fillAmount) => fillAmountTarget = fillAmount;
            }
        }
    }
}