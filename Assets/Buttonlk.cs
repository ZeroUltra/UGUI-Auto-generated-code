using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class Buttonlk:MonoBehaviour
{
	[SerializeField] Button m_Button;

	public Button Button {get { return m_Button;} } 



	#region 用于寻找控件,当控件丢失,点击脚本齿轮->Reset菜单可恢复,也可重新编写下面的路径代码
#if UNITY_EDITOR
	private void Reset()
	{
		m_Button=GetComponent<Button>();
	}
#endif
	#endregion
}