using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class TestPanelCC:MonoBehaviour
{
	[SerializeField] RectTransform m_TestPanel;
	[SerializeField] Image m_Image;
	[SerializeField] Text m_Text;
	[SerializeField] RawImage m_RawImage;




	#region 用于寻找控件,当控件丢失,点击脚本齿轮->Reset菜单可恢复,也可重新编写下面的路径代码
#if UNITY_EDITOR
	private void Reset()
	{
		m_TestPanel=GetComponent<RectTransform>();
		m_Image=transform.Find("Image").GetComponent<Image>();
		m_Text=transform.Find("Text").GetComponent<Text>();
		m_RawImage=transform.Find("RawImage").GetComponent<RawImage>();
	}
#endif
	#endregion
}