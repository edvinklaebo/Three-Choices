using System;

using Characters;

using Events;

using UnityEngine;

namespace CharacterSelect
{
    public class CharacterSelectController : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private CharacterCollection _collection;
        [SerializeField] private CharacterSelectView _view;

        private CharacterSelectionModel _model;

        public int CurrentIndex => this._model.CurrentIndex;
    
        private void Awake()
        {
            if (this._collection == null)
                throw new InvalidOperationException(
                    $"CharacterSelectController requires a {nameof(CharacterCollection)} assigned in the inspector.");
            if (this._view == null)
                throw new InvalidOperationException(
                    $"CharacterSelectController requires a {nameof(CharacterSelectView)} assigned in the inspector.");

            this._model = new CharacterSelectionModel(this._collection.Characters);
        }

        private void OnEnable()
        {
            this._view.DisplayCharacter(this._model.Current);
        }

        public void Next()
        {
            this._model.Next();
            this._view.DisplayCharacter(this._model.Current);
        }

        public void Previous()
        {
            this._model.Previous();
            this._view.DisplayCharacter(this._model.Current);
        }

        public void Confirm()
        {
            GameEvents.CharacterSelected_Event?.Invoke(this._model.Current);
        }
    }
}