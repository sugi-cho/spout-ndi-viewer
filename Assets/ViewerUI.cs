using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;
using UnityEngine.UIElements;

using Klak.Spout;
using Klak.Ndi;
using UniRx;
public class ViewerUI : MonoBehaviour
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
        var saveButton = root.Q<Button>();

        var spoutReceiver = GetComponent<SpoutReceiver>();
        var ndiReceiver = GetComponent<NdiReceiver>();
        var currentTex = (RenderTexture)null;

        spoutDropdown.choices = SpoutManager.GetSourceNames().ToList(); ;
        spoutDropdown.RegisterValueChangedCallback(evt => spoutReceiver.sourceName = evt.newValue);
        spoutDropdown.RegisterCallback<FocusInEvent>(evt =>
        {
            SetTexture(spoutReceiver.receivedTexture, spoutDropdown.value);
            spoutDropdown.choices = SpoutManager.GetSourceNames().ToList();
        });

        ndiDropdown.choices = NdiFinder.sourceNames.ToList();
        ndiDropdown.RegisterValueChangedCallback(evt => ndiReceiver.ndiName = evt.newValue);
        ndiDropdown.RegisterCallback<FocusInEvent>(evt =>
        {
            SetTexture(ndiReceiver.texture, ndiDropdown.value);
            ndiDropdown.choices = NdiFinder.sourceNames.ToList();
        });

        scaleToggle.RegisterValueChangedCallback(evt => scrollView.style.display = evt.newValue ? DisplayStyle.None : DisplayStyle.Flex);
        saveButton.clicked += SaveTexture;

        spoutReceiver.ObserveEveryValueChanged(r => r.receivedTexture)
            .Subscribe(tex => SetTexture(tex, spoutDropdown.value)).AddTo(gameObject);
        ndiReceiver.ObserveEveryValueChanged(r => r.texture)
            .Subscribe(tex => SetTexture(tex, ndiDropdown.value)).AddTo(gameObject);

        void SetTexture(RenderTexture tex, string texName = "")
        {
            currentTex = tex;
            if (tex != null)
            {
                bg.style.backgroundImage = Background.FromRenderTexture(tex);
                unscaled.style.backgroundImage = Background.FromRenderTexture(tex);
                unscaled.style.width = tex.width;
                unscaled.style.height = tex.height;
                Screen.SetResolution(Screen.height * tex.width / tex.height, Screen.height, false);
                scaleToggle.value = true;
                currentTex.name = texName;
            }
        }

        void SaveTexture()
        {
            if (currentTex != null)
            {
                var tmp = RenderTexture.active;

                var w = currentTex.width;
                var h = currentTex.height;
                var rt = new RenderTexture(w, h, 0);
                var tex = new Texture2D(w, h, TextureFormat.RGBA32, false);

                Graphics.Blit(currentTex, rt);
                RenderTexture.active = rt;
                tex.ReadPixels(new Rect(0, 0, w, h), 0, 0);
                var data = tex.EncodeToPNG();
                var now = System.DateTime.Now;
                var fileNmae = $"{currentTex.name}_{now.Year}_{now.Month:00}{now.Day:00}_{now.Hour:00}{now.Minute:00}.png";
                File.WriteAllBytesAsync(fileNmae, data);

                RenderTexture.active = tmp;
                rt.Release();
                Destroy(rt);
                Destroy(tex);
            }
        }
    }
}
