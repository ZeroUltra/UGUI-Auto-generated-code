using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class TestPanel:MonoBehaviour
{
	[SerializeField] RectTransform m_TestPanelBB;
	[SerializeField] Image m_Image;
	[SerializeField] Text m_Text;
	[SerializeField] RawImage m_RawImage;
	[SerializeField] Button m_Button;
	[SerializeField] Text m_Text1;

	public RectTransform TestPanelBB {get { return m_TestPanelBB;} } 
	public Image Image {get { return m_Image;} } 
	public Text Text {get { return m_Text;} } 
	public RawImage RawImage {get { return m_RawImage;} } 
	public Button Button {get { return m_Button;} } 
	public Text Text1 {get { return m_Text1;} } 

	private void Start()
	{
		m_Button.onClick.AddListener(m_Button_OnClick);
	}
	private void m_Button_OnClick()
	{

	}

	#region 用于寻找控件,当控件丢失,点击脚本齿轮->Reset菜单可恢复,也可重新编写下面的路径代码
#if UNITY_EDITOR
	private void Reset()
	{
		m_TestPanelBB=GetComponent<RectTransform>();
		m_Image=transform.Find("Image").GetComponent<Image>();
		m_Text=transform.Find("Text").GetComponent<Text>();
		m_RawImage=transform.Find("RawImage").GetComponent<RawImage>();
		m_Button=transform.Find("Button").GetComponent<Button>();
		m_Text1=transform.Find("Button/Text1").GetComponent<Text>();
	}
#endif
	#endregion
}