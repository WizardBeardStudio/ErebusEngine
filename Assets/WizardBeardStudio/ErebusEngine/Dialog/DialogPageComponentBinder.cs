using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace WizardBeardStudio.ErebusEngine.Dialog
{
    public class DialogPageComponentBinder : MonoBehaviour
    {
        [field: SerializeField] public DialogPage Page { get; private set; }
        [field: SerializeField] public Image Portrait { get; private set; }
        [field: SerializeField] public Image HeaderBg { get; private set; }
        [field: SerializeField] public TMP_Text TitleText { get; private set; }
        [field: SerializeField] public TMP_Text DialogText { get; private set; }
        [field: SerializeField] public GameObject NavButtonPrefab { get; private set; }
        [field: SerializeField] public GameObject NavButtonContainer { get; private set; }

        private bool _hasPage;
        
        private void Awake()
        {
            if (Page == null)
            {
                try
                {
                    Page = GetComponentInParent<DialogPage>();
                    if (Page != null)
                    {
                        _hasPage = true;
                    }
                }
                catch (Exception ex)
                {
                    Debug.LogError($"[Dialog Page Component Binder] Unable to find parent DialogPage component: {ex.Message}");
                }
            }
            else
            {
                _hasPage = true;
            }
        }

        private void OnEnable()
        {
            EnablePortrait();
            EnableHeaderBg();
            EnableTitleText();
            EnableDialogText();
            EnableNavButtonPrefab();
            EnableNavButtonContainer();
        }

        private void EnablePortrait()
        {
            if (Portrait != null && _hasPage)
            {
                Portrait.sprite = Page.Portrait;
            }
        }

        private void EnableHeaderBg()
        {
            if (HeaderBg != null && _hasPage)
            {
                HeaderBg.sprite = Page.HeaderBG;
            }
        }

        private void EnableTitleText()
        {
            if (TitleText != null && _hasPage)
            {
                TitleText.text = Page.Title;
            }
        }

        private void EnableDialogText()
        {
            if (DialogText != null && _hasPage)
            {
                DialogText.text = Page.DialogText;
            }
        }

        private void EnableNavButtonPrefab()
        {
            if (NavButtonPrefab != null && _hasPage)
            {
                NavButtonPrefab.SetActive(false);
            }
        }

        private void EnableNavButtonContainer()
        {
            if (NavButtonContainer != null && _hasPage)
            {
                NavButtonContainer.gameObject.SetActive(true);
            }
        }
    }
}