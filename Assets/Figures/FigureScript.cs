using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FigureScript : MonoBehaviour
{
    #region events
    public delegate void MDownHandler(GameObject gameObject);
    public event MDownHandler OnMDown;
    
    public delegate void MUpHandler(GameObject gameObject);
    public event MUpHandler OnMUp;
    
    public delegate void MDragHandler(GameObject gameObject, Vector2 mousePosition, Vector2 worldPosition, Vector3 figurePosition);
    public event MDragHandler OnMDrag;
    #endregion

    public Vector2 originalMousePosition, newMousePosition;
    public Vector2 originalWorldPosition, newWorldPosition;
    public Vector3 originalFigurePosition;

    public bool couldMove;

    // Color[] colors = new Color[6] { Color.blue, Color.red, Color.green, Color.yellow, Color.magenta, Color.grey };
    Color[] colors = new Color[6] { Color.blue, Color.red, Color.yellow, Color.white, Color.green, Color.magenta };

    void Awake()
    {
        Color blue = new Color32(50, 50, 220, 255);
        colors[0] = blue;
        Color red = new Color32(220, 50, 50, 255);
        colors[1] = red;
        Color yellow = new Color32(220, 220, 50, 255);
        colors[2] = yellow;
        Color white = new Color32(220, 220, 220, 255);
        colors[3] = white;
    }

    private void OnMouseDown()
    {
        if (!couldMove)
            return;

        originalMousePosition = Input.mousePosition;
        originalWorldPosition = Camera.main.ScreenToWorldPoint(originalMousePosition);
        originalFigurePosition = transform.position;

        // ScaleFigureAndChildren("zoom-in");
        if (OnMDown != null)
            OnMDown(gameObject);
    }

    private void OnMouseUp()
    {
        if (!couldMove)
            return;


        newMousePosition = Input.mousePosition;
        newWorldPosition = Camera.main.ScreenToWorldPoint(newMousePosition);

        if (OnMUp != null)
            OnMUp(gameObject);
    }

    private void OnMouseDrag()
    {
        if (!couldMove)
            return;

        Vector2 technoOffset = new Vector2(0f, 2f);

        newMousePosition = Input.mousePosition;
        newWorldPosition = Camera.main.ScreenToWorldPoint(newMousePosition);
        transform.position = (Vector2)newWorldPosition + technoOffset;

        if (OnMDrag != null)
            OnMDrag(gameObject, newMousePosition, newWorldPosition, transform.position);
    }

    public void ScaleFigureAndChildren(string action, List<int> figureColors)
    {
        Vector3 figureLocalScale = Vector3.zero;

        if (action.Equals("zoom-in"))
        {
            figureLocalScale.x = 1f;
            figureLocalScale.y = 1f;
            transform.localScale = figureLocalScale;

            figureLocalScale.x = 0.9f;
            figureLocalScale.y = 0.9f;
        }
        else if (action.Equals("zoom-out"))
        {
            figureLocalScale.x = 0.6f;
            figureLocalScale.y = 0.6f;
            transform.localScale = figureLocalScale;

            figureLocalScale.x = 1.05f;
            figureLocalScale.y = 1.05f;
        }

        for (int i = 0; i < transform.childCount; i++)
        {
            transform.GetChild(i).localScale = figureLocalScale;
            // transform.GetChild(i).GetComponent<SpriteRenderer>().color = new Color32(100, 100, 255, 255);
            transform.GetChild(i).GetComponent<SpriteRenderer>().color = colors[figureColors[i]] * 2f;
        }
    }
}

// RaycastHit2D[] figures =  Physics2D.RaycastAll(position, position, 0.5f);
// if (figures.Length == 0)
//     return null;
// else
//     return figures[0].transform;
