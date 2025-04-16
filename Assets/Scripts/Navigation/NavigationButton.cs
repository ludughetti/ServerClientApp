using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using static Utils.Validator;

namespace Navigation
{
    public class NavigationButton : MonoBehaviour
    {
        [SerializeField] private TMP_Text text;

        private string _id;
        private Button _button;

        public event Action<string> OnClick;

        private void Awake()
        {
            _button = GetComponent<Button>();
            ValidateDependencies();
        }

        private void OnEnable()
        {
            _button.onClick.AddListener(HandleButtonClick);
        }

        private void OnDisable()
        {
            _button.onClick.RemoveListener(HandleButtonClick);
        }

        public void Setup(string id, Action<string> onClick)
        {
            _id = id;
            name = $"{id}_Btn";
            text.SetText(id);
            OnClick = onClick;
        }
        
        private void ValidateDependencies()
        {
            enabled = IsDependencyConfigured(name, "Button", _button) &&
                      IsDependencyConfigured(name, "Button Text", text);
        }

        private void HandleButtonClick()
        {
            OnClick?.Invoke(_id);
        }
    }
}
