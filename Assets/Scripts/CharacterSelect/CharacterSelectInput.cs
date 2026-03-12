using System;

using UnityEngine;

namespace CharacterSelect
{
    public class CharacterSelectInput : MonoBehaviour
    {
        [Header("References")] [SerializeField]
        private CharacterSelectController _controller;

        private void Awake()
        {
            if (this._controller == null)
                throw new InvalidOperationException("CharacterSelectInput requires a CharacterSelectController.");
        }

        private void Update()
        {
            if (Input.GetKeyDown(KeyCode.RightArrow))
                OnNextClicked();

            if (Input.GetKeyDown(KeyCode.LeftArrow))
                OnPreviousClicked();

            if (Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.KeypadEnter))
                OnConfirmClicked();
        }

        private void OnNextClicked()
        {
            this._controller.Next();
        }

        private void OnPreviousClicked()
        {
            this._controller.Previous();
        }

        private void OnConfirmClicked()
        {
            this._controller.Confirm();
        }
    }
}