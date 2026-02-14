using System;
using UnityEngine;
using UnityEngine.UI;

public class HealthBar : MonoBehaviour {
    [SerializeField] Image fillImage;
    [SerializeField] float lerpSpeed = 20f;
    float fillAmountTarget;
    HealthController healthController;


    void Awake() {
        healthController = GetComponentInParent<HealthController>();
    }

    void Start() {
        healthController.OnHealthChange += fillAmount => fillAmountTarget = fillAmount;
        fillAmountTarget = fillImage.fillAmount;
    }

    void Update() {
        if (Mathf.Abs(fillAmountTarget - fillImage.fillAmount) > 0.001f) 
        {
            fillImage.fillAmount = Mathf.Lerp(fillImage.fillAmount, fillAmountTarget, Time.deltaTime * lerpSpeed);
        }

        transform.forward = Camera.main.transform.forward;
    }
}
