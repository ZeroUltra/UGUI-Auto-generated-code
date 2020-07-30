using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class ToggleAA:MonoBehaviour
{
	[SerializeField] Toggle m_Toggle;
	[SerializeField] Image m_Background;
	[SerializeField] Image m_Checkmark;
	[SerializeField] Text m_Label;

	public Toggle Toggle {get { return m_Toggle;} } 
	public Image Background {get { return m_Background;} } 

	private void Start()
	{
		m_Toggle.onValueChanged.AddListener(m_Toggle_OnValueChanged);
	}
	private void m_Toggle_OnValueChanged(bool isOn)
	{

	}



	#region 用于寻找控件,当控件丢失,点击脚本齿轮->Reset菜单可恢复,也可重新编写下面的路径代码
#if UNITY_EDITOR
	private void Reset()
	{
		m_Toggle=GetComponent<Toggle>();
		m_Background=transform.Find("Background").GetComponent<Image>();
		m_Checkmark=transform.Find("Background/Checkmark").GetComponent<Image>();
		m_Label=transform.Find("Label").GetComponent<Text>();
	}
#endif
	#endregion
}