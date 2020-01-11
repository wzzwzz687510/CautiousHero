using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Wing.RPGSystem
{
    [RequireComponent(typeof(Button))]
    public class ButtonSound : MonoBehaviour, IPointerClickHandler, IPointerEnterHandler
    {
        public AudioClip clickClip;
        public AudioClip highlightClip;
        public AudioSource source;

        private Button m_button;

        private void Start()
        {
            m_button = GetComponent<Button>();
            source = AudioManager.Instance.seSource;
        }

        public void OnPointerClick(PointerEventData eventData)
        {
            if (clickClip && m_button.interactable) source.PlayOneShot(clickClip);
        }

        public void OnPointerEnter(PointerEventData eventData)
        {
            if (highlightClip && m_button.interactable) source.PlayOneShot(highlightClip);
        }
    }
}

