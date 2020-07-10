using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class TestPanel:MonoBehaviour
{
	[SerializeField] Image Image;
	[SerializeField] Text Text;
	[SerializeField] RawImage RawImage;
	[SerializeField] Button Button;
	[SerializeField] Toggle Toggle;
	[SerializeField] Slider Slider;
	[SerializeField] Scrollbar Scrollbar;
	[SerializeField] InputField InputField;
	[SerializeField] Dropdown Dropdown;
	[SerializeField] Image Scroll_View;

	private void Start()
	{

		Toggle.onValueChanged.AddListener(Toggle_OnValueChanged);
		Slider.onValueChanged.AddListener(Slider_OnValueChanged);
		Scrollbar.onValueChanged.AddListener(Scrollbar_OnValueChanged);
		InputField.onValueChanged.AddListener(InputField_OnValueChanged);
		Dropdown.onValueChanged.AddListener(Dropdown_OnValueChanged);
	}
	private void Button_OnClick()
	{

	}
	private void Toggle_OnValueChanged(bool isOn)
	{

	}
	private void Slider_OnValueChanged(float value)
	{

	}
	private void Scrollbar_OnValueChanged(float value)
	{

	}
	private void InputField_OnValueChanged(string arg)
	{

	}
	private void Dropdown_OnValueChanged(int index)
	{

	}



	#region 用于寻找控件,当控件丢失,点击脚本齿轮->Reset菜单可恢复,也可重新编写下面的路径代码
#if UNITY_EDITOR
	private void Reset()
	{
		Image=transform.Find("Image").GetComponent<Image>();
		Text=transform.Find("Text").GetComponent<Text>();
		RawImage=transform.Find("RawImage").GetComponent<RawImage>();
		Button=transform.Find("Button").GetComponent<Button>();
		Toggle=transform.Find("Toggle").GetComponent<Toggle>();
		Slider=transform.Find("Slider").GetComponent<Slider>();
		Scrollbar=transform.Find("Scrollbar").GetComponent<Scrollbar>();
		InputField=transform.Find("InputField").GetComponent<InputField>();
		Dropdown=transform.Find("Dropdown").GetComponent<Dropdown>();
		Scroll_View=transform.Find("Scroll View").GetComponent<Image>();
	}
#endif
	#endregion
}