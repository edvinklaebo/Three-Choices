using UnityEngine;

public class CharacterSelectInput : MonoBehaviour
{
    [SerializeField] private CharacterSelectController _controller;

    private void Update()
    {
        if (_controller == null)
            return;

        if (Input.GetKeyDown(KeyCode.RightArrow)) _controller.Next();

        if (Input.GetKeyDown(KeyCode.LeftArrow)) _controller.Previous();

        if (Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.KeypadEnter)) _controller.Confirm();
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