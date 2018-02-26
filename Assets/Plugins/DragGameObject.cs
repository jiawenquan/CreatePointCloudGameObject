using System.Collections;
using UnityEngine;
/// <summary>
/// 鼠标右键拖动物体的脚本 
/// 
/// 地形旋转的脚本也写在这里  
/// </summary>
public class DragGameObject : MonoBehaviour {

	// Use this for initialization
	void Start () {
        //StartCoroutine(OnMouseDown());
	}
	
	// Update is called once per frame
	void Update () {
        if (Input.GetMouseButton(0)) 
        {
            StartCoroutine(OnMouseDown());
        }
	}


    IEnumerator OnMouseDown()
    {
        //Debug.Log("12345");
        //将物体由世界坐标系转换为屏幕坐标系
        Vector3 screenSpace = Camera.main.WorldToScreenPoint(transform.position);//三维物体坐标转屏幕坐标
        //完成两个步骤 1.由于鼠标的坐标系是2维，需要转换成3维的世界坐标系 
        //    //             2.只有3维坐标情况下才能来计算鼠标位置与物理的距离，offset即是距离
        //将鼠标屏幕坐标转为三维坐标，再算出物体位置与鼠标之间的距离
        Vector3 offset = transform.position - Camera.main.ScreenToWorldPoint(new Vector3(Input.mousePosition.x, Input.mousePosition.y, screenSpace.z));
        while (Input.GetMouseButton(0))
        {
            //得到现在鼠标的2维坐标系位置
            Vector3 curScreenSpace = new Vector3(Input.mousePosition.x, Input.mousePosition.y, screenSpace.z);
            //将当前鼠标的2维位置转换成3维位置，再加上鼠标的移动量
            Vector3 curPosition = Camera.main.ScreenToWorldPoint(curScreenSpace) +offset;
            //curPosition就是物体应该的移动向量赋给transform的position属性
            transform.position = curPosition;
            yield return new WaitForFixedUpdate(); //这个很重要，循环执行 等待渲染完成
        }
    }
}
