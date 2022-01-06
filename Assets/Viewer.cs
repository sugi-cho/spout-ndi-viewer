using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UIElements;

using Klak.Spout;
using Klak.Ndi;
using UniRx;
public class Viewer : MonoBehaviour
{
    private void OnEnable()
    {
        var doc = GetComponent<UIDocument>();
        var root = doc.rootVisualElement;
        var bg = root.Q("BG");
        var unscaled = root.Q("Unscaled");
        var spoutDropdown = root.Q<DropdownField>("SpoutNames");
        var ndiDropdown = root.Q<DropdownField>("NdiNames");
        var scrollView = root.Q<ScrollView>();
        var scaleToggle = root.Q<Toggle>();

        var spoutReceiver = GetComponent<SpoutReceiver>();
        var ndiReceiver = GetComponent<NdiReceiver>();

        spoutDropdown.choices = SpoutManager.GetSourceNames().ToList(); ;
        spoutDropdown.RegisterValueChangedCallback(evt => spoutReceiver.sourceName = evt.newValue);
        spoutDropdown.RegisterCallback<FocusInEvent>(evt =>
        {
            SetTexture(spoutReceiver.receivedTexture);
            spoutDropdown.choices = SpoutManager.GetSourceNames().ToList();
        });

        ndiDropdown.choices = NdiFinder.sourceNames.ToList();
        ndiDropdown.RegisterValueChangedCallback(evt => ndiReceiver.ndiName = evt.newValue);
        ndiDropdown.RegisterCallback<FocusInEvent>(evt =>
        {
            SetTexture(ndiReceiver.texture);
            ndiDropdown.choices = NdiFinder.sourceNames.ToList();
        });

        scaleToggle.RegisterValueChangedCallback(evt => scrollView.style.display = evt.newValue ? DisplayStyle.None : DisplayStyle.Flex);

        spoutReceiver.ObserveEveryValueChanged(r => r.receivedTexture)
            .Subscribe(tex => SetTexture(tex)).AddTo(gameObject);
        ndiReceiver.ObserveEveryValueChanged(r => r.texture)
            .Subscribe(tex => SetTexture(tex)).AddTo(gameObject);

        void SetTexture(RenderTexture tex)
        {
            if (tex != null)
            {
                bg.style.backgroundImage = Background.FromRenderTexture(tex);
                unscaled.style.backgroundImage = Background.FromRenderTexture(tex);
                unscaled.style.width = tex.width;
                unscaled.style.height = tex.height;
                Screen.SetResolution(Screen.height * tex.width / tex.height, Screen.height, false);
                scaleToggle.value = true;
            }
        }
    }
}
