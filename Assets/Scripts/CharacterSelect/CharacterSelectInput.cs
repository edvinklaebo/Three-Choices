using System;
using UnityEngine;

public class CharacterSelectInput : MonoBehaviour
{
    [Header("References")] [SerializeField]
    private CharacterSelectController _controller;

    private void Awake()
    {
        if (_controller == null)
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
        _controller.Next();
    }

    private void OnPreviousClicked()
    {
        _controller.Previous();
    }

    private void OnConfirmClicked()
    {
        _controller.Confirm();
    }
}