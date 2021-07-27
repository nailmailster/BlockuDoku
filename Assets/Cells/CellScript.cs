using UnityEngine;

//  коммент проверим в Git
public class CellScript : MonoBehaviour
{
    #region events
    public delegate void MDownHandler(GameObject gameObject, Vector2 mousePosition);
    public event MDownHandler OnMDown;
    
    public delegate void MUpHandler(GameObject gameObject, Vector2 mousePosition);
    public event MUpHandler OnMUp;
    
    public delegate void MDragHandler(GameObject gameObject, Vector2 mousePosition);
    public event MDragHandler OnMDrag;
    #endregion

    //  надо заполнять эти координаты при размещении фигуры на доске
    public Vector2Int coordinates;

    public SpriteRenderer spriteRenderer;

    private void Start()
    {
    }

    private void OnMouseDown()
    {
        if (OnMDown != null)
            OnMDown(gameObject, Input.mousePosition);
    }

    private void OnMouseDrag()
    {
        if (OnMDrag != null)
            OnMDrag(gameObject, Input.mousePosition);
    }

    private void OnMouseUp()
    {
        if (OnMUp != null)
            OnMUp(gameObject, Input.mousePosition);
    }
}
