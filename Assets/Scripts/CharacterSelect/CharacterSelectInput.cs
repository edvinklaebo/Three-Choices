using UnityEngine;

public class CharacterSelectInput : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private CharacterSelectController _controller;

    private void Update()
    {
        if (_controller == null)
            return;

        if (Input.GetKeyDown(KeyCode.RightArrow)) 
            OnNextClicked();

        if (Input.GetKeyDown(KeyCode.LeftArrow)) 
            OnPreviousClicked();

        if (Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.KeypadEnter)) 
            OnConfirmClicked();
    }

    // Button click handlers for UI
    public void OnNextClicked()
    {
        _controller?.Next();
    }

    public void OnPreviousClicked()
    {
        _controller?.Previous();
    }

    public void OnConfirmClicked()
    {
        _controller?.Confirm();
    }
}